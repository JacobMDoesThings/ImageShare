


using ImageShare.Shared.Services.SasService;
using ImageShare.Shared.Services.SasService.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddAzureStorageConfiguration(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSingleton<ISasGeneratorService, SasGeneratorService>();
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