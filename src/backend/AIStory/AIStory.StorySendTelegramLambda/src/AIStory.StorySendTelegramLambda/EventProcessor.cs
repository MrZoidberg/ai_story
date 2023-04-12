namespace AIStory.StorySendTelegramLambda;

using AIStory.SharedModels.Localization;
using AIStory.SharedModels.Models;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

internal sealed class EventProcessor
{
    private readonly StringResourceFactory stringResourceFactory;
    private readonly StoriesRepository storiesRepository;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ILambdaLogger lambdaLogger;

    public EventProcessor(StringResourceFactory stringResourceFactory, StoriesRepository storiesRepository, ITelegramBotClient telegramBotClient, ILambdaLogger lambdaLogger)
    {
        this.stringResourceFactory = stringResourceFactory;
        this.storiesRepository = storiesRepository;
        this.telegramBotClient = telegramBotClient;
        this.lambdaLogger = lambdaLogger;
    }


    public async Task ProcessEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {

        var story = record.Dynamodb.NewImage.ToStory();
        lambdaLogger.LogInformation($"Processing story {story.StoryId} for chat {story.ChatId}");

        stringResourceFactory.Language = story.Language;

        var text = string.Format(stringResourceFactory.StringResources.HereIsStoryFormat, string.Join(Environment.NewLine, story.StoryText.Select(x => x.Value)));
        await telegramBotClient.SendTextMessageAsync(new ChatId(story.ChatId), text);

        story.IsSentToChat = true;
        await storiesRepository.UpdateIsSentToChat(story);
    }
}
