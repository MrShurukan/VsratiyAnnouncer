using Blazored.Toast;
using DiscordBotProject;
using Overlord.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<ActionManager>();

builder.Services.AddSingleton(new DiscordBot());

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

#pragma warning disable CS4014
app.Services.GetService<DiscordBot>()!.StartAsync();
#pragma warning restore CS4014

app.Run();