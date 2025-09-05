using SGS.Projects.Api.Services;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddScoped<IDbOdbcService, DbOdbcService>();
builder.Services.AddScoped<ISapB1ServiceLayerService, SapB1ServiceLayerService>();

// Register HttpClient for SAP B1 Service Layer with SSL bypass
builder.Services.AddHttpClient<ISapB1ServiceLayerService, SapB1ServiceLayerService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                // Ignora tutti i problemi di certificati SSL
                Console.WriteLine($"SSL Certificate validation bypassed. Policy errors: {sslPolicyErrors}");
                return true;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
