using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotFilesManager.Command
{
    internal class CommandHandler
    {
        private Update update;
        public CommandHandler(Update update)
        {
            this.update = update;
        }

        public async Task<ICommand> GetCommandHandler(TelegramBotClient botClient)
        {
            var message = update.Message;
            if(message?.Photo != null && message.Photo.Length > 0)
            {   
                return new AddCommand(message.Photo, botClient);
            }
            else if (message?.Text != null)
            {
                if (message.Text.Split(" ")[0].ToLower() == "delete")
                {
                    return new DeleteCommand(message.Text.Split(" ")[1].ToLower(), botClient);
                }
                else if (message?.Text.Split(" ")[0].ToLower() == "info")
                {
                    return new GetFileNameCommad(botClient, message.Text.Split(" ")[1].ToLower());
                }
                else if (message?.Text?.ToLower() == "list")
                {
                    return new GetCommad(botClient);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                    text: $"Команда не найдена");
                    return null!;
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                chatId: update?.Message?.Chat.Id!,
                text: $"Команда не найдена");
                return null!;
            }
        }
    }
}
