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
using JobAlert.Scrapers;
using OpenQA.Selenium.DevTools.V129.Page;

namespace JetAlert;  

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

                builder.Configuration
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
               
               
                   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                   builder.Services.AddDbContext<ApplicationDbContext>(options =>
                      options.UseSqlServer(connectionString));
                   builder.Services.AddScoped<ScraperService>();
                   builder.Services.AddScoped<NotificationService>();
                   builder.Services.AddScoped<IRepository<Job>, Repository<Job>>();
                   builder.Services.AddScoped<IJobScraper, HelloWorldScraper>();
                   builder.Services.AddScoped<IJobScraper, JobertyScraper>();

        // cors

        var app = builder.Build();
        Console.WriteLine("✈️  JobAlert - Job Tracker");
        Console.WriteLine("═══════════════════════════════════════════\n");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
            var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
            var jobs = await scraper.RunAsync();

            Console.WriteLine($"Pronađeno {jobs.Count} oglasa.");
        }

        app.MapGet("/api/jobs/", async ( ApplicationDbContext db) =>
        {
            var jobs = await db.Jobs.ToListAsync();
            if(jobs != null)
                return Results.Ok(jobs);
            else
                return Results.NotFound();
        });

    }

}
