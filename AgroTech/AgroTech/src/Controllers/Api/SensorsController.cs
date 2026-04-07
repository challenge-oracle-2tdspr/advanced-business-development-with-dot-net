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
        private readonly ILogger<SensorsController> _logger;

        public SensorsController(ISensorService sensorService,  ILogger<SensorsController> logger)
        {
            _sensorService = sensorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorDTO>>> GetAll()
        {
            _logger.LogInformation("Buscando todos os sensores.");
            
            var sensors = (await _sensorService.GetAllAsync()).ToList();

            foreach (var sensor in sensors)
            {
                sensor.Links = GenerateLinks(sensor.Id);
            }
            _logger.LogInformation("Foram retornados {Count} sensores.",  sensors.Count);
            return Ok(sensors); 
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SensorDTO>> GetById(Guid id)
        {
            _logger.LogInformation("Buscando sensor por id: {SensorId}.", id);
            var sensor = await _sensorService.GetByIdAsync(id);

            if (sensor == null)
            {
                _logger.LogWarning("Sensor não Encontrado para o id: {SensorId}", id);
               
                return NotFound(new { message = "Sensor não encontrado." });
            }

            sensor.Links = GenerateLinks(sensor.Id);

            return Ok(sensor);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] List<SensorDTO> dtos)
        {
            _logger.LogInformation("Recebendo lote de sensores com {Count} itens.", dtos.Count);

            var ids = await _sensorService.AddAsync(dtos);

            _logger.LogInformation("Lote de sensores salvo com sucesso. {Count} registros criados.", ids.Count);

            return Ok(new
            {
                message = "Sensores criados com sucesso.",
                ids
            });
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] SensorDTO dto)
        {
            _logger.LogInformation("Atualizando sensor {SensorId}", id);

            if (id != dto.Id)
            {
                _logger.LogWarning("Id da rota {RouteId} diferente do id do corpo {BodyId}", id, dto.Id);
                return BadRequest(new { message = "O id da rota é diferente do id do corpo." });
            }

            await _sensorService.UpdateAsync(dto);

            _logger.LogInformation("Sensor {SensorId} atualizado com sucesso.", id);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Removendo sensor {SensorId}", id);

            await _sensorService.DeleteAsync(id);

            _logger.LogInformation("Sensor {SensorId} removido com sucesso.", id);

            return NoContent();
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<PagedResultDTO<SensorDTO>>> Search([FromQuery] SensorSearchDTO searchDto)
        {
            _logger.LogInformation(
                "Buscando sensores com filtros: Name={Name}, Type={Type}, MinValue={MinValue}, MaxValue={MaxValue}, Page={Page}, PageSize={PageSize}",
                searchDto.Name, searchDto.Type, searchDto.MinValue, searchDto.MaxValue, searchDto.Page, searchDto.PageSize);

            var result = await _sensorService.SearchAsync(searchDto);

            foreach (var item in result.Items)
            {
                item.Links = GenerateLinks(item.Id);
            }

            _logger.LogInformation("Busca retornou {Count} sensores na página {Page}.", result.Items.Count(), result.Page);

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