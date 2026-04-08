using MiniFacebook.Models;
using System.Collections.Generic;

namespace MiniFacebook.Repositories
{
    public interface IPostRepository
    {
        List<Post> GetAll();
        List<Post> GetUserPosts(string userId);
        List<Post> GetFeedForUser(string userId);
        Post GetById(int id);
        Post GetPostWithComments(int id);   
        void Add(Post post);
        void Update(Post post);
        void Delete(int id);
        void Save();
    }
}