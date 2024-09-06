using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("gateway.json", optional: false, reloadOnChange: true);
builder.Services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.MapControllers();

app.Run();
