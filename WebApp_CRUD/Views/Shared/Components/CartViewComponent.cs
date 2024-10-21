using Microsoft.AspNetCore.Mvc;
using WebApp_CRUD.Helper;
using WebApp_CRUD.Models.ViewModels;

namespace WebApp_CRUD.Views.Shared.Components
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
            return View("CartPanel", new CartModel
            {
                Quantity = cart.Sum(x => x.Quantity),
                Total = cart.Sum(x => x.PriceTotal)
            });
        }
    }
}
