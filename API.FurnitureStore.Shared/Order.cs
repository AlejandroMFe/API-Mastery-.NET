namespace API.FurnitureStore.Shared;
public class Order
{
    public int Id { get; set; }
    public int OrderNumber { get; set; }
    public int ClientId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public List<OrderDetail> OrderDetails { get; set; }
}
