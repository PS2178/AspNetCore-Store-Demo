using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetStoreDemo.Data;
using AspNetStoreDemo.Models;
using AspNetStoreDemo.Models.ViewModel;
using AspNetStoreDemo.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetStoreDemo.Controllers
{
    [Authorize(Roles = StaticDetails.SuperAdminUserEndUser)]
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;


        [BindProperty]
        public ProductViewModel ProductsVM { get; set; }

        public ProductsController(ApplicationDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            ProductsVM = new ProductViewModel()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Products = new Models.Products()
            };
        }

        public async Task<IActionResult> Index()
        {
            var products = _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags);

            return View(await products.ToListAsync());
        }

        //GET : Products Create
        public IActionResult Create()
        {
            return View(ProductsVM);
        }

        //POST : Products Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductsVM);
            }

            //add product to db and save the changes if valid
            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //image being saved
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            //retrive added product from db
            var productsFromDb = _db.Products.Find(ProductsVM.Products.Id);

            if (files.Count != 0)
            {
                //Image has been uploaded
                var upload = Path.Combine(webRootPath, StaticDetails.ImageFolder);
                var extention = Path.GetExtension(files[0].FileName);

                using(var filestream = new FileStream(Path.Combine(upload, ProductsVM.Products.Id + extention), FileMode.Create))
                {
                    //moves file onto server and rename it
                    files[0].CopyTo(filestream);
                }
                productsFromDb.Image = @"\" + StaticDetails.ImageFolder + @"\" + ProductsVM.Products.Id + extention;
            }
            //if file was not uploaded
            else
            {
                //use dummy image from the server
                var upload = Path.Combine(webRootPath, StaticDetails.ImageFolder+@"\"+StaticDetails.DefaultProductImage);
                System.IO.File.Copy(upload, webRootPath + @"\" + StaticDetails.ImageFolder + @"\" + ProductsVM.Products.Id + @".png");
                productsFromDb.Image = @"\"+ StaticDetails.ImageFolder + @"\" + ProductsVM.Products.Id + @".png";
            }

            //update the db
            await _db.SaveChangesAsync();

            //return back to index
            return RedirectToAction(nameof(Index));
        }

        //GET : Edit
        public async Task<IActionResult> Edit(int? id)
        {
            //look at id
            if (id == null)
            {
                return NotFound();
            }

            //populate view data
            ProductsVM.Products = await _db.Products
                .Include(m => m.SpecialTags)
                .Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(m => m.Id == id);

            //make sure product has been retrieved
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }
            
            return View(ProductsVM);
        }

        //POST : Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            if(ModelState.IsValid)
            {
                //grab path
                string webRootPath = _hostingEnvironment.WebRootPath;

                //get files uploaded if any
                var files = HttpContext.Request.Form.Files;

                //find name of existing img
                var productFromDb = _db.Products.Where(m => m.Id == ProductsVM.Products.Id).FirstOrDefault();

                if (files.Count > 0 && files[0] != null)
                {
                    //if user uploads a new image
                    var upload = Path.Combine(webRootPath, StaticDetails.ImageFolder);
                    var newExtension = Path.GetExtension(files[0].FileName);
                    var oldExtension = Path.GetExtension(productFromDb.Image);

                    //delete old file if it exists
                    if (System.IO.File.Exists(Path.Combine(upload, ProductsVM.Products.Id + oldExtension)))
                    {
                        System.IO.File.Delete(Path.Combine(upload, ProductsVM.Products.Id + oldExtension));
                    }

                    using (var filestream = new FileStream(Path.Combine(upload, ProductsVM.Products.Id + newExtension), FileMode.Create))
                    {
                        //moves new file onto server and rename it
                        files[0].CopyTo(filestream);
                    }
                    ProductsVM.Products.Image = @"\" + StaticDetails.ImageFolder + @"\" + ProductsVM.Products.Id + newExtension;
                }

                if(ProductsVM.Products.Image != null)
                {
                    //update image property
                    productFromDb.Image = ProductsVM.Products.Image;
                }

                //update remaining properties
                productFromDb.Name = ProductsVM.Products.Name;
                productFromDb.Price = ProductsVM.Products.Price;
                productFromDb.Available = ProductsVM.Products.Available;
                productFromDb.ProductTypeId = ProductsVM.Products.ProductTypeId;
                productFromDb.SpecialTagsId = ProductsVM.Products.SpecialTagsId;
                productFromDb.Description = ProductsVM.Products.Description;

                //update database
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ProductsVM);
        }

        //GET : Details
        public async Task<IActionResult> Details(int? id)
        {
            //look at id
            if (id == null)
            {
                return NotFound();
            }

            //populate view data
            ProductsVM.Products = await _db.Products
                .Include(m => m.SpecialTags)
                .Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(m => m.Id == id);

            //make sure product has been retrieved
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        //GET : Delete
        public async Task<IActionResult> Delete(int? id)
        {
            //look at id
            if (id == null)
            {
                return NotFound();
            }

            //populate view data
            ProductsVM.Products = await _db.Products
                .Include(m => m.SpecialTags)
                .Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(m => m.Id == id);

            //make sure product has been retrieved
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        //POST : Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //grab path to root
            string webRootPath = _hostingEnvironment.WebRootPath;

            //grab product by id to delete
            Products products = await _db.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound();
            }
            else
            {
                //grab path and extension to image
                var upload = Path.Combine(webRootPath, StaticDetails.ImageFolder);
                var extension = Path.GetExtension(products.Image);


                //delete image file
                if (System.IO.File.Exists(Path.Combine(upload, products.Id + extension)))
                {
                    System.IO.File.Delete(Path.Combine(upload, products.Id + extension));
                }
                _db.Products.Remove(products);

                //update db
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
    }
}