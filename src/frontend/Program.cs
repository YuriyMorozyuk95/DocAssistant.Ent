// Copyright (c) Microsoft. All rights reserved.

using Azure.Storage.Blobs;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseUrl = builder.Configuration["AppSettings:BACKEND_URI"];
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
{
    var baseUrl = builder.Configuration["AppSettings:BACKEND_URI"];
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IPermissionApiClient, PermissionApiClient>(client =>
{
    var baseUrl = builder.Configuration["AppSettings:BACKEND_URI"];
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration["BlobStorage"]));

builder.Services.AddScoped<OpenAIPromptQueue>();
builder.Services.AddLocalStorageServices();
builder.Services.AddSessionStorageServices();
builder.Services.AddSpeechSynthesisServices();
builder.Services.AddSpeechRecognitionServices();
builder.Services.AddMudServices();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);
});

await JSHost.ImportAsync(
    moduleName: nameof(JavaScriptModule),
    moduleUrl: $"../js/iframe.js?{Guid.NewGuid()}" /* cache bust */);

var host = builder.Build();
await host.RunAsync();
