using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolSystem.ViewModels
{
    public class DepartmentFormViewModel
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

        [Display(Name = "Copy / Reference Existing Department (Optional)")]
        public int? ParentDepartmentId { get; set; }

        public List<SelectListItem> ExistingDepartments { get; set; } = new List<SelectListItem>();
    }
}
