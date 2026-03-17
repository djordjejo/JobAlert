using JobAlert.Data;
using JobAlert.Models;
using JobAlert.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.DevTools.V129.DOM;

namespace JobAlert.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _db;
        public JobRepository(ApplicationDbContext db)
        {
            _db = db;
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
