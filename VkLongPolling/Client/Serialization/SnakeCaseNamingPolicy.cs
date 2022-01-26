using System.Text.Json;

namespace VkLongPolling.Client.Serialization;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        var result = string.Empty;
        var isFirst = true;
        foreach (var letter in name)
        {
            if (isFirst)
            {
                isFirst = false;
                result += letter.ToString().ToLower();
                continue;
            }

            if (letter is >= 'A' and <= 'Z' && !isFirst)
                result += "_" + letter.ToString().ToLower();
            else
                result += letter.ToString();
        }

        return result;
    }
}