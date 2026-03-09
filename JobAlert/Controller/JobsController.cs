using JobAlert.Models;
using JobAlert.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.DevTools.V129.CSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlert.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController: ControllerBase
    {
        private readonly IRepository<Job> _jobRepository;
        public JobsController(IRepository<Job> repository)
        { 
            _jobRepository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobRepository.GetAllJobsAsync();

                if(jobs != null)
                    return await Task.FromResult(Ok(jobs));
                else
                    return await Task.FromResult(NotFound());
        }
    }
}
