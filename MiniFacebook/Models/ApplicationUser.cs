using Microsoft.AspNetCore.Identity;

namespace MiniFacebook.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = "/images/default-avatar.jpeg";
        public string CoverPhoto { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public List<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Friend> Friends { get; set; } = new List<Friend>();
        public ICollection<Friend> FriendOf { get; set; } = new List<Friend>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}