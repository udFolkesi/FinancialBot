using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinancialBot
{
    internal class Program
    {
        private static readonly string token = "8159298599:AAGPkIEtfH85CJKBiDGTEsmQXyQQew725_w";
        private static TelegramBotClient botClient;
        private static AnketaService anketaService = new();

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient(token);

            //InitBotCommands().GetAwaiter().GetResult();

            var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Бот запущен.");
            Console.ReadLine();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            // Обработка callback-кнопок
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callback = update.CallbackQuery;
                var data = callback.Data;

                if (data.StartsWith("info_"))
                {
                    // Здесь может быть обращение к API или БД
                    string infoText = "📈 Пример:\n\n" +
                                      "📌 ETF S&P 500 — индексный фонд\n" +
                                      "📌 Доходность за 5 лет: ~+60%\n" +
                                      "📌 Волатильность: средняя\n" +
                                      "✅ Подходит для умеренного риска.";

                    await bot.AnswerCallbackQuery(callback.Id);
                    await bot.SendMessage(callback.Message.Chat.Id, infoText);
                }
                else if (data == "/profile")
                {
                    anketaService.StartAnketa(callback.From.Id);
                    await bot.SendMessage(callback.Message.Chat.Id, "📋 Вопрос 1: Сколько вам лет?");
                }

                return;
            }

            // Обработка текстовых сообщений
            var message = update.Message;
            if (message == null || message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                return;

            long userId = message.From.Id;
            string text = message.Text;

            if (text == "/start")
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
        new KeyboardButton[] { "📋 Начать анкету", "ℹ️ Помощь" }
    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false
                };

                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "👋 Привет! Я бот, который поможет тебе с инвестициями.",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                return;
            }

            if (text == "/help")
            {
                string helpText = "📘 *Помощь*\n\n" +
                                  "Я инвестиционный бот. Вот что я умею:\n\n" +
                                  "📋 *Пройти анкету* — определить ваш профиль инвестора\n" +
                                  "📈 *Получить рекомендации* по стратегиям и продуктам\n" +
                                  "📊 *Подробнее о продуктах* — узнать об ETF, акциях и др.\n" +
                                  "🔁 *Пройти снова* — перезапустить анкету\n\n" +
                                  "⌨️ Используйте кнопки ниже или команды: `/start`, `/profile`, `/help`.";

                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: helpText,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                return;
            }


            if (text == "📋 Начать анкету" || text == "/profile")
            {
                anketaService.StartAnketa(userId);
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
        new KeyboardButton[] { "◀️ Назад" }
    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false
                };

                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "📋 Вопрос 1: Сколько вам лет?",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                return;
            }

            // Обработка команды "Назад"
            if (text == "◀️ Назад")
            {
                var previousQuestion = anketaService.GoBack(userId);

                if (previousQuestion != null)
                {
                    await bot.SendMessage(
                        chatId: message.Chat.Id,
                        text: previousQuestion,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Вы находитесь на первом вопросе анкеты.",
                        cancellationToken: cancellationToken);
                }
                return;
            }

            // Ответы на анкету
            var result = anketaService.ProcessAnswer(userId, text);

            if (result.Question != null)
            {
                // Вопрос 2 — кнопки про риск
                if (result.Question.Contains("риску"))
                {
                    var riskKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                new KeyboardButton[] { "Негативно", "Нейтрально", "Положительно"},
                            ["◀️ Назад"]

            })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };

                    await bot.SendMessage(
                        chatId: message.Chat.Id,
                        text: result.Question,
                        replyMarkup: riskKeyboard,
                        cancellationToken: cancellationToken);
                }
                // Вопрос 3 — кнопки про срок
                else if (result.Question.Contains("срок"))
                {
                    var horizonKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                new KeyboardButton[] { "Краткосрочно", "Среднесрочно", "Долгосрочно" },
                ["◀️ Назад"]
            })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };

                    await bot.SendMessage(
                        chatId: message.Chat.Id,
                        text: result.Question,
                        replyMarkup: horizonKeyboard,
                        cancellationToken: cancellationToken);
                }
                // Простой текстовый вопрос
                else
                {
                    await bot.SendMessage(
                        chatId: message.Chat.Id,
                        text: result.Question,
                        cancellationToken: cancellationToken);
                }
            }
            else if (result.Recommendation != null)
            {
                // Финальный блок с рекомендацией
                var rec = result.Recommendation;
                var recommendationText = $"🎯 Ваш инвестиционный профиль: {rec.Profile}\n\n" +
                                         $"📈 Стратегия: {rec.Strategy}\n\n" +
                                         $"💼 Рекомендуемые продукты:\n" +
                                         string.Join("\n", rec.Products);

                // Убираем клавиатуру
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: recommendationText,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);

                // Показываем inline-кнопки
                var buttons = new InlineKeyboardMarkup(new[]
                {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Подробнее о продуктах", $"info_{userId}"),
                InlineKeyboardButton.WithCallbackData("🔁 Пройти снова", "/profile")
            }
        });

                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Что бы вы хотели сделать дальше?",
                    replyMarkup: buttons,
                    cancellationToken: cancellationToken);
            }
        }


        static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static async Task InitBotCommands()
        {
            await botClient.SetMyCommands(new[]
            {
        new BotCommand { Command = "start", Description = "Начать работу с ботом" },
        new BotCommand { Command = "profile", Description = "Пройти анкету" }
        // Добавь другие команды, если нужно
    });
        }

    }
}
