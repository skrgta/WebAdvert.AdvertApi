using AdvertApi.HealthChecks;
using AdvertApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddTransient<IAdvertStorageService, DynamoDBAdvertStorage>();
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("Storage", null, null, new TimeSpan(0, 1, 0));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();
