using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace AgroTech.Web.Controllers
{
    public class CropController : Controller
    {
        private readonly ICropService _cropService;
        private readonly IFarmService _farmService;

        public CropController(ICropService cropService, IFarmService farmService)
        {
            _cropService = cropService;
            _farmService = farmService;
        }

        public async Task<IActionResult> Index()
        {
            var crops = await _cropService.GetAllAsync();
            return View(crops);
        }

        public async Task<IActionResult> Create()
        {
            var farms = await _farmService.GetAllAsync();
            ViewBag.Farms = new SelectList(farms, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CropDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var farms = await _farmService.GetAllAsync();
                ViewBag.Farms = new SelectList(farms, "Id", "Name", dto.FarmId);
                return View(dto);
            }

            try
            {
                await _cropService.AddAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var farms = await _farmService.GetAllAsync();
                ViewBag.Farms = new SelectList(farms, "Id", "Name", dto.FarmId);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var crop = await _cropService.GetByIdAsync(id);
            if (crop == null) return NotFound();

            var farms = await _farmService.GetAllAsync();
            ViewBag.Farms = new SelectList(farms, "Id", "Name", crop.FarmId);

            return View(crop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CropDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var farms = await _farmService.GetAllAsync();
                ViewBag.Farms = new SelectList(farms, "Id", "Name", dto.FarmId);
                return View(dto);
            }

            try
            {
                await _cropService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var farms = await _farmService.GetAllAsync();
                ViewBag.Farms = new SelectList(farms, "Id", "Name", dto.FarmId);
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var crop = await _cropService.GetByIdAsync(id);
            if (crop == null) return NotFound();
            return View(crop);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _cropService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
