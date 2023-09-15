using API.FurnitureStore.Shared;

namespace API.FurnitureStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProducCategoriesController : ControllerBase
{
    private readonly APIFurnitureStoreContext _context;

    public ProducCategoriesController(APIFurnitureStoreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<ProductCategory>> Get()
    {
        return await _context.ProductCategories.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == id);

        if (productCategory is null) return NotFound();

        return Ok(productCategory);
    }

    [HttpPost]
    public async Task<IActionResult> Post(ProductCategory productCategory)
    {
        await _context.ProductCategories.AddAsync(productCategory);
        await _context.SaveChangesAsync();

        return CreatedAtAction("Post", productCategory.Id, productCategory);
    }

    [HttpPut]
    public async Task<IActionResult> Put(ProductCategory productCategory)
    {
        _context.ProductCategories.Update(productCategory);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(p => p.Id == id);

        if (productCategory is null) return NotFound();

        _context.ProductCategories.Remove(productCategory);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
