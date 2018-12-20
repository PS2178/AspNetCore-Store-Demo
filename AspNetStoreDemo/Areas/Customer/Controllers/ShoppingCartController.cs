using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetStoreDemo.Data;
using AspNetStoreDemo.Extensions;
using AspNetStoreDemo.Models;
using AspNetStoreDemo.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetStoreDemo.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;
            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Products = new List<Products>()
            };
        }

        //GET Index ShoppingCart
        public IActionResult Index()
        {
            //grab items from session
            List<int> listShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            if (listShoppingCart.Count > 0)
            {
                foreach (int cartItem in listShoppingCart)
                {
                    //call db and populate products
                    Products products = _db.Products
                        .Include(p => p.SpecialTags)
                        .Include(p => p.ProductTypes)
                        .Where(p => p.Id == cartItem)
                        .FirstOrDefault();
                    ShoppingCartVM.Products.Add(products);
                }
            }
            return View(ShoppingCartVM);
        }

        //POST
        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            //retrieve from session the list of cart items
            List<int> listCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            //merge appointment date and time to the apt object
            ShoppingCartVM.Appointments.AppointmentDate = ShoppingCartVM.Appointments.AppointmentDate
                .AddHours(ShoppingCartVM.Appointments.AppointmentTime.Hour)
                .AddMinutes(ShoppingCartVM.Appointments.AppointmentTime.Minute);

            //create apt obj
            Appointments appointments = ShoppingCartVM.Appointments;

            //add apt to db
            _db.Appointments.Add(appointments);
            _db.SaveChanges();

            //get id, use to insert onto table for products selected for the apt
            int appointmentId = appointments.Id;

            foreach (int productId in listCartItems)
            {
                ProductsSelectedForAppointment productsSelectedForAppointment = new ProductsSelectedForAppointment()
                {
                    AppointmentId = appointmentId,
                    ProductId = productId
                };

                _db.ProductSelectedForAppointment.Add(productsSelectedForAppointment);
            }

            _db.SaveChanges();

            //empty contents of cart and clear the session
            listCartItems = new List<int>();
            HttpContext.Session.Set("ssShoppingCart", listCartItems);

            return RedirectToAction("AppointmentConfirmation", "ShoppingCart", new { Id = appointmentId });
        }

        public IActionResult Remove(int id)
        {
            //get current cart items
            List<int> listCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            if (listCartItems.Count > 0)
            {
                if (listCartItems.Contains(id))
                {
                    listCartItems.Remove(id);
                }
            }

            //update session
            HttpContext.Session.Set("ssShoppingCart", listCartItems);

            return RedirectToAction(nameof(Index));
        }

        //GET
        public IActionResult AppointmentConfirmation(int id)
        {
            //populate viewmodel based on apt id
            ShoppingCartVM.Appointments = _db.Appointments.Where(a => a.Id == id).FirstOrDefault();
            List<ProductsSelectedForAppointment> productList = _db.ProductSelectedForAppointment.Where(p => p.AppointmentId == id).ToList();

            foreach (ProductsSelectedForAppointment product in productList)
            {
                ShoppingCartVM.Products.Add(_db.Products.Include(p => p.ProductTypes)
                                                        .Include(p => p.SpecialTags)
                                                        .Where(p => p.Id == product.ProductId)
                                                        .FirstOrDefault());
            }

            return View(ShoppingCartVM);
        }
    }
}