using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TestRevify.OData;

// Learn more about configuring OData at https://learn.microsoft.com/odata/webapi-8/getting-started
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddOData(opt =>
{
    opt.AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
           .EnableQueryFeatures(100);

    opt.RouteOptions.EnableControllerNameCaseInsensitive = true;
    opt.RouteOptions.EnableActionNameCaseInsensitive = true;
    opt.RouteOptions.EnablePropertyNameCaseInsensitive = true;
    opt.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
});

// Learn more about configuring Swagger/OpenAPI at https://github.com/OData/AspNetCoreOData/tree/main/sample/ODataRoutingSample
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestRevify.OData", Version = "v1" });
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();


app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseODataRouteDebug();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestRevify.OData V1"));

    // OpenAPI
    app.MapOpenApi();
}

app.UseRouting();

app.MapControllers();

app.Run();
