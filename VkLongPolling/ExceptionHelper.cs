namespace VkLongPolling;

public static class ExceptionHelper
{
    public static async Task<T> TryDoWithRethrow<T>(Func<Task<T>> func, Action<Exception> onException)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            onException(ex);
            throw;
        }
    }

    public static async Task<T?> TryDoAsync<T>(
        Func<Task<T>> func,
        Action<Exception> onException
    ) where T : class?
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            onException(ex);
            return null;
        }
    }

    public static async Task TryDoAsync(Func<Task> func, Action<Exception> onException)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            onException(ex);
        }
    }
}