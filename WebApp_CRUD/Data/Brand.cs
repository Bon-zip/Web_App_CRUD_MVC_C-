using System;
using System.Collections.Generic;

namespace WebApp_CRUD.Data;

public partial class Brand
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
    public string? Images { get; set; }

    public string? Slug { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
