using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Common.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        DbSet<T> Set<T>() where T : class;
        Task<int> Commit();

        DbContext Context { get; }
    }
}
