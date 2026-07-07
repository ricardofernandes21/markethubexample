namespace ProducServiceDomain.Entities;

public class Products
{
    public Guid Id { get;  set; }
    
    public decimal Price { get;  set; }

    public int Stock { get;  set; }


    public void UpdateStock(int quantity)
    {
        if(quantity < 0)
            throw new Exception("Stock cannot be negative");

        Stock = quantity;
    }
}