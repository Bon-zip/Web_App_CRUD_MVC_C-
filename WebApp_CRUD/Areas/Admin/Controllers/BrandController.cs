using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using WebApp_CRUD.Data;
using X.PagedList;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BrandController : Controller
    {
        private readonly DataAppMvcContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IToastNotification _toastNotification;
        public BrandController(DataAppMvcContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IConfiguration configuration, IToastNotification toastNotification)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _toastNotification = toastNotification;
        }

        // GET: Admin/Brand
        public async Task<IActionResult> Index(string searchName,int page = 1)
        {
            _toastNotification.AddSuccessToastMessage("Chào mừng đến với trang Brands!");
            // Kích thước trang
            int pageSize = 5;

            // Bắt đầu bằng một truy vấn IQueryable
            var brandsQuery = _context.Brands.AsQueryable();

            // Nếu có chuỗi tìm kiếm, lọc danh sách dựa trên tên
            if (!string.IsNullOrEmpty(searchName))
            {
                brandsQuery = brandsQuery.Where(b => b.Name.Contains(searchName));
            }

            // Chuyển đổi IQueryable thành danh sách trước khi phân trang
            var brandsList = await brandsQuery.ToListAsync();

            // Áp dụng phân trang sau khi đã lọc
            var brandsPaged = brandsList.ToPagedList(page, pageSize);

            return View(brandsPaged);
        }

        // GET: Admin/Brand/Details/5
    /*    public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }
*/
        // GET: Admin/Brand/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Brand/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile fileUpload, [Bind("Id,Name,Description,Slug,Status")] Brand brand)
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
                    ModelState.AddModelError("BrandImage", "Hình ảnh không hợp lệ. Chỉ chấp nhận các định dạng: " + string.Join(", ", allowedExtensions) + ".");
                }
                else
                {
                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/uploads/brands");
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

                    // Lưu tên tệp vào thuộc tính Images của product
                    brand.Images = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("BrandImage", "Hình ảnh không được để trống.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Admin/Brand/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Admin/Brand/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile fileUpload, int id, [Bind("Id,Name,Description,Slug,Status")] Brand brand)
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
                    ModelState.AddModelError("BrandImage", "Hình ảnh không hợp lệ. Chỉ chấp nhận các định dạng: " + string.Join(", ", allowedExtensions) + ".");
                }
                else
                {
                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/uploads/brands");
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

                    // Lưu tên tệp vào thuộc tính Images của product
                    brand.Images = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("BrandImage", "Hình ảnh không được để trống.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
