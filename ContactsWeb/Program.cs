using ContactsWeb.Models;
using ContactsWeb.Repositories;
using ContactsWeb.Services;
using Microsoft.Data.SqlClient;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logPath = builder.Configuration.GetValue<string>("LogFilePath");
if(string.IsNullOrEmpty(logPath))
    logPath = "logs/contactweb-.txt";
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Log.Error("Database Connection String missing");
    throw new InvalidOperationException("Database connection string is missing.");
}

builder.Services.AddScoped<IContactRepository>(provider =>
    new ContactRepository(connectionString, provider.GetService<ILogger<ContactRepository>>()));

builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContactService, ContactService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Contact}/{action=Index}/{id?}");

try
{
    using var connection = new SqlConnection(connectionString);
    var repository = new ContactRepository(connectionString, app.Services.GetService<ILogger<ContactRepository>>());
    await repository.InitializeDatabaseAsync();
    Log.Information("Database initialized successufully");
}
catch(Exception ex)
{
    Log.Fatal(ex, "Failed to initalize database");
    Environment.Exit(1);
}

Log.Information("ContactsWeb starting up.");

app.Run();

