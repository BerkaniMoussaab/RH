using Microsoft.AspNetCore.Mvc;
using RH.Models;
using RH.Services;

namespace RH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformanceReviewController : ControllerBase
    {
        private readonly IPerformanceReviewService _service;

        public PerformanceReviewController(IPerformanceReviewService service) => _service = service;

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            await _service.GetByIdAsync(id) is PerformanceReview r ? Ok(r) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create(PerformanceReview review) =>
            CreatedAtAction(nameof(GetById), new { id = review.Id }, await _service.CreateAsync(review));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PerformanceReview review)
        {
            if (id != review.Id) return BadRequest();
            await _service.UpdateAsync(review);
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
