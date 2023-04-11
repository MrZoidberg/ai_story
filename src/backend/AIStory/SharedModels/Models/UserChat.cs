namespace AIStory.SharedModels.Models;

using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;

public class UserChat
{
    public string ChatId { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public int ChatStepId { get; set; } = -1;

    public string? Language { get; set; }

    public string FirstName { get; set; } = null!;

    public StoryTheme? StoryTheme { get; set; }

    public IEnumerable<string>? Characters { get; set; }

    public string? StoryLocation { get; set; }
}

public static class UserChatExtensions
{
    public static Dictionary<string, AttributeValue> ToDynamoDb(this UserChat item)

    { 
        var dict =  new Dictionary<string, AttributeValue>
        {
            { "ChatId", new AttributeValue { S = item.ChatId } },
            { "UserId", new AttributeValue { S = item.UserId } },
            { "ChatStepId", new AttributeValue { N = item.ChatStepId.ToString() } },
            { "FirstName", new AttributeValue { S = item.FirstName } },
        };

        if (item.Language != null)
        {
            dict.Add("Lang", new AttributeValue { S = item.Language });
        }

        if (item.UserName != null)
        {
            dict.Add("UserName", new AttributeValue { S = item.UserName });
        }

        if (item.StoryTheme != null)
        {
            dict.Add("StoryTheme", new AttributeValue { N = ((int)item.StoryTheme).ToString() });
        }

        if (item.Characters != null)
        {
            dict.Add("Characters", new AttributeValue { SS = item.Characters.ToList() });
        }

        if (item.StoryLocation != null)
        {
            dict.Add("StoryLocation", new AttributeValue { S = item.StoryLocation });
        }

        return dict;
    }

    public static UserChat ToUserChat(this Dictionary<string, AttributeValue> item)
    {
        return new UserChat
        {
            ChatId = item["ChatId"].S,
            UserId = item["UserId"].S,
            UserName = item.ContainsKey("UserName") ? item["UserName"].S : null,
            FirstName = item["FirstName"].S,
            ChatStepId = int.Parse(item["ChatStepId"].N),
            Language = item.ContainsKey("Lang") ? item["Lang"].S : null, 
            StoryTheme = item.ContainsKey("StoryTheme") ? (StoryTheme)int.Parse(item["StoryTheme"].N) : null,
            Characters = item.ContainsKey("Characters") ? item["Characters"].SS : null,
            StoryLocation = item.ContainsKey("StoryLocation") ? item["StoryLocation"].S : null,
        };
    }
}
