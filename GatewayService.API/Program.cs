var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseRouting();
// Map controllers 
app.MapControllers();

app.Run();