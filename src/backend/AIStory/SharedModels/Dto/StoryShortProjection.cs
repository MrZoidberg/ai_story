namespace AIStory.SharedModels.Dto;

using AIStory.SharedModels.Models;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Globalization;

public class StoryShortPage
{
    public List<StoryShortProjection> Stories { get; set; }

    public string? LastEvaluatedKey { get; set; }

    public bool HasMore { get; set; }

    public int PageSize { get; set; }
}

public class StoryShortProjection
{
    public string StoryId { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Story { get; set; }

    public IEnumerable<string> Prompt { get; set; }

    public string Language { get; set; }
}

public static class StoryShortProjectionExtensions
{
    public static StoryShortProjection ToStoryShortProjection(this Dictionary<string, AttributeValue> item)
    {
        return new StoryShortProjection
        {
            StoryId = item["StoryId"].S,
            CreatedAt = DateTime.Parse(item["CreatedAt"].S, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            Story = string.Join(Environment.NewLine, item["Story"].M.ToDictionary(k => int.Parse(k.Key), v => v.Value.S).Select(x => x.Value)),
            Prompt = item["Prompt"].SS,
            Language = item["Language"].S,
        };
    }
}