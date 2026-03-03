using JobAlert.Models;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace JobAlert.Services
{
    public class ScraperService : IDisposable
    {
        private IWebDriver _driver;
        private IConfiguration configuration;
        public Task<List<Job>> ScrapeJobPostingsAsync()
        {
            var url = "https://helloworld.rs/oglasi-za-posao/beograd?icampaign=home-fancy-intro&imedium=site&isource=Helloworld.rs&senioritet%5B0%5D=1";

            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(2000);

            var jobs = new List<Job>();

            var jobCards = _driver.FindElements(By.CssSelector(".__search-results > div"));

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

                    var job = new Job
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Company = company,
                        Location = location,
                        DatePosted = dateParsed,
                    };

                    jobs.Add(job);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }

            return Task.FromResult(jobs);
        }
        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}