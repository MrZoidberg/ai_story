namespace AIStory.TelegramBotLambda.Workflow;

using AIStory.SharedModels.Localization;
using AIStory.SharedModels.Models;
using AIStory.TelegramBotLambda.Helpers;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

internal class Workflow
{
    private readonly UserChatRepository userChatRepository;
    private readonly StringResourceFactory stringResourceFactory;
    private readonly ILambdaLogger logger;
    private readonly Dictionary<WorkflowStep, Func<UserChat, Update, Task<WorkflowStep>>> steps = new Dictionary<WorkflowStep, Func<UserChat, Update, Task<WorkflowStep>>>();

    public Workflow(UserChatRepository userChatRepository, ITelegramBotClient telegramBotClient, StringResourceFactory stringResourceFactory, StoryBuilder storyBuilder, ILambdaLogger logger)
    {
        this.userChatRepository = userChatRepository;
        this.stringResourceFactory = stringResourceFactory;
        this.logger = logger;
        steps.Add(WorkflowStep.Initial, async (chat, update) =>
        {
            if (update.Type != UpdateType.Message)
            {
                return WorkflowStep.Initial;
            }
            
            if (update.Message!.From?.IsBot ?? true)
            {
                return WorkflowStep.Initial;
            }

            await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), string.Format(stringResourceFactory.StringResources.HelloMessageFormat, chat.FirstName));
            InlineKeyboardMarkup inlineKeyboard = new(
               new[]
               {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("English", "en"),
                        InlineKeyboardButton.WithCallbackData("Русский", "ru"),
                        InlineKeyboardButton.WithCallbackData("Український", "uk"),
                    },
               });

            await telegramBotClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: stringResourceFactory.StringResources.ChooseLanguage,
                replyMarkup: inlineKeyboard);

            return WorkflowStep.ChooseLanguage;
        });

        steps.Add(WorkflowStep.ChooseLanguage, async (chat, update) =>
        {
            if (update.CallbackQuery != null)
            {
                if (Constants.SupportedLanguages.Contains(update.CallbackQuery.Data))
                {
                    chat.Language = update.CallbackQuery.Data;
                    stringResourceFactory.Language = chat.Language!;
                    await userChatRepository.UpdateChatLanguage(chat);

                    var themesDict = new Dictionary<string, string>(Enum.GetValues<StoryTheme>().Select(x => new KeyValuePair<string, string>(Enum.GetName(x)!, stringResourceFactory.StringResources.GetThemeName(x))));
                    var replyMarkup = InlineKeyboardMarkupHelper.CreateInlineKeyboardMarkup(themesDict, 2);
                    await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), stringResourceFactory.StringResources.ChooseStoryTheme, replyMarkup: replyMarkup);
                    return WorkflowStep.ChooseStoryTheme;
                }
                else
                {
                    logger.LogError($"Not supported language is used in chat {chat.ChatId}: {update.CallbackQuery.Data}");
                }
            }
            else
            {
                logger.LogError($"CallbackQuery is expected in chat {chat.ChatId}. Type: {update.Type}");
            }

            await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), stringResourceFactory.StringResources.InvalidLanguage);

            return WorkflowStep.ChooseLanguage;
        });

        steps.Add(WorkflowStep.ChooseStoryTheme, async (chat, update) =>
        {
            if (update.CallbackQuery != null)
            {
                if (Enum.TryParse<StoryTheme>(update.CallbackQuery.Data, out var theme))
                {
                    chat.StoryTheme = theme;
                    await userChatRepository.UpdateStoryTheme(chat);
                    await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), 
                        string.Format(stringResourceFactory.StringResources.SelectedStoryThemeFormat, stringResourceFactory.StringResources.GetThemeName(theme).ToLowerInvariant()));
                    await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), 
                        stringResourceFactory.StringResources.ChooseStoryCharacters);
                    return WorkflowStep.ChooseCharacters;
                }
                else
                {
                    logger.LogError($"Not supported theme is used in chat {chat.ChatId}: {update.CallbackQuery.Data}");
                }
            }
            else
            {
                logger.LogError($"CallbackQuery is expected in chat {chat.ChatId}. Type: {update.Type}");
            }

            await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), stringResourceFactory.StringResources.InvalidStoryTheme);

            return WorkflowStep.ChooseStoryTheme;
        });

        steps.Add(WorkflowStep.ChooseCharacters, async (chat, update) =>
        {
            if (update.Message != null)
            {
                var characters = StringHelper.SplitWithMultipleSeparators(update.Message.Text!, stringResourceFactory.Separators);
                if (characters.Count() > 0)
                {
                    chat.Characters = characters;
                    await userChatRepository.UpdateCharacters(chat);
                    await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), stringResourceFactory.StringResources.ChooseStoryLocation);
                    return WorkflowStep.ChooseLocation;
                }
            }
            else
            {
                logger.LogError($"Message is expected in chat {chat.ChatId}. Type: {update.Type}");
            }
            await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), stringResourceFactory.StringResources.InvalidStoryCharacters);
            return WorkflowStep.ChooseCharacters;
        });

        steps.Add(WorkflowStep.ChooseLocation, async (chat, update) =>
        {
            if (update.Message != null)
            {
                if (!string.IsNullOrEmpty(update.Message.Text))
                {
                    chat.StoryLocation = update.Message.Text;
                    await userChatRepository.UpdateStoryLocation(chat);
                    var queueLength = await storyBuilder.GetQueueLength();
                    var storyId = await storyBuilder.BuildStory(chat);

                    logger.LogInformation($"Story {storyId} is being generated for chat {chat.ChatId}");

                    await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId),
                        string.Format(stringResourceFactory.StringResources.StoryBeingGeneratedFormat, queueLength, storyBuilder.CalculateStoryEta(queueLength, chat.Language!)));
                    await telegramBotClient.SendChatActionAsync(chatId: chat.ChatId, chatAction: ChatAction.Typing);
                    return WorkflowStep.Initial;
                }
            }
            else
            {
                logger.LogError($"Message is expected in chat {chat.ChatId}. Type: {update.Type}");
            }
            await telegramBotClient.SendTextMessageAsync(new ChatId(chat.ChatId), stringResourceFactory.StringResources.InvalidStoryLocation);
            return WorkflowStep.ChooseLocation;
        });

        /*steps.Add(WorkflowStep.End, async (chat, update) =>
        {
            if (!update.Message?.From.IsBot ?? true)
            {
                return WorkflowStep.Initial;
            }

            return WorkflowStep.End;
        });*/
    }

    public async Task ProcessWorkflow(UserChat chat, Update update)
    {
        WorkflowStep currentStep = Enum.IsDefined(typeof(WorkflowStep), chat.ChatStepId) ? (WorkflowStep)chat.ChatStepId : WorkflowStep.Initial;

        // override any step with /start
        if (update.Message?.Text == "/start")
        {
            currentStep = WorkflowStep.Initial;
        }

        var step = steps[currentStep];
        stringResourceFactory.Language = chat.Language;
        logger.LogInformation($"Executing step {Enum.GetName(currentStep)} for chat {chat.ChatId}");
        var nextStep = await step(chat, update);

        chat.ChatStepId = (int)nextStep;
        if ((int)currentStep != chat.ChatStepId)
        {
            await userChatRepository.UpdateChatStep(chat);
            logger.LogInformation($"Next step for chat {chat.ChatId} is  {Enum.GetName((WorkflowStep)chat.ChatStepId)}");
        }
    }
}

internal enum WorkflowStep
{
    Initial = 0,
    ChooseLanguage = 1,
    ChooseStoryTheme = 2,
    ChooseCharacters = 3,
    ChooseLocation = 4
}