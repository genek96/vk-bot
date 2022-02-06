using System.ComponentModel.DataAnnotations;

namespace VkBot.Storing.Models;

public class User
{
    public User(int userId)
    {
        UserId = userId;
    }

    [Key]
    public int UserId { get; set; }

    public UserState CurrentState { get; set; }
    public int WorkDuration { get; set; }
    public int Coins { get; set; }
    public int Energy { get; set; }
    public DateTime LastActivityTime { get; set; }
}