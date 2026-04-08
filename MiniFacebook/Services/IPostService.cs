using MiniFacebook.Models;
using System.Collections.Generic;

namespace MiniFacebook.Services
{
    public interface IPostService
    {
        List<Post> GetAllPosts();
        List<Post> GetUserPosts(string userId);
        List<Post> GetFeedForUser(string userId);
        Post GetPostById(int id);
        Post GetPostWithComments(int id); 
        void CreatePost(string content, string imageUrl, string userId);
        void UpdatePost(int id, string content, string imageUrl, string userId);
        void DeletePost(int id, string userId);
        bool IsUserOwnerOfPost(int postId, string userId);
    }
}