using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SchoolSystem.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department Name must be between 2 and 100 characters.")]
        [Remote(action: "VerifyDepartmentName", controller: "Department", AdditionalFields = nameof(Id), ErrorMessage = "Department Name already exists.")]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manager Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Manager Name must be between 3 and 100 characters.")]
        [Display(Name = "Manager Name")]
        public string MgrName { get; set; } = string.Empty;

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
