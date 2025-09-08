using eventManager.Helper;
using eventManager.Service;
using Microsoft.Extensions.FileProviders;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<OrganiserService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
// Serve default wwwroot files
app.UseStaticFiles();

// Optional: explicitly serve /uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

// Enable CORS for Angular (if frontend runs on a different port)
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod());

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//  }

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();
app.UseStaticFiles();
app.MapGet("/", () => "Event Manager API is running");
app.Run();
void PreWarmDatabaseConnection()
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbUserName = Environment.GetEnvironmentVariable("DB_USERNAME");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbDataBase = Environment.GetEnvironmentVariable("DB_DATABASE");
    // Build the connection string
    var dbConnectionStr = $"Server={dbHost};Database={dbDataBase};Uid={dbUserName};Pwd={dbPassword};";
    connectionString = dbConnectionStr + connectionString;

    using (var connection = new MySqlConnection(connectionString))
    {
        try
        {
            connection.Open();
            using (var command = new MySqlCommand("SELECT 1", connection)) // Dummy query
            {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error pre-warming database connection: " + ex.Message);
        }
    }
}
