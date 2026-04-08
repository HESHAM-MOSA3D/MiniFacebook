using MiniFacebook.Models;
using MiniFacebook.Repositories;

namespace MiniFacebook.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;

        public CommentService(ICommentRepository repo)
        {
            _repo = repo;
        }

        public void AddComment(string content, string imageUrl, int postId, string userId)
        {
            var comment = new Comment
            {
                Content = content,
                ImageUrl = imageUrl,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _repo.Add(comment);
            _repo.Save();
        }

        public void UpdateComment(int commentId, string content, string userId)
        {
            var comment = _repo.GetById(commentId);
            if (comment != null && comment.UserId == userId)
            {
                comment.Content = content;
                _repo.Update(comment);
                _repo.Save();
            }
        }

        public void DeleteComment(int commentId, string currentUserId, bool isPostOwner = false)
        {
            var comment = _repo.GetById(commentId);
            if (comment != null && (comment.UserId == currentUserId || isPostOwner))
            {
                _repo.Delete(commentId);
                _repo.Save();
            }
        }
        public void DeleteCommentById(int commentId)
        {
            _repo.Delete(commentId);
            _repo.Save();
        }
        public Comment GetCommentById(int commentId)
        {
            return _repo.GetById(commentId);
        }

        public bool IsUserOwnerOfComment(int commentId, string userId)
        {
            var comment = _repo.GetById(commentId);
            return comment != null && comment.UserId == userId;
        }
    }
}