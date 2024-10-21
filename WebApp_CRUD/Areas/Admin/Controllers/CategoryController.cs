using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using NToastNotify;
using WebApp_CRUD.Data;
using X.PagedList;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly DataAppMvcContext _context;
        private readonly IToastNotification _toastNotification;
        public CategoryController(DataAppMvcContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        // GET: Admin/Category
        public async Task<IActionResult> Index(string searchName,int page = 1)
        {
            // Thêm thông báo chào mừng
            _toastNotification.AddSuccessToastMessage("Chào mừng đến với Category!");

            // Kích thước trang
            int pageSize = 5;

            // Bắt đầu bằng một truy vấn IQueryable
            var categoriesQuery = _context.Categories.AsQueryable();

            // Nếu có chuỗi tìm kiếm, lọc danh sách dựa trên tên
            if (!string.IsNullOrEmpty(searchName))
            {
                categoriesQuery = categoriesQuery.Where(c => c.Name.Contains(searchName));
            }

            // Chuyển đổi IQueryable thành danh sách trước khi phân trang
            var categoriesList = await categoriesQuery.ToListAsync();

            // Áp dụng phân trang sau khi đã lọc
            var categoriesPaged = categoriesList.ToPagedList(page, pageSize);

            return View(categoriesPaged);
        }

        // GET: Admin/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Slug,Status")] Category category)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem danh mục có tồn tại trong cơ sở dữ liệu không
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                {
                    // Thêm thông báo lỗi vào ModelState nếu tên đã tồn tại
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại. Vui lòng nhập lại.");
                    return View(category); // Trả về view với model đã nhập
                }

                // Tạo slug từ tên của danh mục
                category.Slug = SlugHelper.GenerateSlug(category.Name);

                // Lưu vào database
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(); // Sử dụng SaveChangesAsync để lưu bất đồng bộ

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Slug,Status")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
