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
        public Task<List<T>> GetAllAsync();
        public Task<T> GetAsync(Guid id);
        public Task SaveEntity(T entity);
        public Task RemoveEnity(T entity); 
        public Task AddEntity(T entity);

    }
}
