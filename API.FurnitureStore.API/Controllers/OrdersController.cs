namespace API.FurnitureStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly APIFurnitureStoreContext _context;

    public OrdersController(APIFurnitureStoreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Order>> Get()
    {
        return await _context.Orders.Include(o => o.OrderDetails).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetails(int id)
    {
        var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Order order)
    {
        if (order.OrderDetails.Count == 0 || order.OrderDetails is null)
            return BadRequest("Order must have at least one details");

        await _context.Orders.AddAsync(order);
        await _context.OrderDetails.AddRangeAsync(order.OrderDetails);

        await _context.SaveChangesAsync();

        return CreatedAtAction("Post", order.Id, order);
    }

    [HttpPut]
    public async Task<IActionResult> Put(Order order)
    {
        if (order is null) return NotFound();
        if (order.Id <= 0) return NotFound();

        var existingOrder = await _context.Orders
                                          .Include(o => o.OrderDetails)
                                          .FirstOrDefaultAsync(o => o.Id == order.Id);

        if (existingOrder is null) return NotFound();

        existingOrder.OrderNumber = order.OrderNumber;
        existingOrder.OrderDate = order.OrderDate;
        existingOrder.DeliveryDate = order.DeliveryDate;
        existingOrder.ClientId = order.ClientId;

        _context.OrderDetails.RemoveRange(existingOrder.OrderDetails);

        _context.Orders.Update(existingOrder);
        _context.OrderDetails.AddRange(order.OrderDetails);

        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int orderId)
    {
        if (orderId is <= 0) return NotFound();

        var existingOrder = await _context.Orders
                                          .Include(o => o.OrderDetails)
                                          .FirstOrDefaultAsync(o => o.Id == orderId);
        if (existingOrder is null) return NotFound();

        // First remevo the details and then the master
        _context.OrderDetails.RemoveRange(existingOrder.OrderDetails);
        _context.Orders.Remove(existingOrder);

        await _context.SaveChangesAsync();

        return  NoContent();
    }
}
