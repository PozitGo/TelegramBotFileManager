using Newtonsoft.Json;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotFilesManager.Model;

namespace TelegramBotFilesManager.Command
{
    public class AddCommand : ICommand
    {
        private PhotoSize[] photo;
        private TelegramBotClient botClient;
        private HttpClient client;
        private const string URI = "https://syntaxjuggler.com/api/files/upload";
        public AddCommand(PhotoSize[] photo, TelegramBotClient botClient)
        {
            this.photo = photo;
            this.botClient = botClient;
            this.client = new();
        }

        public async Task Execute(Update update)
        {
            try
            {
                var fileId = update.Message!.Photo!.Last().FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);

                using var photoStream = new MemoryStream();
                await botClient.DownloadFileAsync(fileInfo.FilePath!, photoStream);

                string uniqueFileName = Guid.NewGuid().ToString() + ".jpg";

                using var formContent = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(photoStream.ToArray());
                formContent.Add(imageContent, "image", uniqueFileName);

                var response = await client.PostAsync(URI, formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    await botClient.SendTextMessageAsync(
                        chatId: update?.Message?.Chat.Id!,
                        text: $"Файл успешно отправлен, конечная точка полученной картинки https://syntaxjuggler.com/api/files/display/{JsonConvert.DeserializeObject<APIResponse>(responseContent)!.Data}");

                  await Task.Delay(1000);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update?.Message?.Chat.Id!,
                        text: "Ошибка загрузки фотографии");

                    await Task.Delay(1000);
                }

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }
    }

    public class DeleteCommand : ICommand
    {
        private string UUID;
        private TelegramBotClient botClient;
        private HttpClient client;
        private string URI;
        public DeleteCommand(string UUID, TelegramBotClient botClient)
        {
            this.UUID = UUID;
            this.URI = $"https://syntaxjuggler.com/api/files/delete/{UUID}";
            this.client = new HttpClient();
            this.botClient = botClient;

        }

        public async Task Execute(Update update)
        {

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, URI);
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                     text: $"Изображение удалено");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                     text: $"Изображение не найдено");
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                     text: $"Ошибка удаления изображения");
                }
            }
        }
    }

    public class GetCommad : ICommand
    {
        private TelegramBotClient botClient;
        private const string URI = "https://syntaxjuggler.com/api/files";
        public GetCommad(TelegramBotClient botClient)
        {
            this.botClient = botClient;
        }

        private IEnumerable<string> SplitTextIntoChunks(string text, int chunkSize)
        {
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
            }
        }

        public async Task Execute(Update update)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URI);
                HttpResponseMessage response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var maxMessageLength = 4096;

                    var temp = JsonConvert.DeserializeObject<APIResponse>(responseContent)?.Data?.ToString();
                    if (temp != null && temp != "[]")
                    {
                        if (temp.Length <= maxMessageLength)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: update?.Message?.Chat.Id!,
                                text: temp);
                        }
                        else
                        {
                            var chunks = SplitTextIntoChunks(temp, maxMessageLength);
                            foreach (var chunk in chunks)
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: update?.Message?.Chat.Id!,
                                    text: chunk);
                            }
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update?.Message?.Chat.Id!,
                            text: "Список пуст");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                     text: $"{JsonConvert.DeserializeObject<APIResponse>(responseContent)!.ErrorMessage}");
                }
            }
        }
    }

    public class GetFileNameCommad : ICommand
    {
        private string UUID;
        private TelegramBotClient botClient;
        private const string URI = "https://syntaxjuggler.com/api/Files/info/";
        public GetFileNameCommad(TelegramBotClient botClient, string UUID)
        {
            this.botClient = botClient;
            this.UUID = UUID;

        }
        public async Task Execute(Update update)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URI + UUID);
                HttpResponseMessage response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                     text: $"{JsonConvert.DeserializeObject<APIResponse>(responseContent)!.Data!.ToString()}");
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                    chatId: update?.Message?.Chat.Id!,
                     text: $"{JsonConvert.DeserializeObject<APIResponse>(responseContent)!.ErrorMessage}");
                }
            }
        }
    }

}
