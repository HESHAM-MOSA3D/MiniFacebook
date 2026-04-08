using MiniFacebook.Models;

namespace MiniFacebook.Services
{
    public interface ICommentService
    {
        void AddComment(string content, string imageUrl, int postId, string userId);
        void UpdateComment(int commentId, string content, string userId);
        void DeleteComment(int commentId, string currentUserId, bool isPostOwner = false); bool IsUserOwnerOfComment(int commentId, string userId);
        void DeleteCommentById(int commentId);
        Comment GetCommentById(int commentId);
    }
}