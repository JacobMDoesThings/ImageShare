using ImageShare.WebApi.Configuration;
using ImageShare.WebApi.Services;
using ImageShare.WebApi.Services.Testing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddAzureStorageConfiguration(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSingleton<IUserContextProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    
    // Default to mock in development environments
    bool useMock = config.GetValue("UseMockUserContext", builder.Environment.IsDevelopment());
    
    if (useMock)
    {
        // Get mock user ID from configuration with fallback
        string mockUserId = config["MockUserContext:UserId"] ?? "default-mock-user";
        return new MockUserContextProvider(mockUserId);
    }
    
    // TODO: Implement real Entra provider when ready
    return new EntraUserContextProvider(); 
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting(); // Required for routing controllers
app.MapControllers(); // Maps controller endpoints
app.Run();