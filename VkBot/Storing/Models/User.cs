namespace VkBot.Storing.Models;

public class User
{
    public User(int userId, UserState currentState)
    {
        UserId = userId;
        CurrentState = currentState;
    }

    public int UserId { get; set; }
    public UserState CurrentState { get; set; }
}