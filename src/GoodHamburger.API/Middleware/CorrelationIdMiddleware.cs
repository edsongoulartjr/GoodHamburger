using Serilog.Context;

namespace GoodHamburger.API.Middleware;

/// <summary>
/// Garante que cada requisição tenha um CorrelationId único,
/// propagado nos logs e no header de resposta X-Correlation-Id.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N")[..12];

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", $"[{correlationId}]"))
        {
            await next(context);
        }
    }
}
