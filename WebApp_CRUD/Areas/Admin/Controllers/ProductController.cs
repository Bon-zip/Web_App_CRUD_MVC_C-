using System;
using System.Collections.Generic;
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
    public class ProductController : Controller
    {
        private readonly DataAppMvcContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IToastNotification _toastNotification;

        public ProductController(DataAppMvcContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IConfiguration configuration, IToastNotification toastNotification)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _toastNotification = toastNotification;
        }

        // GET: Admin/Product
        public async Task<IActionResult> Index(string searchName, int page = 1)
        {
            _toastNotification.AddSuccessToastMessage("Chào mừng đến với trang Product!");
            // Kích thước trang
            int pageSize = 5;

            // Bắt đầu bằng một truy vấn IQueryable
            var productsQuery = _context.Products.Include(p => p.Brand).Include(p => p.Category).AsQueryable();

            // Nếu có chuỗi tìm kiếm, lọc danh sách dựa trên tên
            if (!string.IsNullOrEmpty(searchName))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchName));
            }

            // Chuyển đổi IQueryable thành danh sách trước khi phân trang
            var productsList = await productsQuery.ToListAsync();

            // Áp dụng phân trang sau khi đã lọc
            var productsPaged = productsList.ToPagedList(page, pageSize);

            return View(productsPaged);
        }

        // GET: Admin/Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var product = _context.Products
                               .Include(p => p.Category)
                               .Include(p => p.Brand)
                               .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Product/Create
        public IActionResult Create()
        {
            var cates = _context.Categories.ToList();
            var brands = _context.Brands.ToList();
            ViewBag.Categories = cates;
            ViewBag.Brands = brands;

            return View();
        }

        // POST: Admin/Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile fileUpload, [Bind("Id,Name,Description,Slug,Price,BrandId,CategoryId")] Product product)
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
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/uploads/products");
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
                    product.Images = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("ProductImage", "Hình ảnh không được để trống.");
            }

            // Kiểm tra tính hợp lệ của ModelState trước khi lưu vào cơ sở dữ liệu
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           
            // Trả lại View nếu có lỗi

            return View(product);
        }
        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            // Truyền tên hình ảnh hiện tại vào ViewBag để hiển thị
            ViewBag.CurrentImage = product.Images; // tên hình ảnh hiện tại
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile fileUpload, [Bind("Id,Name,Description,Slug,Price,BrandId,CategoryId")] Product model)
        {
            
            var product = _context.Products.Find(id); 
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem có tệp tải lên mới không
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
                            var uploadsFolder = Path.Combine(rootPath, "wwwroot/uploads/products");
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
                            product.Images = uniqueFileName;
                        }
                    }
                    else
                    {
                        product.Images = product.Images;
                    }

                    product.Name = model.Name != null ? model.Name : product.Name;
                    product.Description = model.Description != null ? model.Description : product.Description;
                    product.Slug = model.Slug != null ? model.Slug : product.Slug;
                    product.Price = model.Price != null ? model.Price : product.Price;
                    product.BrandId = model.BrandId != null ? model.BrandId : product.BrandId;
                    product.CategoryId = model.CategoryId != null ? model.CategoryId : product.CategoryId;

                    // Cập nhật sản phẩm trong cơ sở dữ liệu
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("ProductImage", "Đã xảy ra lỗi khi cập nhật sản phẩm. Vui lòng thử lại.");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            
            return View(product);
        }
        // GET: Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            }

            // Chuyển hướng về trang Index sau khi xóa thành công
            return RedirectToAction(nameof(Index));
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
