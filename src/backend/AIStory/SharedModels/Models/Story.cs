namespace AIStory.SharedModels.Models;

using Amazon.DynamoDBv2.Model;

public class Story
{
    public string StoryId { get; set; } = null!;
    public IDictionary<int, string> StoryText { get; set; } = null!;
    public TimeSpan ProcessingTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Prompt { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string StoryTheme { get; set; } = null!;
    public string StoryLocation { get; set; } = null!;
    public IEnumerable<string> StoryCharacters { get; set; } = null!;
    public bool GenerateAudio { get; set; }
    public ushort StoryLength { get; set; }
    public string Model { get; set; } = null!;
    public int CompletionTokens { get; set; }
}

public static class StoryExtensions
{
    public static Dictionary<string, AttributeValue> ToDynamoDb(this Story story)
    {
        return new Dictionary<string, AttributeValue>
        {
            { "StoryId", new AttributeValue { S = story.StoryId } },
            { "Story", new AttributeValue { M = story.StoryText.ToDictionary(k => k.Key.ToString(), v => new AttributeValue(v.Value)) } },
            { "ProcessingTime", new AttributeValue { N = story.ProcessingTime.TotalSeconds.ToString() } },
            { "CreatedAt", new AttributeValue { S = story.CreatedAt.ToString() } },
            { "Prompt", new AttributeValue { SS = story.Prompt.ToList() } },
            { "UserId", new AttributeValue { S = story.UserId } },
            { "Language", new AttributeValue { S = story.Language } },
            { "StoryTheme", new AttributeValue { S = story.StoryTheme } },
            { "StoryLocation", new AttributeValue { S = story.StoryLocation } },
            { "StoryCharacters", new AttributeValue { SS = story.StoryCharacters.ToList() } },
            { "GenerateAudio", new AttributeValue { BOOL = story.GenerateAudio } },
            { "StoryLength", new AttributeValue { N = story.StoryLength.ToString() } },
            { "Model", new AttributeValue { S = story.Model } },
            { "CompletionTokens", new AttributeValue { N = story.CompletionTokens.ToString() } },
        };
    }
}