namespace Zoid.AIStory.SharedModels.Dto;

using global::AIStory.SharedModels.Models;

public class GenerateStoryMessage
{
    public string Id { get; set; } = null!;
    public string Language { get; set; } = null!;
    public StoryTheme StoryTheme { get; set; }
    public string StoryLocation { get; set; } = null!;
    public IEnumerable<string> StoryCharacters { get; set; } = null!;
    public bool GenerateAudio { get; set; }

    public ushort StoryLength { get; set; }
    public string Model { get; set; } = null!;

    public string ChatId { get; set; } = null!;
}
