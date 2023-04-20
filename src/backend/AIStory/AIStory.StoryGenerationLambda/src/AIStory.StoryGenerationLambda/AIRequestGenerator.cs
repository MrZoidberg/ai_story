namespace AIStory.StoryGenerationLambda;

using AIStory.SharedModels.Localization;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using Zoid.AIStory.SharedModels.Dto;

internal abstract class AIRequestGenerator: IAIRequestGenerator
{

    public ChatRequest GenerateStoryRequest(GenerateStoryMessage generateStoryMessage, StringResourceFactory stringResourceFactory)
    {
        var tokensCount = GetTokensCount(generateStoryMessage.Model, generateStoryMessage.StoryLength);
        var promptTemplate = GetPromtTemplate(generateStoryMessage.Language, generateStoryMessage.Model, generateStoryMessage.StoryCharacters.Count(), tokensCount);
        if (promptTemplate == null)
        {
            throw new ArgumentException("Language is not supported");
        }

        var participants = GetParticipantsPromptPart(generateStoryMessage.Language, generateStoryMessage.StoryCharacters);
        var storyTheme = stringResourceFactory.StringResources.GetThemeName(generateStoryMessage.StoryTheme).ToLowerInvariant();
        var location = PrepareLocationText(generateStoryMessage.StoryLocation);
        var prompt = string.Format(promptTemplate, storyTheme, generateStoryMessage.StoryCharacters.Count(), participants, location);

        var systemPrompt = GetSystemPrompt(generateStoryMessage.GenerateAudio);

        var storyRequest = new ChatRequest
        {
            Model = generateStoryMessage.Model,
            Messages = new ChatMessage[] { new ChatMessage(ChatMessageRole.System, systemPrompt), new ChatMessage(ChatMessageRole.User, prompt) },
            MaxTokens = tokensCount,
            Temperature = 0.7,
            TopP = 1,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };

        return storyRequest;
    }

    protected abstract string GetSystemPrompt(bool generateAudio);

    protected abstract string GetParticipantsPromptPart(string language, IEnumerable<string> participants);

    protected abstract string PrepareLocationText(string location);

    protected abstract string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens);
  
    protected abstract ushort GetTokensCount(string model, ushort lengthCharacters);   
}
