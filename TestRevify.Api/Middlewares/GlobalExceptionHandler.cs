using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TestRevify.Api.Endpoints;

namespace TestRevify.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int status = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = "An error occurred",
            Type = exception.GetType().Name,
            Detail = exception.Message
        };

        //var typeInfoResolver = new DefaultJsonTypeInfoResolver();
        //var tJsonTypeInfo = typeInfoResolver.GetTypeInfo(ProblemDetails, JsonSerializerOptions.Web);


        //httpContext.Response.WriteAsJsonAsync(problemDetails, new ProblemDetailJsonSerializerContext(null), cancellationToken);

        //await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

//[JsonSerializable(typeof(ProblemDetails))]
//internal partial class ProblemDetailJsonSerializerContext : JsonSerializerContext
//{
//    public ProblemDetailJsonSerializerContext(JsonSerializerOptions? options) : base(options)
//    {
//    }

//    protected override JsonSerializerOptions? GeneratedSerializerOptions => throw new NotImplementedException();

//    public override JsonTypeInfo? GetTypeInfo(Type type)
//    {
//        throw new NotImplementedException();
//    }
//}