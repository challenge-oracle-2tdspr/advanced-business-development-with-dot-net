using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroTech.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ISensorService _sensorService;

        public DashboardController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        public async Task<IActionResult> Index()
        {
            var sensors = await _sensorService.GetAllAsync();
            return View(sensors);
        }
    }
}