using Microsoft.EntityFrameworkCore;
using VkBot.Storing.Models;

namespace VkBot.Storing;

public class UserStateStorage: IUserStateStorage, IDisposable
{
    public UserStateStorage(UserStateContext userStateContext)
    {
        _userStateContext = userStateContext;
    }

    public async Task<UserState> GetUserStateAsync(int userId)
    {
        var user = await _userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            await _userStateContext.Users.AddAsync(new User(userId, UserState.Initial));
            await _userStateContext.SaveChangesAsync();
            return UserState.Initial;
        }

        return user.CurrentState;
    }

    public async Task SetUserState(int userId, UserState state)
    {
        var user = await _userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            await _userStateContext.Users.AddAsync(new User(userId, state));
        }
        else
        {
            user.CurrentState = state;
        }

        await _userStateContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _userStateContext.Dispose();
    }

    private readonly UserStateContext _userStateContext;
}