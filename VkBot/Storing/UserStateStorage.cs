using Microsoft.EntityFrameworkCore;
using VkBot.Storing.Models;

namespace VkBot.Storing;

public class UserStateStorage : IUserStateStorage
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
            user = new(userId)
            {
                CurrentState = UserState.Initial
            };
            await _userStateContext.Users.AddAsync(user);
            await _userStateContext.SaveChangesAsync();
            return UserState.Initial;
        }

        return user.CurrentState;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task SetUserStateAsync(int userId, UserState state)
    {
        var user = await _userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            user = new(userId);
            await _userStateContext.Users.AddAsync(user);
        }

        user.CurrentState = state;

        await _userStateContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(int userId, Action<User> updateAction)
    {
        var user = await _userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            user = new(userId);
            await _userStateContext.Users.AddAsync(user);
        }

        updateAction(user);

        await _userStateContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _userStateContext.Dispose();
    }

    private readonly UserStateContext _userStateContext;
}