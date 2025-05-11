namespace SuperShop.Model;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public double UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
    public bool Discontinued { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
