using Microsoft.AspNetCore.Mvc;
using RH.Models;
using RH.Services;

namespace RH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _service;

        public PayrollController(IPayrollService service) => _service = service;

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            await _service.GetByIdAsync(id) is Payroll p ? Ok(p) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create(Payroll payroll) =>
            CreatedAtAction(nameof(GetById), new { id = payroll.Id }, await _service.CreateAsync(payroll));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payroll payroll)
        {
            if (id != payroll.Id) return BadRequest();
            await _service.UpdateAsync(payroll);
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
