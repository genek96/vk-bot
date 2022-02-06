using VkBot.Storing.Models;

namespace VkBot.Storing;

public interface IUserStateStorage: IDisposable
{
    Task<UserState> GetUserStateAsync(int userId);
    Task<User?> GetUserAsync(int userId);
    Task SetUserStateAsync(int userId, UserState state);
    Task UpdateUserAsync(int userId, Action<User> updateAction);
}