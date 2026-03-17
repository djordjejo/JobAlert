using JobAlert.Models;
using JobAlert.Repository;
using JobAlert.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace JobAlert.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IRepository<Application> _applicationRepository;
        public ApplicationController(IRepository<Application> repository)
        {
            _applicationRepository = repository;
        }
        [HttpGet("getallapplications")]
        public async Task<IActionResult> GetAllApplications()
        {
            var applications = await _applicationRepository.GetAllAsync();
            if (applications != null)
                return await Task.FromResult(Ok(applications));
            else
                return await Task.FromResult(NotFound());
        }

        [HttpPost("apply/{jobId}")]
        public async Task<IActionResult> AddApplication(Guid jobId)
        {
            var application = new Application
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                Applied = true
            };
            if (jobId == Guid.Empty)
                return BadRequest("Application cannot be null.");
            try
            {
                await _applicationRepository.AddEntity(application);
                return Ok("Application added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        
    }
}
