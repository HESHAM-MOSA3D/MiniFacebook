using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniFacebook.Models
{
    public enum NotificationType
    {
        Like,
        Comment,
        Follow
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser Receiver { get; set; }

        [Required]
        public string TriggerUserId { get; set; }
        [ForeignKey("TriggerUserId")]
        public virtual ApplicationUser TriggerUser { get; set; }

        public int? PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        public NotificationType Type { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
