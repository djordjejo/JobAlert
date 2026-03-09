using Azure;
using JobAlert.Models;
using JobAlert.Repository.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JobAlert.Scrapers
{
    public class JobertyScraper : IJobScraper
    {
        private readonly IWebDriver driver;
        private readonly IConfiguration configuration;

        public JobertyScraper(IConfiguration _configuration)
        {
            configuration = _configuration;
            var options = new ChromeOptions();
            options.AddArgument("headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            driver = new ChromeDriver(options);
        }

        public async Task<List<Job>> ScrapeJobs()
        {
            List<Job> jobs = new List<Job>();
            try
            {
                var url = "https://www.joberty.com/sr/IT-jobs?days=15D&domains=Backend,Frontend,Fullstack,Data%20Science,QA&location=Belgrade%20%28Serbia%29&page=1&seniority=Internship,Intermediate&sort=created";

                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                wait.Until(d => d.FindElements(By.CssSelector("div.jbox > div.jbox")).Count > 0);

                var jobCards = driver.FindElements(By.CssSelector("div.jbox > div.jbox"));

                var jobUrls = jobCards.Select(card =>
                {
                    try
                    {
                        var href = card.FindElement(By.CssSelector("span.font-semibold a")).GetAttribute("href");
                        if (!string.IsNullOrEmpty(href) && !href.StartsWith("http"))
                            href = "https://www.joberty.com" + href;
                        return href;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                })
                .Where(href => !string.IsNullOrEmpty(href))
                .ToList();

                foreach (var jobUrl in jobUrls)
                {
                    var job = await ScrapeJobDetails(jobUrl, wait);
                    if (job != null) jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during scraping: {ex.Message}");
            }
            finally
            {
                driver.Quit();
            }

            return jobs;
        }
      private async Task<Job> ScrapeJobDetails(string url, WebDriverWait wait)
        {
            try
            {
                driver.Navigate().GoToUrl(url);

                wait.Until(d => d.FindElements(By.CssSelector("div.container-job-description")).Count > 0);

                var job = new Job();
                job.SiteName = "Joberty";

                try { 
                    job.Title = driver.FindElement(By.CssSelector("span.text-lg.font-semibold")).Text.Trim(); 
                    job.Url = url; 
                } catch { }

                try { job.Company = driver.FindElement(By.CssSelector("a[href*='it-company']")).Text.Trim(); } catch { }

                try
                {
                    job.Location = driver.FindElement(
                    By.XPath("//span[text()='Lokacija:']/following-sibling::span")).Text.Trim();
                }
                catch { }

                try
                {
                    job.JobDescription = driver.FindElement(
                    By.CssSelector("div.container-job-description")).Text.Trim();
                }
                catch { }



                return job;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri scrapovanju {url}: {ex.Message}");
                return null;
            }
        }

    }
}
