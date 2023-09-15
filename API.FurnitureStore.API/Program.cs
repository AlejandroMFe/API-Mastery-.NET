var builder = WebApplication.CreateBuilder(args);

var furnitureStoreSettings = builder.Configuration.GetSection("APIFurnitureStore").Get<APIFurnitureStoreSettings>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<APIFurnitureStoreContext>(options =>
            // options pass the configurations to the DbContext Class aka APIFurnitureStoreContext
            options.UseSqlite(furnitureStoreSettings.ConnectionStrings));

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
