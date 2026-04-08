using MiniFacebook.Models;

namespace MiniFacebook.Repositories
{
    public interface ILikeRepository
    {
        bool IsLiked(string userId, int? postId, int? commentId);
        void Add(Like like);
        void Remove(string userId, int? postId, int? commentId);
        int GetPostLikesCount(int postId);
        int GetCommentLikesCount(int commentId);
        List<ApplicationUser> GetPostLikesUsers(int postId);
        void Save();
    }
}