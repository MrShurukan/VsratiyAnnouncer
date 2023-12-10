using Blazored.Toast;
using DiscordBotProject;
using Microsoft.EntityFrameworkCore;
using Overlord;
using Serilog;
using WebProject;
using WebProject.Domain;
using WebProject.Misc;
using WebProject.Services;

var builder = WebApplication.CreateBuilder(args);

// builder.Logging.AddProvider(new SuppressExceptionHandlerLoggingProvider(
//     builder.Services.BuildServiceProvider().GetService<ILoggerFactory>()!)
// );

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredToast();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<RemoveInactiveAgentsService>();

var connectionString = Overlord.Settings.GetFromFile().DbConnection;

builder.Services.AddDbContext<ApplicationContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(connectionString);
});

builder.Services.AddScoped<ActionManager>();
builder.Services.AddSingleton(new DiscordBot());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Overlord.Settings>());
builder.Services.AddExceptionHandler<ExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseExceptionHandler(_ => { });

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseSwagger();
app.UseSwaggerUI();

if (app.Configuration.GetValue<bool>("LaunchDiscordBot", true))
{
    #pragma warning disable CS4014
    app.Services.GetService<DiscordBot>()!.StartAsync();
    #pragma warning restore CS4014
}
else
{
    ConsoleWriter.WriteWarningLn("Внимание! Дискорд бот не будет запущен, так как стоит LaunchDiscordBot = false в appsettings.json");    
}

ConsoleWriter.WriteInfoLn("Проверяю зависимости...");
DependencyHelper.TestDependencies();

ConsoleWriter.WriteInfoLn("http://localhost:5064/");

app.Run();