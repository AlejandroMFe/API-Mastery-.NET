namespace API.FurnitureStore.Shared;
public class OrderDetail
{
    // Compound Key by OderId + 
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
