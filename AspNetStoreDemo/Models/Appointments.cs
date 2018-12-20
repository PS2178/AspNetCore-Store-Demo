using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetStoreDemo.Models
{
    public class Appointments
    {
        public int Id { get; set; }

        [Display(Name ="Sales Associate")]
        public string SalesAssociateId { get; set; }

        [ForeignKey("SalesAssociateId")]
        public virtual ApplicationUser SalesAssociate { get; set; }

        public DateTime AppointmentDate { get; set; }

        [NotMapped]
        public DateTime AppointmentTime { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerEmail { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
