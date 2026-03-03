using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Models
{
    public class JobCard
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public DateOnly DatePosted { get; set; }

       
    }
}
