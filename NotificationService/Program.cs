using NotificationService;
using NotificationService.Services.Extension;
using NotificationService.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFluentEmail(builder.Configuration);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient();

// El Worker debe estar registrado como HostedService
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
