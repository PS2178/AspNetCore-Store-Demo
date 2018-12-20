using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetStoreDemo.Data;
using AspNetStoreDemo.Models;
using AspNetStoreDemo.Models.ViewModel;
using AspNetStoreDemo.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetStoreDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDetails.AdminEndUser + "," + StaticDetails.SuperAdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET
        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
            //grab current user info
            ClaimsPrincipal currentUser = User;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Appointments>()
            };

            //show sales associate
            appointmentVM.Appointments = _db.Appointments.Include(a => a.SalesAssociate).ToList();

            //if admin, show associate
            if (User.IsInRole(StaticDetails.AdminEndUser))
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.SalesAssociateId == claim.Value).ToList();
            }

            //filter viewmodel based on criteria, if provided values
            if (searchName != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchEmail != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return View(appointmentVM);
        }

        //GET : Edit
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //populate products from apt info 
            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductSelectedForAppointment
                                                      on p.Id equals a.ProductId
                                                      where a.AppointmentId == id
                                                      select p)
                                                   .Include("ProductTypes");

            AppointmentDetailsViewModel appDetailsVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesAssociate).Where(a => a.Id == id).FirstOrDefault(),
                SalesAssociate = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(appDetailsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentDetailsViewModel appDetailsVM)
        {
            if (ModelState.IsValid)
            {
                //combine the apt date and time from view inside the apt date
                appDetailsVM.Appointment.AppointmentDate = appDetailsVM.Appointment.AppointmentDate
                    .AddHours(appDetailsVM.Appointment.AppointmentTime.Hour)
                    .AddMinutes(appDetailsVM.Appointment.AppointmentTime.Minute);

                //grab apt obj from db
                var appointmentFromDb = _db.Appointments.Where(a => a.Id == appDetailsVM.Appointment.Id).FirstOrDefault();
                appointmentFromDb.CustomerName = appDetailsVM.Appointment.CustomerName;
                appointmentFromDb.CustomerEmail = appDetailsVM.Appointment.CustomerEmail;
                appointmentFromDb.CustomerPhoneNumber = appDetailsVM.Appointment.CustomerPhoneNumber;
                appointmentFromDb.AppointmentDate = appDetailsVM.Appointment.AppointmentDate;
                appointmentFromDb.IsConfirmed = appDetailsVM.Appointment.IsConfirmed;

                //update sales associate if super admin
                if (User.IsInRole(StaticDetails.SuperAdminEndUser))
                {
                    appointmentFromDb.SalesAssociateId = appDetailsVM.Appointment.SalesAssociateId;
                }

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(appDetailsVM);
        }

        //GET : Details
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //populate products from apt info 
            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductSelectedForAppointment
                                                      on p.Id equals a.ProductId
                                                      where a.AppointmentId == id
                                                      select p)
                                                   .Include("ProductTypes");

            AppointmentDetailsViewModel appDetailsVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesAssociate).Where(a => a.Id == id).FirstOrDefault(),
                SalesAssociate = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(appDetailsVM);
        }

        //GET : Delete
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //populate products from apt info 
            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductSelectedForAppointment
                                                      on p.Id equals a.ProductId
                                                      where a.AppointmentId == id
                                                      select p)
                                                   .Include("ProductTypes");

            AppointmentDetailsViewModel appDetailsVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesAssociate).Where(a => a.Id == id).FirstOrDefault(),
                SalesAssociate = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(appDetailsVM);
        }

        //POST : Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}