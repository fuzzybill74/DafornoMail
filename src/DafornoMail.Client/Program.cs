using DafornoMail.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace DafornoMail.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        // Set the root components
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Configure HTTP client with base address
        var baseAddress = builder.HostEnvironment.BaseAddress;
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

        // Register services
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
        });

        // Add auth services
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Auth0", options.ProviderOptions);
            options.ProviderOptions.ResponseType = "code";
            options.ProviderOptions.DefaultScopes.Add("email");
            options.ProviderOptions.DefaultScopes.Add("profile");
        });

        // Register application services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IFolderService, FolderService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        
        // Add state management
        builder.Services.AddScoped<AppState>();
        
        // Add http client factory
        builder.Services.AddHttpClient("ServerAPI", 
            client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        // Configure the client to use the API
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient("ServerAPI"));

        await builder.Build().RunAsync();
    }
}
