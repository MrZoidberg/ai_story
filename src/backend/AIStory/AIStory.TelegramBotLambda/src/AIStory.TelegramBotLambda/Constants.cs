namespace AIStory.TelegramBotLambda;

public static class Constants
{
    public static string[] SupportedLanguages = new string[] { "en", "ru", "uk" };

    public const string DefaultModel = "gpt-3.5-turbo";

    public const ushort DefaultStoryLength = 200;
}