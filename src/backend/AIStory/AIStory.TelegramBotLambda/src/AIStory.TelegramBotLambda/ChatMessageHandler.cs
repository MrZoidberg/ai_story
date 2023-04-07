namespace AIStory.TelegramBotLambda;

using AIStory.SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

internal class ChatMessageHandler
{
    private readonly ITelegramBotClient telegramBotClient;
    private readonly UserChatRepository userChatRepository;

    public ChatMessageHandler(ITelegramBotClient telegramBotClient, UserChatRepository userChatRepository)
    {
        this.telegramBotClient = telegramBotClient;
        this.userChatRepository = userChatRepository;
    }

    public async Task ProcessMessage(Message message)
    {
        var userId = message.From.Id;
        var chat = await userChatRepository.GetUserChat(userId);
        if (chat == null)
        {
            chat = new UserChat()
            {
                UserId = userId,
                UserName = message.From.Username,
                ChatStepId = 0
            };
            await userChatRepository.AddUserChat(chat);
        }
        else
        {

        }
    }
}
