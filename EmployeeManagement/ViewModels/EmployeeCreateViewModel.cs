using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class EmployeeCreateViewModel
    {
        public int Id { get; set; }
        [Required, MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
        ErrorMessage = "Invalid email format")]
        [Display(Name = "Office Email")]
        public string Email { get; set; }

        //by putting a question mark(?) next to the Dept, it allow to accept null value
        // by default enum Consider values as Integer values and Dept has all value is integer because it is enum type..
        [Required]
        public Dept? Department { get; set; }

        public IFormFile Photo { get; set; }

    }
}
