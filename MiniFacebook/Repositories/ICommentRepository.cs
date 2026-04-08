using MiniFacebook.Models;

namespace MiniFacebook.Repositories
{
    public interface ICommentRepository
    {
        void Add(Comment comment);
        void Update(Comment comment);
        void Delete(int id);
        Comment GetById(int id);
        void Save();
    }
}