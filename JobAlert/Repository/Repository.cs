using JobAlert.Data;
using JobAlert.Models;
using JobAlert.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.DevTools.V129.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Repository
{
    public class Repository<T> : IRepository<Job> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<Job> _dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<Job>();
        }
        public async Task SaveJobsAsync(List<Job> entity)
        {
            var distinctJobs = entity
        .GroupBy(job => new { job.Title, job.Company, job.SiteName })
        .Select(g => g.First())
        .ToList();

            var existingJobs = await _db.Jobs
                .Select(job => new { job.Title, job.Company, job.SiteName })
                .ToListAsync();

            var newJobs = distinctJobs.Where(job => !existingJobs.Any(e =>
                e.Title == job.Title &&
                e.Company == job.Company &&
                e.SiteName == job.SiteName)).ToList();

            if (!newJobs.Any())
            {
                Console.WriteLine("Nema novih oglasa.");
                return;
            }

            await _db.Jobs.AddRangeAsync(newJobs);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Sacuvano {newJobs.Count} novih oglasa.");
        }
    }
}
