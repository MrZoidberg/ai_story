namespace AIStory.StoryGenerationLambda;
using OpenAI_API.Models;
using System.Collections.Generic;

internal sealed class RuAIRequestGenerator: AIRequestGenerator
{   

    protected override string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens, bool generateAudio)
    {
        string participantsPart = participantsCount == 1 ? "главным персонажем" : "главными персонажами";
        string audioPart = generateAudio ? " Используй Speech Synthesis Markup Language для расстановки интонации в тексте." : string.Empty;
        return $"Напиши {{0}} c {{1}} {participantsPart} {{2}}. История должна происходить в месте {{3}} и должна содержать не менее {GetMinLengthFromTokens(model, lengthTokens)} и не более {GetMaxLengthFromTokens(model, lengthTokens)} слов. Обязательно закончи рассказ.{audioPart}";
    }

    protected override string GetParticipantsPromptPart(string language, IEnumerable<string> participants)
    {
        return string.Join(" и ", participants);
    }

    protected override ushort GetTokensCount(string model, ushort lengthCharacters)
    {
        return (ushort)(lengthCharacters * 5);
    }

    private ushort GetMinLengthFromTokens(string model, ushort lengthTokens)
    {
        if (model == Model.ChatGPTTurbo)
        {
            return (ushort)(lengthTokens / 5);
        }
        else
        {
            return (ushort)(lengthTokens / 5);
        }
    }

    private ushort GetMaxLengthFromTokens(string model, ushort lengthTokens)
    {
        if (model == Model.ChatGPTTurbo)
        {
            return (ushort)(lengthTokens / 4);
        }
        else
        {
            return (ushort)(lengthTokens / 4);
        }
    }
}
