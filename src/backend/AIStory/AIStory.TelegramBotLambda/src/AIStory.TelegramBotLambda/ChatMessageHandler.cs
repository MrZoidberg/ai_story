namespace AIStory.TelegramBotLambda;

using AIStory.SharedModels.Models;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

internal class ChatMessageHandler
{
    private readonly ITelegramBotClient telegramBotClient;
    private readonly UserChatRepository userChatRepository;
    private readonly Workflow.Workflow workflow;

    public ChatMessageHandler(ITelegramBotClient telegramBotClient, UserChatRepository userChatRepository, Workflow.Workflow workflow)
    {
        this.telegramBotClient = telegramBotClient;
        this.userChatRepository = userChatRepository;
        this.workflow = workflow;
    }

    public async Task ProcessMessage(Update update)
    {
        UserChat? chat;
        switch (update.Type)
        {
            case Telegram.Bot.Types.Enums.UpdateType.Message:
                {
                    var chatId = update.Message?.Chat.Id.ToString();
                    if (chatId == null)
                    { 
                        throw new ArgumentNullException(nameof(chatId));
                    }

                    chat = await userChatRepository.GetUserChat(update.Message!.Chat.Id.ToString());                    
                    if (chat == null)
                    {
                        if (update.Message!.From == null)
                        {
                            throw new InvalidOperationException("Message.From is null");
                        }

                        chat = new UserChat()
                        {
                            UserId = update.Message.From.Id.ToString(),
                            ChatId = chatId,
                            Language = update.Message.From.LanguageCode ?? "en",
                            UserName = update.Message.From.Username,
                            FirstName = update.Message.From.FirstName,
                            ChatStepId = -1
                        };
                        await userChatRepository.AddUserChat(chat);
                    }
                    break;
                }
            case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                {
                    var chatId = update.CallbackQuery?.Message?.Chat.Id.ToString();
                    if (chatId == null)
                    {
                        throw new ArgumentNullException(nameof(chatId));
                    }
                    chat = await userChatRepository.GetUserChat(chatId);
                    break;
                }
            default:
                throw new InvalidOperationException($"Update type {update.Type} is not supported");
        }                        

        await workflow.ProcessWorkflow(chat!, update);
    }
}
