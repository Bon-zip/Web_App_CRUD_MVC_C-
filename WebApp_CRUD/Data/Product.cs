using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp_CRUD.Data;

public partial class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Slug { get; set; }

    [Required]
    [Display(Name = "Price")]
    [DisplayFormat(DataFormatString = "{0:#,##0₫}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }

    public int? BrandId { get; set; }

    public int? CategoryId { get; set; }

    public string? Images { get; set; }

    public bool IsOrganic { get; set; } // Thêm thuộc tính IsOrganic
    public int Popularity { get; set; } // Thêm thuộc tính Popularity

    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }
}
