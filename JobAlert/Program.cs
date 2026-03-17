using JobAlert.Services;
using JobAlert.Data;
using JobAlert.Models;
using Microsoft.EntityFrameworkCore;
using JobAlert.Repository.IRepository;
using JobAlert.Repository;
using JobAlert.Scrapers;

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
        builder.Services.AddScoped<IJobRepository, JobRepository>();
        builder.Services.AddScoped<IRepository<Application>, Repository<Application>>();
        //builder.Services.AddScoped<IJobScraper, HelloWorldScraper>();
        //builder.Services.AddScoped<IJobScraper, HelloWorldScraper>();
        //  builder.Services.AddScoped<IJobScraper, JobertyScraper>();
        builder.Services.AddControllers();

        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowAnyOrigin();
            });
        });

        var app = builder.Build();

        Console.WriteLine("✈️  JobAlert - Job Tracker");
        Console.WriteLine("═══════════════════════════════════════════\n");

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
           // var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
           // var jobs = await scraper.RunAsync();
           //Console.WriteLine($"Pronađeno {jobs.Count} oglasa.");
        }

        app.UseCors();
        app.MapControllers();
        app.Run();
    }
}