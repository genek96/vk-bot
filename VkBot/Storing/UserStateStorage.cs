using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Storing.Models;

namespace VkBot.Storing;

public class UserStateStorage : IUserStateStorage
{
    public UserStateStorage(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<UserState> GetUserStateAsync(int userId)
    {
        await using var userStateContext = _serviceProvider.GetService<UserStateContext>()!;
        var user = await userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            user = new(userId)
            {
                CurrentState = UserState.Initial
            };
            await userStateContext.Users.AddAsync(user);
            await userStateContext.SaveChangesAsync();
            return UserState.Initial;
        }

        return user.CurrentState;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        await using var userStateContext = _serviceProvider.GetService<UserStateContext>()!;
        return await userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task SetUserStateAsync(int userId, UserState state)
    {
        await using var userStateContext = _serviceProvider.GetService<UserStateContext>()!;
        var user = await userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            user = new(userId);
            await userStateContext.Users.AddAsync(user);
        }

        user.CurrentState = state;

        await userStateContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(int userId, Action<User> updateAction)
    {
        await using var userStateContext = _serviceProvider.GetService<UserStateContext>()!;
        var user = await userStateContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            user = new(userId);
            await userStateContext.Users.AddAsync(user);
        }

        updateAction(user);

        await userStateContext.SaveChangesAsync();
    }

    private readonly IServiceProvider _serviceProvider;
}