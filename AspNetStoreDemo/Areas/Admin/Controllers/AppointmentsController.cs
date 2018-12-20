using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetStoreDemo.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetStoreDemo.Areas.Admin.Controllers
{
    public class AppointmentsController : Controller
    {
        [Authorize(Roles=StaticDetails.SuperAdminEndUser + "," + StaticDetails.AdminEndUser)]
        [Area("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}