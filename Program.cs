using AtlasComex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionStringn = builder.Configuration.GetConnectionString("AtlasComex")
    ?? throw new InvalidOperationException("Connection string 'AtlasComex' is not found.");

builder.Services.AddDbContext<AtlasComexContext>(options =>
    options.UseSqlServer(connectionStringn));

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularLocal", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AngularLocal");

app.UseAuthorization();

app.MapControllers();

app.Run();
