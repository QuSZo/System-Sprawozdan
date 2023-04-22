using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using SystemSprawozdan.Backend;
using SystemSprawozdan.Backend.Data;
using SystemSprawozdan.Backend.Data.Models.DbModels;
using SystemSprawozdan.Backend.Services;
using SystemSprawozdan.Backend.Middleware;
using SystemSprawozdan.Backend.Data.Seeder;
using SystemSprawozdan.Backend.Authorization;
using FluentValidation;
using SystemSprawozdan.Backend.Data.Models.Dto;
using SystemSprawozdan.Backend.Data.Models.Validators;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration["ConnectionString"];
#region AuthenticationSettings
var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);

builder.Services
    .AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = "Bearer";
        option.DefaultScheme = "Bearer";
        option.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.SaveToken = true;
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = authenticationSettings.JwtIssuer,
            ValidAudience = authenticationSettings.JwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
        };
    });
#endregion

// Add services to the container.
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<ApiDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddControllers().AddFluentValidation();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthorizationHandler, UserResourceOperationRequirementHandler>();
builder.Services.AddScoped<IPasswordHasher<Student>, PasswordHasher<Student>>();
builder.Services.AddScoped<IPasswordHasher<Teacher>, PasswordHasher<Teacher>>();
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();
builder.Services.AddScoped<IValidator<LoginUserDto>, LoginUserDtoValidator>();
builder.Services.AddScoped<IValidator<RegisterStudentDto>, RegisterStudentDtoValidator>();
builder.Services.AddScoped<IValidator<RegisterTeacherOrAdminDto>, RegisterTeacherOrAdminDtoValidator>();


builder.Services.AddScoped<IReportTopicService, ReportTopicService>();
builder.Services.AddScoped<IStudentReportService, StudentReportService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISubjectGroupService, SubjectGroupService>();
builder.Services.AddScoped<ISubjectSubgroupService, SubjectSubgroupService>();


var app = builder.Build();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
seeder.Seed();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}

app.UseAuthentication();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();