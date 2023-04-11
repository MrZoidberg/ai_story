namespace AIStory.StoryGenerationLambda;

using AIStory.SharedModels.Localization;
using OpenAI_API.Chat;
using Zoid.AIStory.SharedModels.Dto;

public interface IAIRequestGenerator
{
    ChatRequest GenerateStoryRequest(GenerateStoryMessage generateStoryMessage, StringResourceFactory stringResourceFactory);
}
