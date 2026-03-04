using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Repository.IRepository
{
    public interface IJobScraper
    {
        Task<List<Models.Job>> ScrapeJobs();
    }
}
