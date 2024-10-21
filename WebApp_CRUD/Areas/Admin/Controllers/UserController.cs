using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using WebApp_CRUD.Data;
using WebApp_CRUD.Models.ViewModels;
using X.PagedList;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly DataAppMvcContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IToastNotification _toastNotification;
        public UserController(DataAppMvcContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IConfiguration configuration, IToastNotification toastNotification)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _toastNotification = toastNotification;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index(string searchName, int page = 1)
        {
            _toastNotification.AddSuccessToastMessage("Chào mừng đến với trang Account!");
            // Kích thước trang
            int pageSize = 7;

            // Bắt đầu bằng một truy vấn IQueryable
            var usersQuery = _context.Users.AsQueryable();

            // Nếu có chuỗi tìm kiếm, lọc danh sách dựa trên tên
            if (!string.IsNullOrEmpty(searchName))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchName));
            }

            // Chuyển đổi IQueryable thành danh sách trước khi phân trang
            var usersList = await usersQuery.ToListAsync();

            // Áp dụng phân trang sau khi đã lọc
            var usersPaged = usersList.ToPagedList(page, pageSize);
            return View(usersPaged);
        }

      /*  // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
*/
        // GET: Admin/User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile fileUpload, [Bind("Id,UserName,Password,FirstName,LastName,Address,Avatar,Phone,Gender,Lever,Active")] User user)
        {
            if (fileUpload != null)
            {
                // Đường dẫn gốc của dự án
                var rootPath = _environment.ContentRootPath;

                // Các phần mở rộng tệp được cho phép
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Lấy phần mở rộng của tệp
                var fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("UserAvatar", "Hình ảnh không hợp lệ. Chỉ chấp nhận các định dạng: " + string.Join(", ", allowedExtensions) + ".");
                }
                else
                {
                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/uploads/users");
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
                    user.Avatar = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("UserAvatar", "Hình ảnh không được để trống.");
            }

            // Kiểm tra tính hợp lệ của ModelState trước khi lưu vào cơ sở dữ liệu
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Trả lại View nếu có lỗi

            return View(user);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile fileUpload, int id, [Bind("Id,UserName,FirstName,LastName,Address,Avatar,Phone,Gender,Lever,Active")] User user)
        {
            if (fileUpload != null)
            {
                // Đường dẫn gốc của dự án
                var rootPath = _environment.ContentRootPath;

                // Các phần mở rộng tệp được cho phép
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Lấy phần mở rộng của tệp
                var fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("UserAvatar", "Hình ảnh không hợp lệ. Chỉ chấp nhận các định dạng: " + string.Join(", ", allowedExtensions) + ".");
                }
                else
                {
                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(rootPath, "wwwroot/uploads/users");
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
                    user.Avatar = uniqueFileName;
                }
            }
            else
            {
                ModelState.AddModelError("UserAvatar", "Hình ảnh không được để trống.");
            }

            // Kiểm tra tính hợp lệ của ModelState trước khi lưu vào cơ sở dữ liệu
            if (ModelState.IsValid)
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Trả lại View nếu có lỗi

            return View(user);
        }
        // GET: Admin/User/ChangePassword/5
        public IActionResult ChangePassword(string userId)
        {
            var model = new ChangePasswordViewModel { UserId = userId };
            return View(model);
        }

      

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
