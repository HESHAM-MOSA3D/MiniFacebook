using System;

namespace MiniFacebook.Models
{
    public class Friend
    {
        public int Id { get; set; }
        public string ?UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string ?FriendId { get; set; }
        public ApplicationUser ?FriendUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public FriendStatus Status { get; set; }
    }

    public enum FriendStatus
    {
        Pending,
        Accepted,
        Blocked
    }
}