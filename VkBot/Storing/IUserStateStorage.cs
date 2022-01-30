namespace VkBot.Storing;

public interface IUserStateStorage
{
    Task<UserState> GetUserStateAsync(int userId);
    Task SetUserState(int userId, UserState state);
}