namespace AIStories.ApiGetStories.Models;

using AIStory.SharedModels.Dto;

public class GetStoriesResponse
{
    public StoryShortPage Page { get; set; }

    public GetStoriesResponse(StoryShortPage page)
    {
        Page = page;
    }
}
