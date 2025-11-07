using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroTech.Web.Controllers
{
    public class FarmController : Controller
    {
        private readonly IFarmService _farmService;

        public FarmController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        public async Task<IActionResult> Index()
        {
            var farms = await _farmService.GetAllAsync();
            return View(farms);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _farmService.AddAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var farm = await _farmService.GetByIdAsync(id);
            if (farm == null) return NotFound();
            return View(farm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FarmDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _farmService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var farm = await _farmService.GetByIdAsync(id);
            if (farm == null) return NotFound();
            return View(farm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _farmService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
