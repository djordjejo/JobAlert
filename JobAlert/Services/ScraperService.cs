    using JobAlert.Models;
    using JobAlert.Repository;
    using JobAlert.Repository.IRepository;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;

    namespace JobAlert.Services
    {
        public class ScraperService
        {
            private readonly IEnumerable<IJobScraper> _scrapers;
            private readonly IRepository<Job> _repository;
            public ScraperService(IEnumerable<IJobScraper> scrapers, IRepository<Job> repository)
            {
                _scrapers = scrapers;
                _repository = repository;

            }

            public async Task<List<Job>> RunAsync()
            {

                var allJobs = new List<Job>();

                foreach (var scraper in _scrapers)
                    {
                        var jobs = await scraper.ScrapeJobs();
                        allJobs.AddRange(jobs);
                        await _repository.AddJobsAsync(jobs);
                        Console.WriteLine($"Sačuvano {jobs.Count} oglasa.");
                    }
                return allJobs;
            }
        }
    }