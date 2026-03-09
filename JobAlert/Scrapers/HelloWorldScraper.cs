using JobAlert.Models;
using JobAlert.Repository.IRepository;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace JobAlert.Repository
{
    public class HelloWorldScraper : IJobScraper
    {
        private readonly IWebDriver _driver;
        private readonly IConfiguration _configuration;

        public HelloWorldScraper(IConfiguration configuration)
        {
            _configuration = configuration;
            var options = new ChromeOptions();
            options.AddArgument("--headless=new"); 
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1800,1080");

            _driver = new ChromeDriver(options);
        }

        public async Task<List<Job>> ScrapeJobs()
        {
            var url = "https://helloworld.rs/oglasi-za-posao/beograd?icampaign=home-fancy-intro&imedium=site&isource=Helloworld.rs&senioritet%5B0%5D=1&vreme_postavljanja=2"; //za oglase od pre 2 dana

            _driver.Navigate().GoToUrl(url);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElements(By.CssSelector(".__ga4_job_title")).Count > 0);
            var jobs = new List<Job>();

            var jobCards = _driver.FindElements(By.CssSelector(".__search-results .relative.bg-white, .__search-results .relative.bg-transparent"));

            foreach (var card in jobCards)
            {
                try
                {
                    var titleElement = card.FindElement(By.CssSelector(".__ga4_job_title"));
                    var title = titleElement.Text.Trim();

                    var companyElement = card.FindElement(By.CssSelector(".__ga4_job_company"));
                    var company = companyElement.Text.Trim();

                    var locationElement = card.FindElement(By.CssSelector(".la-map-marker + p"));
                    var location = locationElement.Text.Trim();

                    var dateElement = card.FindElement(By.CssSelector(".la-clock + p"));
                    var dateText = dateElement.Text.Trim();
                    var dateParsed = DateOnly.ParseExact(dateText, "dd.MM.yyyy.", null);
                    var jobUrl = card.FindElement(By.CssSelector("a.__ga4_job_title")).GetAttribute("href");
                    var job = new Job
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Company = company,
                        Location = location,
                        DatePosted = dateParsed,
                        SiteName = "HelloWorld",
                        Url = jobUrl
                    };

                    jobs.Add(job);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing job card: {ex.Message}");
                    continue;
                }
            }

            return await Task.FromResult(jobs);
        }
      
    }
}
