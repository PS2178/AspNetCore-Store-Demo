using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetStoreDemo.Data;
using AspNetStoreDemo.Models;
using AspNetStoreDemo.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetStoreDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDetails.AdminEndUser)]
    [Area("Admin")]
    public class ProductTypesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductTypesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.ProductTypes.ToList());
        }

        //GET Create action method
        public IActionResult Create()
        {
            return View();
        }

        //POST Create action method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<IActionResult> Create(ProductTypes productTypes)
        {
            if (ModelState.IsValid)
            {
                _db.Add(productTypes);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(productTypes);
        }

        //GET Edit action method
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = await _db.ProductTypes.FindAsync(id);
            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        //POST Edit action method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductTypes productTypes)
        {
            if (id != productTypes.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _db.Update(productTypes);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(productTypes);
        }

        //GET Details action method
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = await _db.ProductTypes.FindAsync(id);
            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        //GET Delete action method
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = await _db.ProductTypes.FindAsync(id);
            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        //POST Delete action method
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productTypes = await _db.ProductTypes.FindAsync(id);

            _db.ProductTypes.Remove(productTypes);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}