using Deeps;
using Deeps.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<PracticeStore>();
builder.Services.AddSingleton<HotkeyService>();

var host = builder.Build();

// Initialise the store (loads from localStorage) before rendering
var store = host.Services.GetRequiredService<PracticeStore>();
await store.InitAsync();

await host.RunAsync();
