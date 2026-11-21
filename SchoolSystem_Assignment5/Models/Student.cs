using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Student Name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required.")]
        [Range(16, 60, ErrorMessage = "Age must be between 16 and 60.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Department.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<StuCrsRes> StuCrsRes { get; set; } = new List<StuCrsRes>();
    }
}
