using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace WebApp_CRUD.Data;

public partial class User
{
    // Các thuộc tính mở rộng của người dùng
    public int Id { get; set; }
    public string? UserName { get; set; } // Tên
    public string? FirstName { get; set; } // Tên
    public string? LastName { get; set; }  // Họ
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }   // Địa chỉ
    public string? Avatar { get; set; }    // Đường dẫn ảnh đại diện
    public int? Gender { get; set; }       // Giới tính (0: Nam, 1: Nữ, 2: Khác)
    public int? Lever { get; set; }        // Lever (Phân quyền, cấp độ)
    public int? Active { get; set; }       // Trạng thái hoạt động (1: Hoạt động, 0: Vô hiệu)
    public string? RandomKey { get; internal set; }
}
