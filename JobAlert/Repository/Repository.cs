using JobAlert.Data;
using JobAlert.Models;
using JobAlert.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSetT;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSetT = _db.Set<T>();
        }

        public Task AddEntity(T entity)
        {
            return Task.Run(async () =>
            {
                var result = await _dbSetT.AddAsync(entity);
                if (result == null)
                    throw new Exception("Failed to add entity to the database.");
                await _db.SaveChangesAsync();
            });
        }

        public async Task<List<T>> GetAllAsync()
        {
             return await _dbSetT.ToListAsync();

        }

        public async Task<T> GetAsync(Guid id)
        {
            return await _dbSetT.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public Task RemoveEnity(T entity)
        {
            return Task.Run(() =>
            {
                _dbSetT.Remove(entity);
                _db.SaveChanges();
            });
        }

        public async Task SaveEntity(T entity)
        {
            var result = await _dbSetT.AddAsync(entity);

            if (result == null)
                throw new Exception("Failed to add entity to the database.");

            await _db.SaveChangesAsync();
        }

    }
}
