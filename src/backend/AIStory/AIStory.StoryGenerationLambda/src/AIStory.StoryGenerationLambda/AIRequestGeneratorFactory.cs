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
            case "uk":
                return new UkAIRequestGenerator();
            case "en":
                return new EngAIRequestGenerator();
            default:
                throw new ArgumentException("Language is not supported");
        }
    }
}
