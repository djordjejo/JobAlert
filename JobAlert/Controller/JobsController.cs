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
            var jobs = await _jobRepository.GetAllAsync();

                if(jobs != null)
                    return await Task.FromResult(Ok(jobs));
                else
                    return await Task.FromResult(NotFound());
        }
        [HttpPost("jooble")]
        public async Task<IActionResult> GetJoobleAPI()
        { 
            var client = new HttpClient();
            var body = new
            {
                keywords = "software developer",
                location = "United States",
                page = 1,
                ResultOnPage = 20
            };

            var response = await client.PostAsJsonAsync(
                    "https://jooble.org/api/8f23f6df-6957-4f82-9fb0-ef216a6754a4",
                    body
                );

            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, $"Jooble greška: {response.StatusCode}");

            return Ok(data);
        }
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            var application = await _jobRepository.GetAsync(id);
            if (application == null)
                return NotFound("Application not found.");
            try
            {
                await _jobRepository.RemoveEnity(application);
                return Ok("Application deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
