using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Security.Claims;
using WebApp_CRUD.Data;
using WebApp_CRUD.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using WebApp_CRUD.Helper;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly DataAppMvcContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly ILogger<LoginController> _logger; // Thêm logger

        public LoginController(DataAppMvcContext context, IToastNotification toastNotification, ILogger<LoginController> logger)
        {
            _context = context;
            _toastNotification = toastNotification;
            _logger = logger; // Gán logger
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User model, string? returnUrl)
        {
            try
            {
                var acc = _context.Users.FirstOrDefault(x => x.UserName == model.UserName);
                if (acc != null)
                {
                    if (acc.Password == model.Password) // Chỉnh sửa để không truyền tham số model.Password
                    {
                        if (acc.Active == 1)
                        {
                            if (acc.Lever == 1)
                            {
                                var identity = new ClaimsIdentity(new[]
                                {
                                    new Claim("userId", acc.Id.ToString()),
                                    new Claim(ClaimTypes.Name, acc.UserName),
                                    new Claim("fullName", acc.FirstName + " " + acc.LastName),
                                    new Claim("avatar", acc.Avatar ?? "default.png"),
                                    new Claim(ClaimTypes.Role, "Admin"),
                                }, CookieAuthenticationDefaults.AuthenticationScheme);

                                var principal = new ClaimsPrincipal(identity);
                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                                _toastNotification.AddSuccessToastMessage("Đăng nhập thành công!");
                                if (!string.IsNullOrEmpty(returnUrl))
                                {
                                    return Redirect(returnUrl);
                                }
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                ModelState.AddModelError("UserName", "Không có quyền truy cập!");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("UserName", "Tài khoản đã bị khóa!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Mật khẩu không chính xác!");
                    }
                }
                else
                {
                    ModelState.AddModelError("UserName", "Tài khoản không tồn tại!");
                }
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi
                _logger.LogError(ex, "Một lỗi đã xảy ra trong quá trình đăng nhập.");
                ModelState.AddModelError("", "Đã xảy ra một lỗi không mong muốn.");
            }

            // Đảm bảo trả về giá trị cuối cùng nếu có lỗi
            return View("Index", model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            // Kiểm tra xem returnUrl có hợp lệ và không phải trang đăng nhập
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("Login"))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessRestricted()
        {
            // Nếu người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập với returnUrl
            if (!User.Identity.IsAuthenticated)
            {
                var returnUrl = Url.Action("AccessRestricted"); // Đường dẫn đến trang hiện tại
                return RedirectToAction("Login", "Login", new { returnUrl });
            }

            return View();
        }
    }
}
