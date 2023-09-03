using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotFilesManager.Command
{
    public interface ICommand
    {
        Task Execute(Update update);
    }
}
