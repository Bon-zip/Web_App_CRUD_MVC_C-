using System.Text.RegularExpressions;

namespace WebApp_CRUD.Areas.Admin.Controllers
{
    public class SlugHelper
    {
        public static string GenerateSlug(string name)
        {
            // Chuyển tên sang chữ thường
            string slug = name.ToLowerInvariant();

            // Loại bỏ các ký tự không phải chữ hoặc số (trừ khoảng trắng)
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Thay khoảng trắng và dấu cách bằng dấu gạch ngang
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            return slug;
        }
    }
}
