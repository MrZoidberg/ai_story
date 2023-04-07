namespace AIStory.StoryGenerationLambda;
using System;

public class AIRequestGeneratorFactory
{
    public IAIRequestGenerator Create(string language)
    {
        switch (language)
        {
            case "ru":
                return new RuAIRequestGenerator();
            default:
                throw new ArgumentException("Language is not supported");
        }
    }
}
