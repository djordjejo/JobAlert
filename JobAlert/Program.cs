using JobAlert.Services;
using JobAlert.Data;
using JobAlert.Models; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JobAlert.Services;
using JobAlert.Repository.IRepository;
using JobAlert.Repository;

namespace JetAlert;  

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
               .ConfigureAppConfiguration((context, config) =>
               {
                   config.SetBasePath(Directory.GetCurrentDirectory());
                   config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
               })
               .ConfigureServices((context, services) =>
               {
                   var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                   services.AddDbContext<ApplicationDbContext>(options =>
                       options.UseSqlServer(connectionString));

                   services.AddScoped<ScraperService>();
                   services.AddScoped<NotificationService>();

                   services.AddScoped<IRepository<Job>, Repository<Job>>();
                   services.AddScoped<IJobScraper, HelloWorldScraper>();
               })
               .Build();

        Console.WriteLine("✈️  JobAlert - Flight Tracker");
        Console.WriteLine("═══════════════════════════════════════════\n");

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
            var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
            var jobs = await scraper.RunAsync();

            Console.WriteLine($"Pronađeno {jobs.Count} oglasa.");

          
        }

    }

}
