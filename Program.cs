using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using SEBIZ.Data;
using SEBIZ.Domain.Contracts;
using SEBIZ.Extensions;
using SEBIZ.Service;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureCors(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<AppDbContext>();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();
app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/Images"
});

app.UseAuthorization();

app.MapControllers();

app.Run();
