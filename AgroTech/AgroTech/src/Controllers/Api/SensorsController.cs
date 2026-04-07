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
            var sensors = (await _sensorService.GetAllAsync()).ToList();

            foreach (var sensor in sensors)
            {
                sensor.Links = GenerateLinks(sensor.Id);
            }

            return Ok(sensors);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SensorDTO>> GetById(Guid id)
        {
            var sensor = await _sensorService.GetByIdAsync(id);

            if (sensor == null)
                return NotFound(new { message = "Sensor não encontrado." });

            sensor.Links = GenerateLinks(sensor.Id);

            return Ok(sensor);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] List<SensorDTO> dtos)
        {
            var ids = await _sensorService.AddAsync(dtos);

            return Ok(new
            {
                message = "Sensores criados com sucesso.",
                ids
            });
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

            foreach (var item in result.Items)
            {
                item.Links = GenerateLinks(item.Id);
            }

            return Ok(result);
        }
        private List<LinkDTO> GenerateLinks(Guid id)
        {
            return new List<LinkDTO>
            {
                new LinkDTO
                {
                    Rel = "self",
                    Href = Url.Action(nameof(GetById), new { id }) ?? $"/api/sensors/{id}",
                    Method = "GET"
                },
                new LinkDTO
                {
                    Rel = "update",
                    Href = Url.Action(nameof(Update), new { id }) ?? $"/api/sensors/{id}",
                    Method = "PUT"
                },
                new LinkDTO
                {
                    Rel = "delete",
                    Href = Url.Action(nameof(Delete), new { id }) ?? $"/api/sensors/{id}",
                    Method = "DELETE"
                },
                new LinkDTO
                {
                    Rel = "search",
                    Href = Url.Action(nameof(Search)) ?? "/api/sensors/search",
                    Method = "GET"
                }
            };
        }
    }
}