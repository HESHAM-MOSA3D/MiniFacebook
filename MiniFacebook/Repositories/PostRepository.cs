using MiniFacebook.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MiniFacebook.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Post> GetAll()
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public List<Post> GetUserPosts(string userId)
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public List<Post> GetFeedForUser(string userId)
        {
            var followingIds = _context.Friends
                .Where(f => f.UserId == userId && f.Status == FriendStatus.Accepted)
                .Select(f => f.FriendId)
                .ToList();
                
            followingIds.Add(userId);

            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public Post GetById(int id)
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .FirstOrDefault(p => p.Id == id)!;
        }

        public Post GetPostWithComments(int id)
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(Post post)
        {
            _context.Posts.Add(post);
        }

        public void Update(Post post)
        {
            _context.Posts.Update(post);
        }

        public void Delete(int id)
        {
            var post = _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefault(p => p.Id == id);
                
            if (post != null)
            {
                var commentIds = post.Comments.Select(c => c.Id).ToList();
                var likes = _context.Likes.Where(l => l.PostId == id || (l.CommentId.HasValue && commentIds.Contains(l.CommentId.Value)));
                _context.Likes.RemoveRange(likes);

                var notifications = _context.Notifications.Where(n => n.PostId == id);
                _context.Notifications.RemoveRange(notifications);

                _context.Posts.Remove(post);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}