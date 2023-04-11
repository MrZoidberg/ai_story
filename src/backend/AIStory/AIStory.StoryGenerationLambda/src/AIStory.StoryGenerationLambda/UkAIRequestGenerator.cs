namespace AIStory.StoryGenerationLambda;
using OpenAI_API.Models;
using System.Collections.Generic;

internal sealed class UkAIRequestGenerator : AIRequestGenerator
{

    protected override string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens, bool generateAudio)
    {
        string participantsPart = participantsCount == 1 ? "головним персонажем" : "головними персонажами";
        string audioPart = generateAudio ? " Використовуйте Speech Synthesis Markup Language для розміщення інтонації в тексті." : string.Empty;
        return $"Напиши {{0}} з {{1}} {participantsPart} {{2}}. Історія має відбуватися у місці {{3}} і має містити не менше {GetMinLengthFromTokens(model, lengthTokens)} і не більше {GetMaxLengthFromTokens(model, lengthTokens)} слів. Обов'язково закінчи розповідь.{audioPart}";
    }

    protected override string GetParticipantsPromptPart(string language, IEnumerable<string> participants)
    {
        return string.Join(" та ", participants);
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