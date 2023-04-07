namespace AIStory.SharedModels.Models;

using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;

public class UserChat
{
    public long UserId { get; set; }

    public string UserName { get; set; } = null!;

    public int ChatStepId { get; set; } = -1;
}

public static class UserChatExtensions
{
    public static Dictionary<string, AttributeValue> ToDynamoDb(this UserChat story)

    { 
        return new Dictionary<string, AttributeValue>
        {
            { "UserId", new AttributeValue { N = story.UserId.ToString() } },
            { "UserName", new AttributeValue { S = story.UserName } },
            { "ChatStepId", new AttributeValue { N = story.ChatStepId.ToString() } },
        };
    }

    public static UserChat ToUserChat(this Dictionary<string, AttributeValue> item)
    {
        return new UserChat
        {
            UserId = long.Parse(item["UserId"].N),
            UserName = item["UserName"].S,
            ChatStepId = int.Parse(item["ChatStepId"].N),
        };
    }
}