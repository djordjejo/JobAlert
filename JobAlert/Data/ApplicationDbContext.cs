using JobAlert.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Company).IsRequired();
                entity.Property(e => e.Location).IsRequired();
                entity.Property(e => e.JobDescription).IsRequired();
                entity.Property(e => e.Remote).IsRequired();
                entity.Property(e => e.DatePosted).IsRequired();
                entity.Property(e => e.SiteName);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");


            });
            base.OnModelCreating(modelBuilder);
        }


    }
}
