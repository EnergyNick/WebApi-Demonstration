using System.Net;
using Serilog;

namespace UsersManager.Service.HostUtilities;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            Log.Error(e, "Global exception thrown in {Delegate}", _next.Method.Name);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync($"Internal exception {nameof(e)} was thrown");
        }
    }
}