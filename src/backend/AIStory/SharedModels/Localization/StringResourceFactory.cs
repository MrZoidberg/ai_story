namespace AIStory.SharedModels.Localization;

using AIStory.SharedModels.Models;

public sealed class StringResourceFactory
{
    private string language;

    public string Language
    {
        get => language;
        set
        {
            language = value;
            switch (language)
            {
                case "ru":
                    StringResources = new RuStringResources();
                    break;
                case "uk":
                    StringResources = new UaStringResources();
                    break;
                default:
                    StringResources = new EngStringResources();
                    break;
            }
        }
    }

    public IStringResources StringResources { get; private set; }

    public string[] Separators
    {
        get
        {
            switch (language)
            {
                case "ru":
                    return new[] { ",", " и " };
                case "uk":
                    return new[] { ",", " и ", " і ", " та " };
                case "en":
                    return new[] { ",", " and ", " & " };
                default:
                    throw new NotSupportedException("This language is not supported: " + language);
            }
        }
    }

    public StringResourceFactory()
    {
        Language = "en";
    }
}

public interface IStringResources
{
    string HelloMessageFormat { get; set; }

    string ChooseLanguage { get; set; }

    string InvalidLanguage { get; set; }

    string ChooseStoryTheme { get; set; }

    string GetThemeName(StoryTheme theme);

    public string InvalidStoryTheme { get; set; }

    public string SelectedStoryThemeFormat { get; set; }

    public string ChooseStoryCharacters { get; set; }

    public string InvalidStoryCharacters { get; set; }

    public string ChooseStoryLocation { get; set; }

    public string InvalidStoryLocation { get; set; }

    public string StoryBeingGeneratedFormat { get; set; }

    public string HereIsStoryFormat { get; set; }

    public string StoryGenerationError { get; set; }
}

internal class RuStringResources : IStringResources
{
    public string HelloMessageFormat { get; set; } = "Привет {0}! Я бот, который поможет тебе написать историю. Напиши мне /start, чтобы начать.";

    public string ChooseLanguage { get; set; } = "Выберите язык";

    public string InvalidLanguage { get; set; } = "Неверный язык";

    public string ChooseStoryTheme { get; set; } = "Выберите тему истории";

    public string GetThemeName(StoryTheme theme)
    {
        return theme switch
        {
            StoryTheme.Horror => "Хоррор",
            StoryTheme.HorrorHappyEnding => "Хоррор со счастливым концом",
            StoryTheme.HorrorAdventure => "Хоррор-приключение",
            StoryTheme.HorrorLove => "Любовный хоррор",
            StoryTheme.HorrorHumor => "Смешной хоррор",
            StoryTheme.HorrorMystery => "Мистический хоррор",
            StoryTheme.Fantasy => "Фэнтези история",
            StoryTheme.FantasyHorror => "Фэнтези-хоррор",
            StoryTheme.FantasyAdventure => "Приключенческий хоррор",
            StoryTheme.Historical => "Историческая история",
            StoryTheme.ScienceFiction => "Научная фантастика",
            StoryTheme.Detective => "Детектив",
            StoryTheme.Adventure => "Приключенческий рассказ",
            StoryTheme.Love => "Любовная история",
            StoryTheme.Humor => "Юмор",
            StoryTheme.Hero => "Героическая история",
            StoryTheme.Fairytale => "Сказка",
            StoryTheme.HistoricalAdventure => "Исторический приключенческая история",
            StoryTheme.HistoricalLove => "Историческая любовная история",
            StoryTheme.Mystery => "Мистика",
            StoryTheme.Thriller => "Триллер",
            _ => throw new NotImplementedException(),
        };
    }

    public string InvalidStoryTheme { get; set; } = "Неверная тема истории";

    public string SelectedStoryThemeFormat { get; set; } = "Выбрана тема {0}";

    public string ChooseStoryCharacters { get; set; } = "Выберите персонажей истории. Пример: Коля и Петя";

    public string InvalidStoryCharacters { get; set; } = "Не могу определить персонажей из вашего ответа";

    public string ChooseStoryLocation { get; set; } = "Выберите место действия";

    public string InvalidStoryLocation { get; set; } = "Не могу опеределить место действия из вашего ответа";

    public string StoryBeingGeneratedFormat { get; set; } = "Я добавил вашу историю к себе в очередь под номером {0}. История будет готова примерно через {1} секунд. Мой искусственный разум уже думает над ней...";

    public string HereIsStoryFormat { get; set; } = "Вот ваша история:\n\n{0}\n\nСделано с помощью https://t.me/AIGPTStoriesBot\nhttps://robostoryz.com";

    public string StoryGenerationError { get; set; } = "Произошла ошибка при генерации истории. Попробуйте еще раз.";
}

internal class UaStringResources : IStringResources
{
    public string HelloMessageFormat { get; set; } = "Привіт {0}! Я бот, який допоможе тобі написати історію. Напиши мені /start, щоб почати.";

    public string ChooseLanguage { get; set; } = "Виберіть мову";

    public string InvalidLanguage { get; set; } = "Невірна мова";

    public string ChooseStoryTheme { get; set; } = "Виберіть тему історії";

    public string GetThemeName(StoryTheme theme)
    {
        return theme switch
        {
            StoryTheme.Horror => "Хоррор",
            StoryTheme.HorrorHappyEnding => "Хоррор з щасливим кінцем",
            StoryTheme.HorrorAdventure => "Хоррор-пригоди",
            StoryTheme.HorrorLove => "Любовний хоррор",
            StoryTheme.HorrorHumor => "Смішний хоррор",
            StoryTheme.HorrorMystery => "Містичний хоррор",
            StoryTheme.Fantasy => "Фентезі історія",
            StoryTheme.FantasyHorror => "Фентезі-хоррор",
            StoryTheme.FantasyAdventure => "Пригодницький хоррор",
            StoryTheme.Historical => "Історична історія",
            StoryTheme.ScienceFiction => "Наукова фантастика",
            StoryTheme.Detective => "Детектив",
            StoryTheme.Adventure => "Пригодницька розповідь",
            StoryTheme.Love => "Любовна історія",
            StoryTheme.Humor => "Гумор",
            StoryTheme.Hero => "Героїчна історія",
            StoryTheme.Fairytale => "Казка",
            StoryTheme.HistoricalAdventure => "Історична пригодницька історія",
            StoryTheme.HistoricalLove => "Історична любовна історія",
            StoryTheme.Mystery => "Містика",
            StoryTheme.Thriller => "Триллер",
            _ => throw new NotImplementedException(),
        };
    }

    public string InvalidStoryTheme { get; set; } = "Невірна тема історії";

    public string SelectedStoryThemeFormat { get; set; } = "Вибрано тему {0}";

    public string ChooseStoryCharacters { get; set; } = "Виберіть персонажів історії. Приклад: Коля і Петя";

    public string InvalidStoryCharacters { get; set; } = "Не можу визначити персонажів із вашої відповіді";

    public string ChooseStoryLocation { get; set; } = "Виберіть місце дії";

    public string InvalidStoryLocation { get; set; } = "Не можу визначити місце дії з вашої відповіді";

    public string StoryBeingGeneratedFormat { get; set; } = "Я додав історію до себе в чергу під номером {0}. Історія буде готова приблизно через {1} секунд. Мій штучний розум уже думає над нею...";

    public string HereIsStoryFormat { get; set; } = "Ось ваша історія:\n\n{0}\n\nЗроблено за допомогою https://t.me/AIGPTStoriesBot\nhttps://robostoryz.com";

    public string StoryGenerationError { get; set; } = "Сталася помилка при генерації історії. Спробуйте ще раз.";
}

internal class EngStringResources : IStringResources
{
    public string HelloMessageFormat { get; set; } = "Hello {0}! I'm a bot that will help you write a story. Write me /start to start.";

    public string ChooseLanguage { get; set; } = "Choose language";

    public string InvalidLanguage { get; set; } = "Wrong language";

    public string ChooseStoryTheme { get; set; } = "Choose story theme";

    public string GetThemeName(StoryTheme theme)
    {
        return theme switch
        {
            StoryTheme.Horror => "Horror",
            StoryTheme.HorrorHappyEnding => "Horror with Happy Ending",
            StoryTheme.HorrorAdventure => "Horror-Adventure",
            StoryTheme.HorrorLove => "Love Horror",
            StoryTheme.HorrorHumor => "Funny Horror",
            StoryTheme.HorrorMystery => "Mystery Horror",
            StoryTheme.Fantasy => "Fantasy Story",
            StoryTheme.FantasyHorror => "Fantasy-Horror",
            StoryTheme.FantasyAdventure => "Fantasy Adventure",
            StoryTheme.Historical => "Historical Story",
            StoryTheme.ScienceFiction => "Science Fiction",
            StoryTheme.Detective => "Detective",
            StoryTheme.Adventure => "Adventure story",
            StoryTheme.Love => "Love Story",
            StoryTheme.Humor => "Humor",
            StoryTheme.Hero => "Heroic Story",
            StoryTheme.Fairytale => "Fairytale",
            StoryTheme.HistoricalAdventure => "Historical Adventure",
            StoryTheme.HistoricalLove => "Historical Love Story",
            StoryTheme.Mystery => "Mystery",
            StoryTheme.Thriller => "Thriller",
            _ => throw new NotImplementedException(),
        };
    }

    public string InvalidStoryTheme { get; set; } = "Wrong story theme";

    public string SelectedStoryThemeFormat { get; set; } = "Selected theme {0}";

    public string ChooseStoryCharacters { get; set; } = "Choose story characters. Example: John and Kate";

    public string InvalidStoryCharacters { get; set; } = "Can't identify the characters from your answer";

    public string ChooseStoryLocation { get; set; } = "Choose a location";

    public string InvalidStoryLocation { get; set; } = "I can't determine the location of the action from your answer";

    public string StoryBeingGeneratedFormat { get; set; } = "I added your story to my queue under number {0}. The story will be ready in about {1} seconds. My AI mind is already thinking about it...";

    public string HereIsStoryFormat { get; set; } = "Here is your story:\n\n{0}\n\nMade with https://t.me/AIGPTStoriesBot.\nhttps://robostoryz.com";

    public string StoryGenerationError { get; set; } = "An error occurred while generating the story. Try again.";
}