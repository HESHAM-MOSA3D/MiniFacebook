using MiniFacebook.Models;
using MiniFacebook.Repositories;

namespace MiniFacebook.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;

        public PostService(IPostRepository repo)
        {
            _repo = repo;
        }

        public List<Post> GetAllPosts()
        {
            return _repo.GetAll();
        }

        public List<Post> GetUserPosts(string userId)
        {
            return _repo.GetUserPosts(userId);
        }

        public List<Post> GetFeedForUser(string userId)
        {
            return _repo.GetFeedForUser(userId);
        }

        public Post GetPostById(int id)
        {
            return _repo.GetById(id);
        }

        public Post GetPostWithComments(int id)
        {
            return _repo.GetPostWithComments(id);
        }

        public void CreatePost(string content, string imageUrl, string userId)
        {
            var post = new Post
            {
                Content = content,
                ImageUrl = imageUrl,
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            _repo.Add(post);
            _repo.Save();
        }

        public void UpdatePost(int id, string content, string imageUrl, string userId)
        {
            var post = _repo.GetById(id);
            if (post != null && post.UserId == userId)
            {
                post.Content = content;
                post.ImageUrl = imageUrl;
                _repo.Update(post);
                _repo.Save();
            }
        }

        public void DeletePost(int id, string userId)
        {
            var post = _repo.GetById(id);
            if (post != null && post.UserId == userId)
            {
                _repo.Delete(id);
                _repo.Save();
            }
        }

        public bool IsUserOwnerOfPost(int postId, string userId)
        {
            var post = _repo.GetById(postId);
            return post != null && post.UserId == userId;
        }
    }
}