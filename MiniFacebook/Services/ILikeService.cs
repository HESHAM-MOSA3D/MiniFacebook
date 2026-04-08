namespace MiniFacebook.Services
{
    public interface ILikeService
    {
        bool ToggleLike(string userId, int? postId, int? commentId);
        int GetLikesCount(int? postId, int? commentId);
        List<string> GetLikesUsers(int? postId, int? commentId);
        bool IsLikedByUser(string userId, int? postId, int? commentId);
    }
}