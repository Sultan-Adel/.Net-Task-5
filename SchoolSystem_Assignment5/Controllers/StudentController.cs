using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;
using SchoolSystem.ViewModels;

namespace SchoolSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly SchoolDbContext _context;

        public StudentController(SchoolDbContext context)
        {
            _context = context;
        }

        private async Task<List<SelectListItem>> GetDepartmentsSelectListAsync(int? selectedId = null)
        {
            return await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = selectedId.HasValue && d.Id == selectedId.Value
                })
                .ToListAsync();
        }

        // Remote Validation Action for Student Name
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerifyStudentName(string name, int id)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(true);

            var exists = await _context.Students
                .AnyAsync(s => s.Name.ToLower() == name.Trim().ToLower() && s.Id != id);

            if (exists)
            {
                return Json($"Student '{name}' already exists in the system.");
            }

            return Json(true);
        }

        // Index View linked to the Custom Template Layout (also forwards to GetAll logic)
        [HttpGet]
        public async Task<IActionResult> Index(string? searchName, int? departmentId, int pageNumber = 1, int pageSize = 5)
        {
            var viewModel = await BuildStudentListViewModelAsync(searchName, departmentId, pageNumber, pageSize);
            return View(viewModel);
        }

        // GET: /Student/GetAll?searchName=...&departmentId=...&pageNumber=1&pageSize=5
        [HttpGet]
        public async Task<IActionResult> GetAll(string? searchName, int? departmentId, int pageNumber = 1, int pageSize = 5)
        {
            var viewModel = await BuildStudentListViewModelAsync(searchName, departmentId, pageNumber, pageSize);
            return View(viewModel);
        }

        private async Task<StudentListViewModel> BuildStudentListViewModelAsync(string? searchName, int? departmentId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var query = _context.Students
                .Include(s => s.Department)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var term = searchName.Trim();
                query = query.Where(s => s.Name.Contains(term));
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new StudentListViewModel
            {
                Students = students,
                SearchName = searchName,
                DepartmentId = departmentId,
                Departments = await GetDepartmentsSelectListAsync(departmentId),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // GET: /Student/GetById/3
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.StuCrsRes)
                    .ThenInclude(sc => sc.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: /Student/Add
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var viewModel = new StudentFormViewModel
            {
                Departments = await GetDepartmentsSelectListAsync()
            };

            return View(viewModel);
        }

        // POST: /Student/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(StudentFormViewModel viewModel)
        {
            // Server-side custom validation
            if (!string.IsNullOrWhiteSpace(viewModel.Name))
            {
                bool nameExists = await _context.Students
                    .AnyAsync(s => s.Name.ToLower() == viewModel.Name.Trim().ToLower());
                if (nameExists)
                {
                    ModelState.AddModelError(nameof(viewModel.Name), "Student Name already exists.");
                }
            }

            bool departmentExists = await _context.Departments.AnyAsync(d => d.Id == viewModel.DepartmentId);
            if (!departmentExists)
            {
                ModelState.AddModelError(nameof(viewModel.DepartmentId), "Selected department does not exist.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.Departments = await GetDepartmentsSelectListAsync(viewModel.DepartmentId);
                return View(viewModel);
            }

            var student = new Student
            {
                Name = viewModel.Name.Trim(),
                Age = viewModel.Age,
                DepartmentId = viewModel.DepartmentId
            };

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student added successfully!";
            return RedirectToAction(nameof(GetAll));
        }

        // GET: /Student/Edit/3
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            var viewModel = new StudentFormViewModel
            {
                Id = student.Id,
                Name = student.Name,
                Age = student.Age,
                DepartmentId = student.DepartmentId,
                Departments = await GetDepartmentsSelectListAsync(student.DepartmentId)
            };

            return View(viewModel);
        }

        // POST: /Student/Edit/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Name))
            {
                bool nameExists = await _context.Students
                    .AnyAsync(s => s.Name.ToLower() == viewModel.Name.Trim().ToLower() && s.Id != id);
                if (nameExists)
                {
                    ModelState.AddModelError(nameof(viewModel.Name), "Another student with this name already exists.");
                }
            }

            bool departmentExists = await _context.Departments.AnyAsync(d => d.Id == viewModel.DepartmentId);
            if (!departmentExists)
            {
                ModelState.AddModelError(nameof(viewModel.DepartmentId), "Selected department does not exist.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.Departments = await GetDepartmentsSelectListAsync(viewModel.DepartmentId);
                return View(viewModel);
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            student.Name = viewModel.Name.Trim();
            student.Age = viewModel.Age;
            student.DepartmentId = viewModel.DepartmentId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student updated successfully!";
            return RedirectToAction(nameof(GetAll));
        }

        // GET: /Student/Delete/3
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View("DeleteWarning", student);
        }

        // POST: /Student/DeleteConfirmed/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students
                .Include(s => s.StuCrsRes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            if (student.StuCrsRes.Any())
            {
                _context.StuCrsRes.RemoveRange(student.StuCrsRes);
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student deleted successfully!";
            return RedirectToAction(nameof(GetAll));
        }
    }
}
