using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using WebApp_CRUD.Data;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IToastNotification _toastNotification;
        public HomeController( IToastNotification toastNotification)
        {
            _toastNotification = toastNotification;
        }

        public IActionResult Index()
        {
            _toastNotification.AddSuccessToastMessage("Chào mừng đến với Admin Web App!");
            return View();
        }
    }
}
