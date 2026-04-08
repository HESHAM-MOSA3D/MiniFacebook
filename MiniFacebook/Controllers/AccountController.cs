using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniFacebook.Models;
using MiniFacebook.Services;
using MiniFacebook.ViewModels;
using MiniFacebook.SignalR;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPostService _postService;
    private readonly IFileUploadService _fileUploadService;
    private readonly AppDbContext _context;
    private readonly IHubContext<SocialHub> _hubContext;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPostService postService,
        IFileUploadService fileUploadService,
        AppDbContext context,
        IHubContext<SocialHub> hubContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _postService = postService;
        _fileUploadService = fileUploadService;
        _context = context;
        _hubContext = hubContext;
    }

    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
            return Redirect("/index");
            
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var existingUser = await _userManager.FindByNameAsync(vm.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "This username is taken");
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.UserName,
            FullName = vm.FullName,
            ProfilePicture = "/images/default-avatar.png"
        };

        var result = await _userManager.CreateAsync(user, vm.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Redirect($"/profile/{user.UserName}");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(vm);
    }

    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity.IsAuthenticated)
            return Redirect("/index");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM vm, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _signInManager.PasswordSignInAsync(vm.UserName, vm.Password, false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/index");
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(vm);
    }
    [HttpGet]
public async Task<IActionResult> GetAllUsers()
{
    var users = _userManager.Users.ToList();
    var currentUser = await _userManager.GetUserAsync(User);
    ViewBag.CurrentUserId = currentUser.Id;
    return View(users);
}
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string username)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        ApplicationUser profileUser;

        if (string.IsNullOrEmpty(username))
        {
            if (currentUser == null)
                return Redirect("/login");
            profileUser = currentUser;
        }
        else
        {
            profileUser = _userManager.Users.FirstOrDefault(u => u.UserName == username);
            if (profileUser == null)
                return NotFound();
        }

        bool isOwner = currentUser != null && currentUser.Id == profileUser.Id;
        bool isGuest = currentUser == null;
        bool isFollowing = false;

        if (!isGuest && !isOwner)
        {
            isFollowing = _context.Friends.Any(f => f.UserId == currentUser.Id && f.FriendId == profileUser.Id && f.Status == FriendStatus.Accepted);
        }

        var userPosts = _postService.GetUserPosts(profileUser.Id);

        ViewBag.IsOwner = isOwner;
        ViewBag.IsGuest = isGuest;
        ViewBag.IsFollowing = isFollowing;
        ViewBag.ProfileUser = profileUser;
        ViewBag.CurrentUserImage = currentUser?.ProfilePicture ?? "/images/default-avatar.jpeg";

        return View(userPosts);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(string fullName, string userName, string newPassword, string bio, IFormFile profilePicture)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "User not found" });

        if (!string.IsNullOrEmpty(fullName))
            user.FullName = fullName;

        if (!string.IsNullOrEmpty(bio))
            user.Bio = bio;

        if (!string.IsNullOrEmpty(userName) && userName != user.UserName)
        {
            var userExists = await _userManager.FindByNameAsync(userName);
            if (userExists != null && userExists.Id != user.Id)
            {
                return Json(new { success = false, message = "Username is already taken" });
            }
            user.UserName = userName;
        }

        if (!string.IsNullOrEmpty(newPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!passwordResult.Succeeded)
            {
                return Json(new { success = false, message = string.Join(", ", passwordResult.Errors.Select(e => e.Description)) });
            }
        }

        if (profilePicture != null && profilePicture.Length > 0)
        {
            if (!string.IsNullOrEmpty(user.ProfilePicture) && 
                !user.ProfilePicture.Contains("default-avatar"))
            {
                _fileUploadService.DeleteFile(user.ProfilePicture);
            }
            user.ProfilePicture = await _fileUploadService.UploadFileAsync(profilePicture, "uploads");
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Json(new { success = true, message = "Profile updated successfully" });

        return Json(new { success = false, message = "Failed to update profile" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "User not found" });

   
        try 
        {
            var userFriends = _context.Friends.Where(f => f.UserId == user.Id || f.FriendId == user.Id);
            _context.Friends.RemoveRange(userFriends);

            var userLikes = _context.Likes.Where(l => l.UserId == user.Id);
            _context.Likes.RemoveRange(userLikes);

            var userPosts = _context.Posts.Where(p => p.UserId == user.Id).ToList();
            var userPostIds = userPosts.Select(p => p.Id).ToList();

            var likesOnUserPosts = _context.Likes.Where(l => l.PostId != null && userPostIds.Contains(l.PostId.Value));
            _context.Likes.RemoveRange(likesOnUserPosts);

            var notifsOnUserPosts = _context.Notifications.Where(n => n.PostId != null && userPostIds.Contains(n.PostId.Value));
            _context.Notifications.RemoveRange(notifsOnUserPosts);

            var userTriggers = _context.Notifications.Where(n => n.TriggerUserId == user.Id);
            _context.Notifications.RemoveRange(userTriggers);

            var userComments = _context.Comments.Where(c => c.UserId == user.Id);
            foreach(var c in userComments) 
            {
                if (!string.IsNullOrEmpty(c.ImageUrl)) _fileUploadService.DeleteFile(c.ImageUrl);
            }
            _context.Comments.RemoveRange(userComments);

            var commentsOnUserPosts = _context.Comments.Where(c => userPostIds.Contains(c.PostId));
            foreach(var c in commentsOnUserPosts)
            {
                if (!string.IsNullOrEmpty(c.ImageUrl) && c.UserId != user.Id) 
                    _fileUploadService.DeleteFile(c.ImageUrl);
            }

            foreach(var p in userPosts)
            {
                if (!string.IsNullOrEmpty(p.ImageUrl)) _fileUploadService.DeleteFile(p.ImageUrl);
            }
            _context.Posts.RemoveRange(userPosts);

            if (!string.IsNullOrEmpty(user.ProfilePicture) && !user.ProfilePicture.Contains("default-avatar"))
            {
                _fileUploadService.DeleteFile(user.ProfilePicture);
            }

            await _context.SaveChangesAsync();
        } 
        catch (Exception ex) 
        {
            return Json(new { success = false, message = "Database cleanup failed: " + ex.Message });
        }

        await _signInManager.SignOutAsync();
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
            return Json(new { success = true });

        return Json(new { success = false, message = "Failed to finalize account deletion" });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFollow(string username)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var targetUser = _userManager.Users.FirstOrDefault(u => u.UserName == username);
        if (targetUser == null || targetUser.Id == currentUser.Id) return BadRequest();

        var followRecord = _context.Friends.FirstOrDefault(f => f.UserId == currentUser.Id && f.FriendId == targetUser.Id);
        bool isNowFollowing = false;

        if (followRecord != null)
        {
            _context.Friends.Remove(followRecord);
        }
        else
        {
            _context.Friends.Add(new Friend { UserId = currentUser.Id, FriendId = targetUser.Id, Status = FriendStatus.Accepted });
            isNowFollowing = true;

            var notification = new Notification
            {
                ReceiverId = targetUser.Id,
                TriggerUserId = currentUser.Id,
                Type = NotificationType.Follow,
                Message = $"{currentUser.UserName} followed you"
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.User(targetUser.Id).SendAsync("ReceiveNotification", notification.Message, $"/profile/{currentUser.UserName}");
        }

        if (!isNowFollowing) 
        {
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, isFollowing = isNowFollowing });
    }

    [HttpGet]
    public async Task<IActionResult> GetFollowers(string username)
    {
        var targetUser = await _userManager.FindByNameAsync(username);
        if (targetUser == null) return NotFound();

        var followers = await _context.Friends
            .Include(f => f.User)
            .Where(f => f.FriendId == targetUser.Id && f.Status == FriendStatus.Accepted)
            .Select(f => new {
                username = f.User.UserName,
                fullName = f.User.FullName,
                profilePicture = string.IsNullOrEmpty(f.User.ProfilePicture) ? "/images/default-avatar.jpeg" : f.User.ProfilePicture
            }).ToListAsync();

        return Json(followers);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFollower(string followerUsername)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var followerUser = await _userManager.FindByNameAsync(followerUsername);
        if (followerUser == null) return NotFound();

        var followRecord = await _context.Friends.FirstOrDefaultAsync(f => f.UserId == followerUser.Id && f.FriendId == currentUser.Id);
        if (followRecord != null)
        {
            _context.Friends.Remove(followRecord);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var notifications = await _context.Notifications
            .Where(n => n.ReceiverId == currentUser.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new {
                n.Id,
                n.Message,
                n.IsRead,
                n.CreatedAt,
                url = n.Type == NotificationType.Follow ? $"/profile/{n.TriggerUser.UserName}" : $"/Post/Details/{n.PostId}"
            })
            .ToListAsync();

        return Json(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> ClearNotifications()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var unread = await _context.Notifications
            .Where(n => n.ReceiverId == currentUser.Id && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread) n.IsRead = true;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Json(new object[] { });

        query = query.ToLower();
        var results = _userManager.Users
            .Where(u => u.UserName.ToLower().Contains(query) || u.FullName.ToLower().Contains(query))
            .Take(6)
            .Select(u => new
            {
                username = u.UserName,
                fullName = u.FullName,
                profilePicture = string.IsNullOrEmpty(u.ProfilePicture) ? "/images/default-avatar.jpeg" : u.ProfilePicture
            })
            .ToList();

        return Json(results);
    }
}