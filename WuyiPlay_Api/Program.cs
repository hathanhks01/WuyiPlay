using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using WuyiPlay_Api.Configurations;
using WuyiPlay_BLL.IServices;
using WuyiPlay_BLL.MappingProfiles;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {your_token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend", policy =>
//        policy.WithOrigins("http://localhost:3000", "http://localhost:5173","https://wuyi-play-fe.vercel.app")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials());
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .AllowAnyOrigin() // 🔥 test
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});
builder.Services.AddDbContext<WuyiPlayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<ProductImageRepository>();
builder.Services.AddScoped<BalanceAuditLogRepository>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductImageService>();
builder.Services.AddScoped<BalanceAuditLogService>();
builder.Services.AddScoped<ILoggerService, LoggerService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

_ = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// ============================================================
// Serve file tĩnh — tự động theo StorageMode trong config
// ============================================================
var storageMode = builder.Configuration["FileStorage:StorageMode"] ?? "external";
var uploadPath = builder.Configuration["FileStorage:UploadPath"] ?? "uploads";
var publicPath = builder.Configuration["FileStorage:PublicPath"] ?? "/uploads";

if (storageMode == "wwwroot")
{
    // Lưu trong wwwroot → ASP.NET Core tự serve, không cần setup thêm
    // Chỉ cần đảm bảo thư mục tồn tại
    var wwwrootUpload = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", uploadPath);
    Directory.CreateDirectory(wwwrootUpload);

    // UseStaticFiles mặc định đã serve wwwroot rồi, không cần thêm gì
    app.UseStaticFiles();
}
else
{
    // Lưu ngoài wwwroot (external/VPS) → cần PhysicalFileProvider
    var absoluteUploadPath = Path.IsPathRooted(uploadPath)
        ? uploadPath
        : Path.Combine(builder.Environment.ContentRootPath, uploadPath);

    Directory.CreateDirectory(absoluteUploadPath);

    // Serve wwwroot trước (css, js, favicon...)
    app.UseStaticFiles();

    // Serve thêm thư mục upload ngoài
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(absoluteUploadPath),
        RequestPath = publicPath
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
