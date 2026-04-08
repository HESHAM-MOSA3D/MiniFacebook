namespace MiniFacebook.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string ?Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ?UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<Comment> ?Comments { get; set; }
        public ICollection<Like> ?Likes { get; set; }
    }
}