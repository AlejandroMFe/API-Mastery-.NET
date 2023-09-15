
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtConfig>(
    builder.Configuration.GetSection(nameof(JwtConfig)));

builder.Services.AddDbContext<APIFurnitureStoreContext>(options =>
            // options pass the configurations to the DbContext Class aka APIFurnitureStoreContext
            options.UseSqlite(builder.Configuration.GetConnectionString("APIFurnitureStoreContext")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
