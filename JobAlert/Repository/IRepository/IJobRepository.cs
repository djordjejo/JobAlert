using JobAlert.Models;

namespace JobAlert.Repository.IRepository
{
    public interface IJobRepository
    {
         public Task SaveJobsAsync(List<Job> job);
           
    }
}
