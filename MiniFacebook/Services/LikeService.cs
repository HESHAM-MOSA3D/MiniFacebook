using MiniFacebook.Models;
using MiniFacebook.Repositories;

namespace MiniFacebook.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _repo;

        public LikeService(ILikeRepository repo)
        {
            _repo = repo;
        }

        public bool ToggleLike(string userId, int? postId, int? commentId)
        {
            if (_repo.IsLiked(userId, postId, commentId))
            {
                _repo.Remove(userId, postId, commentId);
                _repo.Save();
                return false;
            }
            else
            {
                var like = new Like
                {
                    UserId = userId,
                    PostId = postId,
                    CommentId = commentId,
                    CreatedAt = DateTime.Now
                };
                _repo.Add(like);
                _repo.Save();
                return true;
            }
        }

        public int GetLikesCount(int? postId, int? commentId)
        {
            if (postId.HasValue)
                return _repo.GetPostLikesCount(postId.Value);
            if (commentId.HasValue)
                return _repo.GetCommentLikesCount(commentId.Value);
            return 0;
        }

        public List<string> GetLikesUsers(int? postId, int? commentId)
        {
            var users = _repo.GetPostLikesUsers(postId ?? 0);
            return users.Select(u => u.FullName).ToList();
        }

        public bool IsLikedByUser(string userId, int? postId, int? commentId)
        {
            return _repo.IsLiked(userId, postId, commentId);
        }
    }
}