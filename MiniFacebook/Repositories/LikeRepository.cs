using System.Linq;
using MiniFacebook.Models;

namespace MiniFacebook.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool IsLiked(string userId, int? postId, int? commentId)
        {
            return _context.Likes.Any(l => l.UserId == userId && l.PostId == postId && l.CommentId == commentId);
        }

        public void Add(Like like)
        {
            _context.Likes.Add(like);
        }

        public void Remove(string userId, int? postId, int? commentId)
        {
            var like = _context.Likes.FirstOrDefault(l => l.UserId == userId && l.PostId == postId && l.CommentId == commentId);
            if (like != null)
            {
                _context.Likes.Remove(like);
            }
        }

        public int GetPostLikesCount(int postId)
        {
            return _context.Likes.Count(l => l.PostId == postId);
        }

        public int GetCommentLikesCount(int commentId)
        {
            return _context.Likes.Count(l => l.CommentId == commentId);
        }

        public List<ApplicationUser> GetPostLikesUsers(int postId)
        {
            return _context.Likes
                .Where(l => l.PostId == postId)
                .Select(l => l.User)
                .ToList();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}