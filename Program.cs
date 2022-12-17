using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentsForStudentsAPI;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Services.MailService;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Services.UserService;

var builder = WebApplication.CreateBuilder(args);
var builderConf = new ConfigurationBuilder()
                 .SetBasePath(builder.Environment.ContentRootPath)
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                 .AddEnvironmentVariables();

var configuration = builderConf.Build();

var connectionString = configuration.GetConnectionString("default");

builder.Services.AddCors(p => p.AddPolicy("StudentsForStudents", builder =>
{
    builder.WithOrigins(configuration.GetSection("CorsURL").Value).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSignalR();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
}).AddEntityFrameworkStores<DatabaseContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:JwtSecret").Value)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "StudentsForStudentsAPI",
        Description = "Une API pour l'application web et mobile StudentsForStudents",
    });
    
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization Header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
    var config = scope.ServiceProvider.GetService<IConfiguration>();

    await Seed.AddDefaultRole(roleManager);
}

app.UseHttpsRedirection();
app.UseCors("StudentsForStudents");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<SignalRHub>("/Notifications");

app.Run();
