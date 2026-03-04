using JobAlert.Data;
using JobAlert.Models;
using JobAlert.Repository.IRepository;
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
        public Repository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task AddJobsAsync(List<Job> enitites)
        {
            if (enitites.Count <= 0)
            {
                Console.WriteLine("No jobs to add.");   
            }
            await _db.Jobs.AddRangeAsync(enitites);
            await _db.SaveChangesAsync();

        }
    }
}
