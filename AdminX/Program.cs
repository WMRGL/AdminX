using AdminX.Data;
using AdminX.Meta;
using APIControllers.Data;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("secrets.json", optional: false)
    .Build();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ClinicalContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<AdminContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<LabContext>(options => options.UseSqlServer(config.GetConnectionString("ConStringLab")));
builder.Services.AddDbContext<APIContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<KlaxonContext>(options => options.UseSqlServer(config.GetConnectionString("ConStringEpic")));
builder.Services.AddDbContext<DocumentContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<DQContext>(options => options.UseSqlServer(config.GetConnectionString("DQLab")));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(options =>
   {
       options.LoginPath = "/Login/UserLogin";
   });

builder.Services.AddScoped<IAppointmentDQData, AppointmentDQData>();
builder.Services.AddScoped<IPatientDQData, PatientDQData>();
builder.Services.AddScoped<IGenderIdentityData, GenderIdentityData>();

builder.Services.AddMvc();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secrets.json");

// this code is all for the shared authentication
var directoryInfo = new DirectoryInfo(@"C:\Websites\Authentication");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(directoryInfo)
    .SetApplicationName("GeneticsWebAppHome");

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = ".AspNet.GeneticsWebAppHome";
    options.Cookie.Path = "/";
});
//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Login}/{action=UserLogin}/{id?}");
    pattern: "{controller=Home}/{action=Index}");

app.Run();
