using Microsoft.EntityFrameworkCore;
using MiniFacebook.Models;

namespace MiniFacebook.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
        }

        public void Delete(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }
        }

        public Comment GetById(int id)
        {
            return _context.Comments
                .Include(c => c.User)
                .FirstOrDefault(c => c.Id == id);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}