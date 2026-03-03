using JetAlert.Model;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace JetAlert.Services
{
    public class ScraperService : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly IConfiguration _configuration;

        public ScraperService(IConfiguration configuration)
        {
            _configuration = configuration;

            var options = new ChromeOptions();
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            var userAgent = _configuration["ScraperSettings:UserAgent"]
                 ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
            options.AddArgument($"user-agent={userAgent}");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        public async Task<List<Flight>> ScrapeMatrixAsync(
            string origin,
            string destination,
            DateTime departureDate,
            DateTime returnDate,
            int adults)
        {
            var allFlights = new List<Flight>();

            var url = $"https://booking.airserbia.com/dx/JUDX/#/matrix" +
                $"?journeyType=round-trip" +
                $"&origin={origin}&destination={destination}" +
                $"&date={departureDate:MM-dd-yyyy}" +
                $"&origin1={destination}&destination1={origin}" +
                $"&date1={returnDate:MM-dd-yyyy}" +
                $"&ADT={adults}" +
                $"&pointOfSale=RS&locale=sr-LATN";

            _driver.Navigate().GoToUrl(url);

            // Čekaj duže da se učitaju PRAVE cene (ne dummy)
            Console.WriteLine("Čekam 10 sekundi da se učitaju prave cene...");
            await Task.Delay(10000);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(d => d.FindElements(By.CssSelector("tbody")).Count > 0);
            wait.Until(d => d.FindElements(By.CssSelector("button.dxp-matrix-grid-cell-new")).Count > 0);

            var priceCells = _driver.FindElements(By.CssSelector("button.dxp-matrix-grid-cell-new"));
            var jsExecutor = (IJavaScriptExecutor)_driver;

            var cellsWithPrices = new List<(IWebElement cell, decimal price)>();

            foreach (var cell in priceCells)
            {
                try
                {
                    var priceText = jsExecutor.ExecuteScript(
                        "return arguments[0].querySelector('span.amount span.number')?.textContent || 'N/A';",
                        cell
                    )?.ToString();

                    if (priceText != null && priceText != "N/A")
                    {
                        var price = ParsePrice(priceText);
                        cellsWithPrices.Add((cell, price));
                        Console.WriteLine($"Ćelija: {price} RSD");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška čitanja cene: {ex.Message}");
                }
            }

            if (!cellsWithPrices.Any())
            {
                Console.WriteLine("❌ Nijedna cena nije pronađena!");
                return allFlights;
            }

            // Proveri da li su sve cene iste (dummy data)
            var uniquePrices = cellsWithPrices.Select(c => c.price).Distinct().Count();
            if (uniquePrices == 1)
            {
                Console.WriteLine($"⚠️ UPOZORENJE: Sve ćelije imaju istu cenu ({cellsWithPrices[0].price})!");
                Console.WriteLine("⚠️ Moguće je da GraphQL vraća dummy podatke!");
            }

            var minPrice = cellsWithPrices.Min(c => c.price);
            Console.WriteLine($"🎯 Najniža cena: {minPrice} RSD");

            var cheapestCell = cellsWithPrices.First(c => c.price == minPrice);

            try
            {
                Console.WriteLine($"Klikam na celiju sa cenom {minPrice}...");

                // JavaScript klik na ćeliju
                jsExecutor.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", cheapestCell.cell);
                await Task.Delay(500);
                jsExecutor.ExecuteScript("arguments[0].click();", cheapestCell.cell);

                Console.WriteLine("✅ Kliknuo na ćeliju");

                // Čekaj da dugme postane enabled
                await Task.Delay(2000);

                var continueBtn = _driver.FindElement(By.CssSelector("button.dxp-matrix-footer-search-button"));
                var isEnabled = !continueBtn.GetAttribute("class").Contains("disabled");

                Console.WriteLine($"Continue dugme enabled: {isEnabled}");

                if (isEnabled)
                {
                    var currentUrl = _driver.Url;
                    Console.WriteLine($"URL pre klika na Continue: {currentUrl}");

                    jsExecutor.ExecuteScript("arguments[0].click();", continueBtn);

                    // ČEKAJ da se URL promeni na flight-selection
                    var urlWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                    try
                    {
                        urlWait.Until(d => d.Url != currentUrl && d.Url.Contains("flight-selection"));

                        Console.WriteLine($"✅ URL se promenio: {_driver.Url}");

                        // Čekaj da se učitaju letovi
                        await Task.Delay(7000);

                        var flights = await ScrapeCurrentPageFlightsAsync(origin, destination, departureDate);
                        allFlights.AddRange(flights);

                        Console.WriteLine($"✅ Pronađeno {flights.Count} letova");
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Console.WriteLine("❌ URL se nije promenio u 15 sekundi!");
                        Console.WriteLine($"Trenutni URL: {_driver.Url}");

                        // Probaj da scrape-uješ i dalje (možda je stranica učitana)
                        var flights = await ScrapeCurrentPageFlightsAsync(origin, destination, departureDate);
                        allFlights.AddRange(flights);

                        if (flights.Count == 0)
                        {
                            // Screenshot za debug
                            var screenshotPath = $"timeout_{DateTime.Now:HHmmss}.png";
                            ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(screenshotPath);
                            Console.WriteLine($"Screenshot sačuvan: {screenshotPath}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❌ Dugme još uvek disabled!");
                    var screenshotPath = "disabled_button.png";
                    ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(screenshotPath);
                    Console.WriteLine($"Screenshot sačuvan: {screenshotPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Greška pri kliku: {ex.Message}");
            }

            return allFlights;
        }

        private async Task<List<Flight>> ScrapeCurrentPageFlightsAsync(
            string origin,
            string destination,
            DateTime departureDate)
        {
            var flights = new List<Flight>();

            try
            {
                var delay = _configuration.GetValue<int>("ScraperSettings:DelayBetweenRequests", 2000);
                await Task.Delay(delay);

                var flightCards = _driver.FindElements(By.CssSelector("div.dxp-itinerary-part-offer"));

                Console.WriteLine($"  Found {flightCards.Count} flight cards on page");

                if (!flightCards.Any())
                    return flights;

                for (int i = 0; i < flightCards.Count; i++)
                {
                    try
                    {
                        var cards = _driver.FindElements(By.CssSelector("div.dxp-itinerary-part-offer"));
                        var card = cards[i];

                        var price = card.FindElement(By.CssSelector("span.number"));
                        var flightNum = card.FindElement(By.CssSelector("div.flight-number"));
                        var duration = card.FindElement(By.CssSelector("time.dxp-duration span"));
                        var seats = card.FindElement(By.CssSelector("div.itinerary-part-remaining-seats span"));

                        var flight = new Flight
                        {
                            FlightNumber = flightNum.Text.Trim(),
                            Origin = origin,
                            Destination = destination,
                            DepartureDate = departureDate,
                            Price = ParsePrice(price.Text),
                            Seats = seats.Text.Trim(),
                            Duration = duration.Text.Trim(),
                            Currency = "EUR",
                            ScrapedAt = DateTime.UtcNow
                        };

                        flights.Add(flight);
                        Console.WriteLine($"    ✓ {flight.FlightNumber}: {flight.Price} EUR");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error flight {i}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping page: {ex.Message}");
            }

            return flights;
        }

        private decimal ParsePrice(string priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText))
                return 0;

            // Ukloni sve osim cifara
            var digitsOnly = new string(priceText.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(digitsOnly))
                return 0;

            // Parse kao ceo broj
            if (decimal.TryParse(digitsOnly,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out var price))
            {
                return price;
            }

            return 0;
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}