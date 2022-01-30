namespace VkBot.Storing;

public class InMemoryUserStateStorage : IUserStateStorage
{
    public InMemoryUserStateStorage()
    {
        _userStates = new Dictionary<int, UserState>();
    }

    public Task<UserState> GetUserStateAsync(int userId)
    {
        if (!_userStates.ContainsKey(userId))
        {
            _userStates.Add(userId, UserState.Initial);
        }

        return Task.FromResult(_userStates[userId]);
    }

    public Task SetUserState(int userId, UserState state)
    {
        if (!_userStates.ContainsKey(userId))
        {
            _userStates.Add(userId, state);
        }
        else
        {
            _userStates[userId] = state;
        }

        return Task.CompletedTask;
    }

    private readonly Dictionary<int, UserState> _userStates;
}