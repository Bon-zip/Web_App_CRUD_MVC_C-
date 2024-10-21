using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NToastNotify;
using WebApp_CRUD.Data;
using X.PagedList;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BlogController : Controller
    {
        private readonly DataAppMvcContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IToastNotification _toastNotification;

        public BlogController(DataAppMvcContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IConfiguration configuration, IToastNotification toastNotification)
        {
            _toastNotification = toastNotification;
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        // GET: Admin/Blog
        public async Task<IActionResult> Index(string searchName, int page = 1)
        {
            // Thêm thông báo Toast
            _toastNotification.AddSuccessToastMessage("Chào mừng đến với trang Blogs!");

            // Kích thước trang
            int pageSize = 5;

            // Bắt đầu bằng một truy vấn IQueryable
            var blogsQuery = _context.Blogs.AsQueryable();

            // Nếu có chuỗi tìm kiếm, lọc danh sách dựa trên tên
            if (!string.IsNullOrEmpty(searchName))
            {
                blogsQuery = blogsQuery.Where(c => c.Name.Contains(searchName));
            }

            // Chuyển đổi IQueryable thành danh sách trước khi phân trang
            var blogsList = await blogsQuery.ToListAsync();

            // Áp dụng phân trang sau khi đã lọc
            var blogsPaged = blogsList.ToPagedList(page, pageSize);

            return View(blogsPaged);

        }

        // GET: Admin/Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Admin/Blog/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Blog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile fileUpload, [Bind("Id,Name,Images,Description")] Blog blog)
        {
            if (fileUpload != null)
            {
                // Đường dẫn gốc của dự án
                var rootPath = _environment.ContentRootPath;

                // Các phần mở rộng tệp được cho phép
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".webp" };

                // Lấy phần mở rộng của tệp
                var fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("BlogImage", "Hình ảnh không hợp lệ. Chỉ chấp nhận các định dạng: " + string.Join(", ", allowedExtensions) + ".");
                }
                else
                {
                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/Uploads/blogs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên tệp duy nhất
                    var fileName = Path.GetFileNameWithoutExtension(fileUpload.FileName);
                    var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu tệp lên máy chủ
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }

                    // Lưu tên tệp vào thuộc tính Images của blog
                    blog.Images = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("BlogImages", "Hình ảnh không được để trống.");
            }

            // Kiểm tra tính hợp lệ của ModelState trước khi lưu vào cơ sở dữ liệu
            if (ModelState.IsValid)
            {
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Admin/Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Admin/Blog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile fileUpload, int id, [Bind("Id,Name,Images,Description")] Blog blog)
        {
            if (fileUpload != null)
            {
                // Đường dẫn gốc của dự án
                var rootPath = _environment.ContentRootPath;

                // Các phần mở rộng tệp được cho phép
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                // Lấy phần mở rộng của tệp
                var fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("BlogImage", "Hình ảnh không hợp lệ. Chỉ chấp nhận các định dạng: " + string.Join(", ", allowedExtensions) + ".");
                }
                else
                {
                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/Uploads/blogs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên tệp duy nhất
                    var fileName = Path.GetFileNameWithoutExtension(fileUpload.FileName);
                    var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu tệp lên máy chủ
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }

                    // Lưu tên tệp vào thuộc tính Images của blog
                    blog.Images = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("BlogImages", "Hình ảnh không được để trống.");
            }

            // Kiểm tra tính hợp lệ của ModelState trước khi lưu vào cơ sở dữ liệu
            if (ModelState.IsValid)
            {
                _context.Update(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Admin/Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
