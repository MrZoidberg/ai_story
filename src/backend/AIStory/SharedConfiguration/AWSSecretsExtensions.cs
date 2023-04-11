namespace AIStory.TelegramBotLambda;

using Microsoft.Extensions.Configuration;

public static class AWSSecretsExtensions
{
    public static IConfigurationBuilder AddAmazonSecretsManager(this IConfigurationBuilder configurationBuilder,
                    string region,
                    string secretName)
    {
        var configurationSource =
                new AmazonSecretsManagerConfigurationSource(region, secretName);

        configurationBuilder.Add(configurationSource);

        return configurationBuilder;
    }
}
