using ECO.WebApi.Infrastructure;
using ECO.WebApi.Host.Configurations;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddConfigurations();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
await app.Services.InitializeDatabasesAsync();

app.UseInfrastructure();
app.MapEndpoints();
app.Run();
