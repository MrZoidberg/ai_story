namespace AIStory.StoryGenerationLambda.Tests;

using Zoid.AIStory.SharedModels;
using Zoid.AIStory.SharedModels.Dto;

public class AIRequestGeneratorTests
{
    [Fact]
    public void TestPositiveCase_Ru()
    {
        // Arrange
        var message = new GenerateStoryMessage
        {
            GenerateAudio = false,
            Id = Guid.NewGuid().ToString(),
            Language = "ru",
            Model = "gpt-3.5-turbo",
            StoryCharacters = new List<string>(new[] { "Стас", "Филипп" }),
            StoryLength = 200,
            StoryLocation = "в логове врага",
            StoryTheme = "героический рассказ",
            UserId = Guid.NewGuid().ToString()
        };

        // Act
        AIRequestGeneratorFactory aIRequestGeneratorFactory = new AIRequestGeneratorFactory();
        var aiRequestGenerator = aIRequestGeneratorFactory.Create(message.Language);
        var chatRequest = aiRequestGenerator.GenerateStoryRequest(message);

        // Assert
        Assert.NotNull(chatRequest);
        Assert.Equal(message.Model, chatRequest.Model);
        Assert.Equal(message.StoryLength, chatRequest.MaxTokens / 5);
        Assert.Equal(1, chatRequest.Messages.Count);
        Assert.Equal("Напиши героический рассказ и 2 главными персонажами Стас и Филипп. История должна происходить в логове врага и должна содержать не менее 200 и не более 250 слов. Обязательно закончи рассказ.", chatRequest.Messages[0].Content);
    }
}