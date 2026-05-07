using Microsoft.EntityFrameworkCore;
using WuyiPlay_DAL.Models;
using System;
using System.Threading.Tasks;

namespace WuyiPlay_DAL.Common.Repository
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly WuyiPlayDbContext _context;

        public UnitOfWork(WuyiPlayDbContext context)
        {
            _context = context;
        }

        public DbContext Context => _context;

        // Bổ sung <T> ở đây
        public DbSet<T> Set<T>() where T : class
        {
            return _context.Set<T>();
        }

        // Trả về int để khớp với Task<int> Commit()
        public async Task<int> Commit()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}




