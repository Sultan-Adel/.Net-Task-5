using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;
using SchoolSystem.ViewModels;

namespace SchoolSystem.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly SchoolDbContext _context;

        public DepartmentController(SchoolDbContext context)
        {
            _context = context;
        }

        private async Task<List<SelectListItem>> GetDepartmentsSelectListAsync(int? selectedId = null, int? excludeId = null)
        {
            var query = _context.Departments.AsNoTracking().AsQueryable();
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return await query
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Name} (Mgr: {d.MgrName})",
                    Selected = selectedId.HasValue && d.Id == selectedId.Value
                })
                .ToListAsync();
        }

        // Remote Validation Action for Department Name
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerifyDepartmentName(string name, int id)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(true);

            var exists = await _context.Departments
                .AnyAsync(d => d.Name.ToLower() == name.Trim().ToLower() && d.Id != id);

            if (exists)
            {
                return Json($"Department '{name}' already exists.");
            }

            return Json(true);
        }

        // GET: /Department/ShowAll
        [HttpGet]
        public async Task<IActionResult> ShowAll()
        {
            var departments = await _context.Departments
                .Include(d => d.Students)
                .AsNoTracking()
                .ToListAsync();

            return View(departments);
        }

        // GET: /Department/Add
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var viewModel = new DepartmentFormViewModel
            {
                ExistingDepartments = await GetDepartmentsSelectListAsync()
            };

            return View(viewModel);
        }

        // POST: /Department/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(DepartmentFormViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.Name))
            {
                bool exists = await _context.Departments
                    .AnyAsync(d => d.Name.ToLower() == viewModel.Name.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(viewModel.Name), "Department Name must be unique.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.ExistingDepartments = await GetDepartmentsSelectListAsync(viewModel.ParentDepartmentId);
                return View(viewModel);
            }

            var department = new Department
            {
                Name = viewModel.Name.Trim(),
                MgrName = viewModel.MgrName.Trim()
            };

            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Department added successfully!";
            return RedirectToAction(nameof(ShowAll));
        }

        // GET: /Department/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            var viewModel = new DepartmentFormViewModel
            {
                Id = department.Id,
                Name = department.Name,
                MgrName = department.MgrName,
                ExistingDepartments = await GetDepartmentsSelectListAsync(excludeId: department.Id)
            };

            return View(viewModel);
        }

        // POST: /Department/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Name))
            {
                bool exists = await _context.Departments
                    .AnyAsync(d => d.Name.ToLower() == viewModel.Name.Trim().ToLower() && d.Id != id);
                if (exists)
                {
                    ModelState.AddModelError(nameof(viewModel.Name), "Another department already uses this name.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.ExistingDepartments = await GetDepartmentsSelectListAsync(viewModel.ParentDepartmentId, excludeId: id);
                return View(viewModel);
            }

            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            department.Name = viewModel.Name.Trim();
            department.MgrName = viewModel.MgrName.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Department updated successfully!";
            return RedirectToAction(nameof(ShowAll));
        }
    }
}
