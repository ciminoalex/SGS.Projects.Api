using SGS.Projects.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register custom services
builder.Services.AddScoped<IHanaOdbcService, HanaOdbcService>();
builder.Services.AddScoped<ISapB1ServiceLayerService, SapB1ServiceLayerService>();

// Register HttpClient for SAP B1 Service Layer
builder.Services.AddHttpClient<ISapB1ServiceLayerService, SapB1ServiceLayerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
