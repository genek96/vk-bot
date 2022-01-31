using Microsoft.EntityFrameworkCore;
using VkBot.Configuration;
using VkBot.Storing.Models;

namespace VkBot.Storing;

public sealed class UserStateContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public UserStateContext(DatabaseSettings settings)
    {
        _settings = settings;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_settings.ConnectionString);
    }

    private readonly DatabaseSettings _settings;
}