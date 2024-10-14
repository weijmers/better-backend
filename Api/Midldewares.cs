namespace Api;

public static class Middlewares
{
    public static async Task HandleExceptions(HttpContext context, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error.");
        }
    }
}