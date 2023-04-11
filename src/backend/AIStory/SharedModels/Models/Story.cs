namespace AIStory.SharedModels.Models;

using Amazon.DynamoDBv2.Model;
using System.Globalization;

public class Story
{
    public string StoryId { get; set; } = null!;
    public IDictionary<int, string>? StoryText { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Prompt { get; set; } = null!;
    public string ChatId { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string StoryTheme { get; set; } = null!;
    public string StoryLocation { get; set; } = null!;
    public IEnumerable<string> StoryCharacters { get; set; } = null!;
    public bool GenerateAudio { get; set; }
    public ushort StoryLength { get; set; }
    public string Model { get; set; } = null!;
    public int CompletionTokens { get; set; }
    public bool IsSentToChat { get; set; }
    public string? Error { get; set; }
}

public static class StoryExtensions
{
    public static Dictionary<string, AttributeValue> ToDynamoDb(this Story story)
    {
        var dict = new Dictionary<string, AttributeValue>
        {
            { "StoryId", new AttributeValue { S = story.StoryId } },
            { "ProcessingTime", new AttributeValue { N = story.ProcessingTime.TotalSeconds.ToString() } },
            { "CreatedAt", new AttributeValue { S = story.CreatedAt.ToString(CultureInfo.InvariantCulture) } },
            { "Prompt", new AttributeValue { SS = story.Prompt.ToList() } },
            { "ChatId", new AttributeValue { S = story.ChatId } },
            { "Language", new AttributeValue { S = story.Language } },
            { "StoryTheme", new AttributeValue { S = story.StoryTheme } },
            { "StoryLocation", new AttributeValue { S = story.StoryLocation } },
            { "StoryCharacters", new AttributeValue { SS = story.StoryCharacters.ToList() } },
            { "GenerateAudio", new AttributeValue { BOOL = story.GenerateAudio } },
            { "StoryLength", new AttributeValue { N = story.StoryLength.ToString() } },
            { "Model", new AttributeValue { S = story.Model } },
            { "CompletionTokens", new AttributeValue { N = story.CompletionTokens.ToString() } },
            { "IsSentToChat", new AttributeValue { BOOL = story.IsSentToChat } },
        };

        if (story.StoryText != null)
        {
            dict.Add("Story", new AttributeValue { M = story.StoryText.ToDictionary(k => k.Key.ToString(), v => new AttributeValue(v.Value)) });
        }

        if (!string.IsNullOrEmpty(story.Error))
        {
            dict.Add("Error", new AttributeValue { S = story.Error });
        }

        return dict;
    }

    public static Story ToStory(this Dictionary<string, AttributeValue> item)
    {
        return new Story
        {
            StoryId = item["StoryId"].S,
            StoryText = item.ContainsKey("Story") ? item["Story"].M.ToDictionary(k => int.Parse(k.Key), v => v.Value.S) : null,
            ProcessingTime = TimeSpan.FromSeconds(double.Parse(item["ProcessingTime"].N)),
            CreatedAt = DateTime.Parse(item["CreatedAt"].S, CultureInfo.InvariantCulture),
            Prompt = item["Prompt"].SS,
            ChatId = item["ChatId"].S,
            Language = item["Language"].S,
            StoryTheme = item["StoryTheme"].S,
            StoryLocation = item["StoryLocation"].S,
            StoryCharacters = item["StoryCharacters"].SS,
            GenerateAudio = item["GenerateAudio"].BOOL,
            StoryLength = ushort.Parse(item["StoryLength"].N),
            Model = item["Model"].S,
            CompletionTokens = int.Parse(item["CompletionTokens"].N),
            IsSentToChat = item.ContainsKey("IsSentToChat") ? item["IsSentToChat"].BOOL : false,
            Error = item.ContainsKey("Error") ? item["Error"].S : null
        };
    }
}