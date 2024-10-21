using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Diagnostics;
using System.Security.Cryptography;
using WebApp_CRUD.Data;
using WebApp_CRUD.Data.ViewModels;
using WebApp_CRUD.Helper;
using WebApp_CRUD.Models;
using WebApp_CRUD.Models.ViewModels;
using X.PagedList;

namespace WebApp_CRUD.Controllers
{
    #region HomeController
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataAppMvcContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly IMapper _mapper;


        public HomeController(ILogger<HomeController> logger, DataAppMvcContext context, IToastNotification toastNotification, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _toastNotification = toastNotification;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            var products = _context.Products
                                  .Include(p => p.Category)
                                  .Include(p => p.Brand)
                                  .ToList();

            if (products == null || !products.Any())
            {
                return View("NoProductsFound"); // Trả về một view thông báo không có sản phẩm
            }

            return View(products);
        }
        public IActionResult Shop(string searchString, string sortOrder, int? page)
        {
            var products = _context.Products
                             .Include(p => p.Category)
                             .Include(p => p.Brand)
                             .AsQueryable(); // Sử dụng AsQueryable để thực hiện truy vấn

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // Sắp xếp sản phẩm
            products = sortOrder switch
            {
                "popularity" => products.OrderByDescending(p => p.Popularity),
                "organic" => products.OrderBy(p => p.IsOrganic),
                _ => products.OrderBy(p => p.Name),
            };

            int pageSize = 9;
            int pageNumber = page ?? 1;
            var pagedList = products.ToPagedList(pageNumber, pageSize);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không có sản phẩm nào được tìm thấy.";
            }

            return View(pagedList);
        }
        public IActionResult Detail(int? id)
        {
            var product = _context.Products
                   .Include(p => p.Category)
                   .Include(p => p.Brand)
                   .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(); // If product not found, return 404
            }

            var relatedProducts = _context.Products
                                    .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                                    .ToList();

            var viewModel = new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Images = product.Images,
                Price = product.Price, // Sử dụng giá mặc định nếu Price là null
                Category = product.Category,
                Brand = product.Brand,
                RelatedProducts = relatedProducts
            };

            return View(viewModel);
        }
        public List<CartItem> cartItems => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Cart()
        {
            return View(cartItems);
        }
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var cart = cartItems ?? new List<CartItem>(); // Kiểm tra và khởi tạo giỏ hàng nếu chưa có
            var item = cart.SingleOrDefault(p => p.Id == id);

            if (item == null)
            {
                var product = _context.Products.SingleOrDefault(p => p.Id == id);

                if (product == null)
                {
                    _toastNotification.AddErrorToastMessage($"Không tìm thấy sản phẩm có mã {id}");
                    return RedirectToAction("Index");
                }

                // Thêm sản phẩm mới vào giỏ hàng
                item = new CartItem
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Image = product.Images ?? string.Empty,
                    Quantity = quantity
                };
                cart.Add(item);
                _toastNotification.AddSuccessToastMessage($"{product.Name} đã được thêm vào giỏ hàng.");
            }
            else
            {
                // Cập nhật số lượng nếu sản phẩm đã có trong giỏ hàng
                item.Quantity += quantity;  // Cộng thêm số lượng
                _toastNotification.AddSuccessToastMessage($"Số lượng của {item.Name} đã được cập nhật.");
            }

            // Cập nhật lại session giỏ hàng
            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            return RedirectToAction("Cart", "Home");
        }

        public IActionResult RemoveCart(int id)
        {
            var cart = cartItems;
            var item = cart.SingleOrDefault(p => p.Id == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }
            return RedirectToAction("Cart", "Home");

        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel model, IFormFile Avatar)
        {
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<User>(model);
                user.RandomKey = MyUtil.GenerateRanDomKey();
                user.Password = model.Password;
                user.Lever = 0;
                if (Avatar != null)
                {
                    user.Avatar = MyUtil.UploadAvatar(Avatar, "~/Upload/khachhang");
                }
                _context.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login", "Home");
            }

            return View(model);
        }
        #endregion
        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        public IActionResult Login(LoginViewModel model, string? returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                var checkAccount = _context.Users.FirstOrDefault(a => a.UserName == model.UserName);
                if (checkAccount != null)
                {
                    HttpContext.Session.SetInt32("Id", checkAccount.Id);
                    HttpContext.Session.SetString("Name", checkAccount.UserName);
                    HttpContext.Session.SetString("Avatar", checkAccount.Avatar != null ? checkAccount.Avatar : "null");
                    if (checkAccount.Active == 1)
                    {
                        if (checkAccount.Password == checkAccount.Password)
                        {
                            _toastNotification.AddSuccessToastMessage("Đăng nhập thành công!");
                            if (Url.IsLocalUrl(returnUrl))
                                return Redirect(returnUrl);
                            else
                                return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            TempData["error"] = "Sai mật khẩu!";
                            return View(model);
                        }
                    }
                    else
                    {
                        TempData["error"] = "Tài khoản đã bị khóa!";
                        return View(model);
                    }
                }
                else
                {
                    TempData["error"] = "Tài khoản không tồn tại!";
                    return View(model);
                }
            }
        }

        public IActionResult Logout()
        {
            _toastNotification.AddSuccessToastMessage("Đăng xuất thành công !");
            HttpContext.Session.Remove("Id");
            HttpContext.Session.Remove("Name");
            HttpContext.Session.Remove("Avatar");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
