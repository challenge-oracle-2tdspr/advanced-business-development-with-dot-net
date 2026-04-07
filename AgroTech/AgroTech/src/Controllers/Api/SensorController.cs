using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroTech.Web.Controllers.Api
{
    [ApiController]
    [Route("api/sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensorsController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorDTO>>> GetAll()
        {
            var sensors = await _sensorService.GetAllAsync();
            return Ok(sensors);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SensorDTO>> GetById(Guid id)
        {
            var sensor = await _sensorService.GetByIdAsync(id);

            if (sensor == null)
                return NotFound(new { message = "Sensor não encontrado." });

            return Ok(sensor);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] SensorDTO dto)
        {
            var id = await _sensorService.AddAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { id, message = "Sensor criado com sucesso." }
            );
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] SensorDTO dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "O id da rota é diferente do id do corpo." });

            await _sensorService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _sensorService.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("search")]
        public async Task<ActionResult<PagedResultDTO<SensorDTO>>> Search([FromQuery] SensorSearchDTO searchDto)
        {
            var result = await _sensorService.SearchAsync(searchDto);
            return Ok(result);
        }
    }
}