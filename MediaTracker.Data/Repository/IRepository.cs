namespace MediaTracker.Data.Repository
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
}
