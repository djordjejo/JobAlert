using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Scrapers
{
    public class JoobleScraper
    {
        private readonly IWebDriver driver;
        private readonly IConfiguration configuration;

        public JoobleScraper(IConfiguration configuration)
        {
            configuration = configuration;
            var options = new ChromeOptions();
            options.AddArgument("headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            driver = new ChromeDriver();
        }

        
    }
}
