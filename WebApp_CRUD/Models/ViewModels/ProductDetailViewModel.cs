namespace WebApp_CRUD.Data.ViewModels
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public decimal Price { get; set; }
        public Category Category { get; set; }
        public Brand Brand { get; set; }

        // Related products
        public List<Product> RelatedProducts { get; set; }
    }
}
