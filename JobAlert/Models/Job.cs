using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Models
{
    public class Job : JobCard
    {
        public Guid Id { get; set; }
        public string SiteName { get; set; }
        public bool? Remote { get; set; }
        public decimal? Salary { get; set; }
        public string JobDescription { get; set; }

    }
}
