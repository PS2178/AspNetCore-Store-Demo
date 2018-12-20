using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetStoreDemo.Models.ViewModel
{
    public class AppointmentDetailsViewModel
    {
        public Appointments Appointment { get; set; }
        public List<ApplicationUser> SalesAssociate { get; set; }
        public List<Products> Products { get; set; }
    }
}
