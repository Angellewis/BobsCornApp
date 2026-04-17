using BobsCornApp.Application;
using BobsCornApp.Application.Options;
using BobsCornApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<CornRateLimitOptions>(
    builder.Configuration.GetSection(CornRateLimitOptions.SectionName));
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bob's Corn API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
