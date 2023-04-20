namespace AIStories.ApiGetStories.Models;

using AIStory.SharedModels.Dto;

public class GetStoriesResponse
{
    public StoryShortPage Page { get; set; }

    public string Language { get; set; }

    public GetStoriesResponse(StoryShortPage page, string language)
    {
        Page = page;
        Language = language;
    }
}
