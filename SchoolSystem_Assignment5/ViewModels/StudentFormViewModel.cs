using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolSystem.ViewModels
{
    public class StudentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Student Name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z؀-ۿ\s]+$", ErrorMessage = "Name must contain letters and spaces only.")]
        [Remote(action: "VerifyStudentName", controller: "Student", AdditionalFields = nameof(Id), ErrorMessage = "A student with this name already exists.")]
        [Display(Name = "Student Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required.")]
        [Range(16, 60, ErrorMessage = "Age must be between 16 and 60.")]
        [Display(Name = "Student Age")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Please select a department.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }
}
