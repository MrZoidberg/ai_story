namespace AIStory.TelegramBotLambda;
public class LambdaOptions
{
    public string TelegramBotToken { get; set; }

    public string? DynamoDbChatsTable { get; set; }

    public string SqsStoryGenerationUrl { get; set; }
}
