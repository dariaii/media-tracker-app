using Microsoft.EntityFrameworkCore;

namespace MediaTracker.Data
{
    public interface IRepository
    {
        IQueryable<T> Set<T>() where T : class;

        IQueryable<T> SetNoTracking<T>() where T : class;

        IQueryable<T> SetNoTracking<T>(params string[] includes) where T : class;

        void Add<T>(T obj) where T : class;

        void Update<T>(T obj) where T : class;

        void Delete<T>(T obj) where T : class;
    }

    public partial class Repository(ApplicationDbContext context) : IRepository
    {
        readonly ApplicationDbContext _context = context;

        public IQueryable<T> Set<T>() where T : class
        {
            return _context.Set<T>().AsQueryable<T>();
        }

        public IQueryable<T> SetNoTracking<T>() where T : class
        {
            return Set<T>().AsNoTracking();
        }

        public IQueryable<T> SetNoTracking<T>(params string[] includes) where T : class
        {
            var query = _context.Set<T>().AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        public void Add<T>(T obj) where T : class
        {
            _context.Set<T>().Add(obj);
            SaveChanges();
        }

        public void Update<T>(T obj) where T : class
        {
            _context.Entry<T>(obj).State = EntityState.Modified;
            SaveChanges();
        }

        public void Delete<T>(T obj) where T : class
        {
            _context.Entry<T>(obj).State = EntityState.Deleted;
            SaveChanges();
        }

        void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
