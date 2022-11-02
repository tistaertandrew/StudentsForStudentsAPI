using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentsForStudentsAPI;
using StudentsForStudentsAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var builderConf = new ConfigurationBuilder()
                 .SetBasePath(builder.Environment.ContentRootPath)
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                 .AddEnvironmentVariables();

var Configuration = builderConf.Build();

var connectionString = Configuration.GetConnectionString("default");

builder.Services.AddCors(p => p.AddPolicy("StudentsForStudents", builder =>
{
    builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
    //builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
}).AddEntityFrameworkStores<DatabaseContext>();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("18b9c42a-55d2-11ed-bdc3-0242ac120002")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

    await Seed.AddDefaultRole(roleManager);
}

app.UseHttpsRedirection();
app.UseCors("StudentsForStudents");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
