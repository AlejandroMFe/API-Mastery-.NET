// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        // Add Authorization header to Swagger
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Furniture_Store_API",
            Version = "v1"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
        });
    });

    builder.Services.AddDbContext<APIFurnitureStoreContext>(options =>
                // options pass the configurations to the DbContext Class aka APIFurnitureStoreContext
                options.UseSqlite(builder.Configuration.GetConnectionString("APIFurnitureStoreContext")));

    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(nameof(JwtConfig)));

    /* Email */
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(nameof(SmtpSettings)));
    builder.Services.AddSingleton<IEmailSender, EmailService>();

    /* Token */
    var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);
    var tokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        // Issuer is how emitted the token
        // in production this will be true
        ValidateIssuer = false,

        // Audience is how recieved the token
        ValidateAudience = false,
        RequireExpirationTime = false,
        ValidateLifetime = true
    };

    builder.Services.AddSingleton(tokenValidationParameters);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });

    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
         options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<APIFurnitureStoreContext>();

    /* NLog */
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    /* Swagger */
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    logger.Error(e, "Therer has been an error");
    throw;
}
finally
{
    LogManager.Shutdown();
}
