namespace API.FurnitureStore.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly APIFurnitureStoreContext _context;

    public ProductsController(APIFurnitureStoreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Product>> Get()
    {
        return await _context.Products.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

        if (product is null) return NotFound();

        return CreatedAtAction("Post", product.Id, product);
    }

    [HttpGet("GetByCategory/{categoryId}")]
    public async Task<IEnumerable<Product>> GetByCategory(int categoryId)
    {
       return await _context.Products
                            .Where(p => p.ProductCategoryId == categoryId)
                            .ToListAsync();
    }


    [HttpPost]
    public async Task<IActionResult> Post(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPut]
    public async Task<IActionResult> Put(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

        if (product is null) return NotFound();

        _context.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
