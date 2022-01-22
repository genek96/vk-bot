namespace VkLongPolling.Exceptions;

public class NetworkException: Exception
{
    public NetworkException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}