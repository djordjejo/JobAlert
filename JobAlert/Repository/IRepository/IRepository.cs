using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobAlert.Models;

namespace JobAlert.Repository.IRepository
{
    public interface IRepository <T>  where T : class
    {
        public Task AddJobsAsync(HashSet<T> entity);
    }
}
