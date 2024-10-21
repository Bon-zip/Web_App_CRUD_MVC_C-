using System.Text;

namespace WebApp_CRUD.Helper
{
    public class MyUtil
    {
        public static string UploadAvatar(IFormFile formFile , string folder)
        {  // Kết hợp đường dẫn để lưu tệp
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "khachhang", folder);

            // Kiểm tra nếu thư mục không tồn tại thì tạo mới
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            // Tạo đường dẫn đầy đủ cho tệp hình ảnh
            var filePath = Path.Combine(fullPath, formFile.FileName);

            // Sử dụng FileStream để lưu tệp hình ảnh
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                // Sao chép nội dung của formFile vào luồng tệp
                formFile.CopyTo(stream);
            }

            // Trả về đường dẫn tệp đã lưu
            return filePath;
        }
        public static string GenerateRanDomKey(int  length = 5 )
        {
            var pattern = @"kjlhdfhsdfiuwehjbfdshfJKDSHFUIBSDHGFH!@#$%^&*()_)(*&";
            var sb = new StringBuilder();
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)] );
            }
            return sb.ToString();
        }
    }
}
