using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotFilesManager;
using TelegramBotFilesManager.Command;

public class TelegramBot
{
    private readonly TelegramBotClient _botClient;
    private readonly CancellationTokenSource cts;
    private AutoResetEvent _waitHandle = new AutoResetEvent(false);

    private readonly ConcurrentDictionary<long, RequestRateLimiter> _userRateLimiters = new ConcurrentDictionary<long, RequestRateLimiter>();

    public TelegramBot(string botToken)
    {
        cts = new();
        _botClient = new TelegramBotClient(botToken);
    }

    public void Start()
    {
        try
        {
            _botClient.StartReceiving(Update, Error);
            Console.WriteLine("Бот успешно запущен.");
            _waitHandle.WaitOne();
            Console.WriteLine("Бот завершил свою работу.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Бот завершил свою работу.");
        }
    }

    private async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        _ = Task.Run(async () =>
        {
            if (update != null && update.Message != null)
            {
                long userId = update.Message.From!.Id;

                if (!_userRateLimiters.TryGetValue(userId, out var rateLimiter))
                {
                    rateLimiter = new RequestRateLimiter();
                    _userRateLimiters.TryAdd(userId, rateLimiter);
                }

                if (!rateLimiter.CanMakeRequest())
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Превышено ограничение на количество запросов. Попробуйте позже.");
                    return;
                }

                rateLimiter.UpdateUserRequestTime();

                await Console.Out.WriteLineAsync($"[{DateTime.Now}] {update.Message.From.FirstName} {update.Message.From.LastName} {update.Message.From.Username}: {update.Message.Text ?? "Фотография"}");

                CommandHandler handler = new CommandHandler(update);
                ICommand command = await handler.GetCommandHandler(_botClient);
                command?.Execute(update);
            }
        });
    }

    private Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Произошла ошибка: {exception.Message}");
        return Task.CompletedTask;
    }

}



