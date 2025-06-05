using Microsoft.AspNetCore.Mvc;
using RH.Models;
using RH.Services;

namespace RH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobTitleController : ControllerBase
    {
        private readonly IJobTitleService _service;

        public JobTitleController(IJobTitleService service) => _service = service;

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            await _service.GetByIdAsync(id) is JobTitle jt ? Ok(jt) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create(JobTitle jobTitle) =>
            CreatedAtAction(nameof(GetById), new { id = jobTitle.Id }, await _service.AddAsync(jobTitle));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, JobTitle jobTitle)
        {
            if (id != jobTitle.Id) return BadRequest();
            await _service.UpdateAsync(jobTitle);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
