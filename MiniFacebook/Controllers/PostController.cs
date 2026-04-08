using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniFacebook.Models;
using MiniFacebook.Services;
using MiniFacebook.SignalR;
using Microsoft.AspNetCore.SignalR;
using MiniFacebook.Models;
using MiniFacebook.Services;
using MiniFacebook.SignalR;

[Authorize]
public class PostController : Controller
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<SocialHub> _hubContext;
    private readonly IFileUploadService _fileUploadService;
    private readonly AppDbContext _context;

    public PostController(
        IPostService postService,
        ICommentService commentService,
        ILikeService likeService,
        UserManager<ApplicationUser> userManager,
        IHubContext<SocialHub> hubContext,
        IFileUploadService fileUploadService,
        AppDbContext context)
    {
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
        _userManager = userManager;
        _hubContext = hubContext;
        _fileUploadService = fileUploadService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Redirect("/login");

        var posts = _postService.GetFeedForUser(user.Id);
        ViewBag.CurrentUserId = user.Id;
        return View(posts);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var post = _postService.GetPostWithComments(id);
        if (post == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        ViewBag.CurrentUserId = user?.Id;

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string content, IFormFile image)
    {
        var user = await _userManager.GetUserAsync(User);
        string imageUrl = await _fileUploadService.UploadFileAsync(image, "uploads");

        _postService.CreatePost(content, imageUrl, user.Id);

        var post = new { userFullName = user.FullName, content, imageUrl, userId = user.Id };
        await _hubContext.Clients.All.SendAsync("ReceivePost", user.FullName, content, imageUrl);

        return Ok(new { success = true, message = "Post created successfully", post });
    }


    [HttpPost]
    public async Task<IActionResult> UpdatePost(int id, string content, IFormFile image, bool removeImage = false)
    {
        var user = await _userManager.GetUserAsync(User);
        string imageUrl = null;

        var existingPost = _postService.GetPostById(id);
        if (existingPost != null && existingPost.UserId == user.Id)
        {
            if (removeImage && !string.IsNullOrEmpty(existingPost.ImageUrl))
            {
                _fileUploadService.DeleteFile(existingPost.ImageUrl);
                imageUrl = null;
            }
            else if (image != null && image.Length > 0)
            {
                imageUrl = await _fileUploadService.UploadFileAsync(image, "uploads");
                if (!string.IsNullOrEmpty(existingPost.ImageUrl))
                {
                    _fileUploadService.DeleteFile(existingPost.ImageUrl);
                }
            }
            else
            {
                imageUrl = existingPost.ImageUrl;
            }

            _postService.UpdatePost(id, content, imageUrl, user.Id);
            await _hubContext.Clients.All.SendAsync("PostUpdated", id, content, imageUrl, user.FullName);
        }

        return Ok(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeletePost(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var post = _postService.GetPostWithComments(id);

        if (post != null && post.UserId == user.Id)
        {
            _fileUploadService.DeleteFile(post.ImageUrl);

            if (post.Comments != null)
            {
                foreach (var comment in post.Comments)
                {
                    _fileUploadService.DeleteFile(comment.ImageUrl);
                }
            }

            _postService.DeletePost(id, user.Id);
            await _hubContext.Clients.All.SendAsync("PostDeleted", id);

            return Ok(new { success = true, message = "Post deleted" });
        }

        return Unauthorized(new { success = false, message = "You are not the owner" });
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int postId, string content, IFormFile image)
    {
        var user = await _userManager.GetUserAsync(User);
        string imageUrl = await _fileUploadService.UploadFileAsync(image, "uploads");

        _commentService.AddComment(content, imageUrl, postId, user.Id);

        var comment = new { postId, userName = user.FullName, content, imageUrl, userId = user.Id };
        await _hubContext.Clients.All.SendAsync("ReceiveComment", postId, user.FullName, content, imageUrl);

        var post = _postService.GetPostById(postId);
        if (post != null && post.UserId != user.Id)
        {
            var notification = new Notification
            {
                ReceiverId = post.UserId,
                TriggerUserId = user.Id,
                PostId = postId,
                Type = NotificationType.Comment,
                Message = $"{user.UserName} commented on your post."
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.User(post.UserId).SendAsync("ReceiveNotification", notification.Message, $"/Post/Details/{postId}");
        }

        return Ok(comment);
    }
    [HttpPost]
    public async Task<IActionResult> DeleteComment(int commentId, int postId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var comment = _commentService.GetCommentById(commentId);
        var post = _postService.GetPostById(postId);

        if (comment == null || post == null)
            return NotFound(new { success = false, message = "Comment or post not found" });

        if (comment.UserId == currentUser.Id || post.UserId == currentUser.Id)
        {
            _fileUploadService.DeleteFile(comment.ImageUrl);

            _commentService.DeleteCommentById(commentId);

            await _hubContext.Clients.All.SendAsync("CommentDeleted", commentId, postId);

            return Ok(new { success = true });
        }

        return Forbid();
    }
    [HttpPost]
    public async Task<IActionResult> UpdateComment(int commentId, int postId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        var comment = _commentService.GetCommentById(commentId);
        if (comment != null && comment.UserId == user.Id)
        {
            _commentService.UpdateComment(commentId, content, user.Id);
            await _hubContext.Clients.All.SendAsync("CommentUpdated", commentId, postId, content);
            return Ok(new { success = true });
        }
        return Forbid();
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(int? postId, int? commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        var isLiked = _likeService.ToggleLike(user.Id, postId, commentId);
        var likesCount = _likeService.GetLikesCount(postId, commentId);

        await _hubContext.Clients.All.SendAsync("LikeToggled", postId, commentId, user.FullName, isLiked, likesCount);

        if (isLiked && postId.HasValue)
        {
            var post = _postService.GetPostById(postId.Value);
            if (post != null && post.UserId != user.Id)
            {
                var likeCount = _likeService.GetLikesCount(postId, null);
                string msg = likeCount > 1 
                    ? $"{user.UserName} and {likeCount - 1} others liked your post." 
                    : $"{user.UserName} liked your post.";

                var notification = new Notification
                {
                    ReceiverId = post.UserId,
                    TriggerUserId = user.Id,
                    PostId = postId,
                    Type = NotificationType.Like,
                    Message = msg
                };
                
                var oldNotif = _context.Notifications.FirstOrDefault(n => n.ReceiverId == post.UserId && n.PostId == postId && n.Type == NotificationType.Like && !n.IsRead);
                if (oldNotif != null) _context.Notifications.Remove(oldNotif);
                
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.User(post.UserId).SendAsync("ReceiveNotification", notification.Message, $"/Post/Details/{postId.Value}");
            }
        }

        return Ok(new { isLiked, likesCount });
    }

    [HttpGet]
    public IActionResult GetUserPosts(string userId)
    {
        var posts = _postService.GetUserPosts(userId);

        var result = posts.Select(p => new
        {
            p.Id,
            p.Content,
            p.ImageUrl,
            p.CreatedAt,
            UserFullName = p.User.FullName,
            p.UserId,
            ProfilePicture = string.IsNullOrEmpty(p.User.ProfilePicture) ? "/images/default-avatar.jpeg" : p.User.ProfilePicture
        }).ToList();

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostLikes(int postId)
    {
        var likes = await _context.Likes
            .Include(l => l.User)
            .Where(l => l.PostId == postId)
            .Select(l => new {
                username = l.User.UserName,
                fullName = l.User.FullName,
                profilePicture = string.IsNullOrEmpty(l.User.ProfilePicture) ? "/images/default-avatar.jpeg" : l.User.ProfilePicture
            }).ToListAsync();

        return Json(likes);
    }
}