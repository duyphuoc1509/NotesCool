using Microsoft.AspNetCore.Diagnostics;
using NotesCool.Shared.Errors;

namespace NotesCool.Api.Extensions;

public static class ExceptionMiddleware
{
    public static void UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(e => e.Run(async ctx =>
        {
            var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex is ApiException a)
            {
                ctx.Response.StatusCode = a.StatusCode;
                await ctx.Response.WriteAsJsonAsync(new ErrorResponse(a.Code, a.Message));
            }
            else if (ex != null)
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.WriteAsJsonAsync(new ErrorResponse("internal_error", "An unexpected error occurred."));
            }
        }));
    }
}
