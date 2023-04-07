namespace AIStory.StoryGenerationLambda;

using OpenAI_API.Chat;
using Zoid.AIStory.SharedModels;
using Zoid.AIStory.SharedModels.Dto;

public interface IAIRequestGenerator
{
    ChatRequest GenerateStoryRequest(GenerateStoryMessage generateStoryMessage);
}
