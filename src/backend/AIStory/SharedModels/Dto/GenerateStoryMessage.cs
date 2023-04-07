namespace Zoid.AIStory.SharedModels.Dto;

public class GenerateStoryMessage
{
    public string Id { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string StoryTheme { get; set; } = null!;
    public string StoryLocation { get; set; } = null!;
    public IEnumerable<string> StoryCharacters { get; set; } = null!;
    public bool GenerateAudio { get; set; }

    public ushort StoryLength { get; set; }
    public string Model { get; set; } = null!;

    public string UserId { get; set; } = null!;
}
