using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace HW_botFTP
{
    class Program
    {
        private static readonly string BotToken = "7999647843:AAHOaWP7PcDicnXBVB9touLXcVZW7XkKqyw";
        private static readonly TelegramBotClient BotClient = new TelegramBotClient(BotToken);
        private static int currentQuestionIndex = 0;

        private static List<Question> questions = new List<Question>
        {
            new Question("Какой стиль в искусстве пришел на смену классицизму в XIX веке?", 
                new List<string> {"Романтизм", "Барокко", "Импрессионизм", "Модернизм"}, 0, "Правильно! Романтизм пришел на смену классицизму."),
            new Question("Кто написал картину 'Звездная ночь'?",
                new List<string> {"Клод Моне", "Винсент Ван Гог", "Пабло Пикассо", "Леонардо да Винчи"}, 1, "Верно! Автор этой картины — Винсент Ван Гог."),
            new Question("В каком столетии начался период Ренессанса?",
                new List<string> {"XIII век", "XIV век", "XV век", "XVI век"}, 1, "Ренессанс начался в XIV веке, в Италии.")
        };

        static async Task Main(string[] args)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() 
            };

            BotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: CancellationToken.None
            );

            Console.WriteLine("Бот запущен...");
            Console.ReadLine();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var message = update.Message;

                if (message.Text.ToLower() == "/start")
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в викторину по искусству.");
                    await SendQuestion(message.Chat.Id);
                }
                else if (currentQuestionIndex < questions.Count)
                {
                    await CheckAnswer(message.Chat.Id, message.Text);
                }
                else
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Викторина завершена! Спасибо за участие.");
                    currentQuestionIndex = 0;
                }
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Произошла ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task SendQuestion(long chatId)
        {
            var question = questions[currentQuestionIndex];
            var replyKeyboard = new ReplyKeyboardMarkup(GetReplyKeyboard(question.Options))
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await BotClient.SendTextMessageAsync(
                chatId,
                $"Вопрос {currentQuestionIndex + 1}: {question.Text}",
                replyMarkup: replyKeyboard);
        }

        private static async Task CheckAnswer(long chatId, string answer)
        {
            var question = questions[currentQuestionIndex];
            if (question.Options[question.CorrectOptionIndex] == answer)
            {
                await BotClient.SendTextMessageAsync(chatId, question.CorrectMessage);
            }
            else
            {
                await BotClient.SendTextMessageAsync(chatId, $"Неверно. Правильный ответ: {question.Options[question.CorrectOptionIndex]}");
            }

            currentQuestionIndex++;
            if (currentQuestionIndex < questions.Count)
            {
                await SendQuestion(chatId);
            }
            else
            {
                await BotClient.SendTextMessageAsync(chatId, "Викторина завершена! Спасибо за участие.");
            }
        }

        private static KeyboardButton[][] GetReplyKeyboard(List<string> options)
        {
            var keyboard = new KeyboardButton[options.Count][];
            for (int i = 0; i < options.Count; i++)
            {
                keyboard[i] = new KeyboardButton[] { new KeyboardButton(options[i]) };
            }
            return keyboard;
        }
    }

    class Question
    {
        public string Text { get; }
        public List<string> Options { get; }
        public int CorrectOptionIndex { get; }
        public string CorrectMessage { get; }

        public Question(string text, List<string> options, int correctOptionIndex, string correctMessage)
        {
            Text = text;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
            CorrectMessage = correctMessage;
        }
    }
}
