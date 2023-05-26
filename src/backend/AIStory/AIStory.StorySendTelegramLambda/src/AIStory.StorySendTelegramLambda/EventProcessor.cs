namespace AIStory.StorySendTelegramLambda;

using AIStory.SharedModels.Localization;
using AIStory.SharedModels.Models;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using System.Text;
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
        if (story.StoryText == null)
        {
            story = await storiesRepository.GetStory(story.StoryId);
        }
        lambdaLogger.LogInformation($"Processing story {story.StoryId} for chat {story.ChatId}");

        stringResourceFactory.Language = story.Language;

        if (story.StoryText == null || story.StoryText.Count == 0)
        {
            lambdaLogger.LogError($"Processing story {story.StoryId} failed because story text is absent");

            await telegramBotClient.SendTextMessageAsync(new ChatId(story.ChatId), stringResourceFactory.StringResources.StoryGenerationError);
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(story.Title))
            {
                sb.AppendLine(story.Title);
                sb.AppendLine();
            }
            sb.AppendLine(string.Join(Environment.NewLine, story.StoryText!.Select(x => x.Value)));
            if (story.HashTags != null && story.HashTags.Count() > 0)
            {
                sb.AppendLine(string.Join(' ', story.HashTags.Select(h => h.StartsWith('#') ? h : "#" + h)));
            }

            var text = string.Format(stringResourceFactory.StringResources.HereIsStoryFormat, sb.ToString());
            await telegramBotClient.SendTextMessageAsync(new ChatId(story.ChatId), text);
        }
        story.IsSentToChat = true;
        await storiesRepository.UpdateIsSentToChat(story);
    }
}
