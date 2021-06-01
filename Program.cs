using EnglishBot.TgModels;
using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace EnglishBot
{
    class Program
    {
        //public static WordQuiz GlobalQuiz = new WordQuiz();

        //public static int GameIterator { get; set; }
        //public static int rights = 0;
        //public static int wrongs = 0;

        private static TelegramBotClient BotClient;

        public static List<TgUser> TelegramUsers { get; set; } = new List<TgUser>();

        //public static UserWords globalUserWord= new UserWords();

        static void Main(string[] args)
        {
            ApiSetting.InitializeClient();

            BotClient = new TelegramBotClient("1745546408:AAGrPpwXI0G0MoQucdl-bopeYW3kI16JXjU");
            BotClient.OnMessage += BotOnMessageRecieve;
            BotClient.OnCallbackQuery += BotOnCallBack;

            BotClient.StartReceiving();
            Console.ReadLine();
            BotClient.StopReceiving();
        }

        private static async void BotOnCallBack(object sender, CallbackQueryEventArgs e)
        {
           var user = TelegramUsers.Find(user => user.Id == e.CallbackQuery.From.Id);
           await ApiClient.GetByChatId($"{ApiSetting.Base}", e.CallbackQuery.From.Id, user.globalUserWord);
           await BotClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"The word is added");
        }

        private static async void BotOnMessageRecieve(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            if (TelegramUsers?.Find(tgUser => tgUser.Id == message.From.Id) == null)
            {
                TgUser tgUser = new TgUser(message.From.Id);
                TelegramUsers.Add(tgUser);
            }
            
            string name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"Message from {name}: {message.Text}");
            
            switch (message.Text)
            {
                case "/start":
                    string text =
$@"🤩 I am here to help you with learning English!
Use <i>/commands</i> to see what you can use me for";
                    await BotClient.SendTextMessageAsync(message.From.Id, text, parseMode: ParseMode.Html);
                    break;
                case "/commands":
                    string text2 =
$@"/vocabulary - learn new words
/word_quiz - play word game
/check_vocab - see your vocabulary
<b>Translate to uk <i>your phrase</i></b> - to get translate to Ukrainian
<b>Translate to en <i>your phrase</i></b> - to get translate to English
<b>What is <i>your word</i></b> - to get explanation of a word 
<b>Delete <i>word from vocabularly</i></b> - delete a word from your list of words
<b>Check: <i>your expression</i> - to check expression for grammar mistakes</b>";
                    await BotClient.SendTextMessageAsync(message.From.Id, text2, parseMode: ParseMode.Html);
                    break;
                case "/word_quiz":
                    WordQuiz wordQuizy = await ApiClient.PlayWordQuiz($"{ApiSetting.Base}quiz?level=4");
                    await wordQuiz(e, "3", wordQuizy);
                    break;
                case "/vocabulary":
                    await learnNewWord(e);
                    break;
                case "/check_vocab":
                    var userVocabs = await ApiClient.GetVocabFromDb($"{ApiSetting.Base}", message.From.Id);
                    await showUserVocab(message.From.Id, userVocabs);
                    break;
                default:
                    await additionalCommand(e);
                    break;
            }
            
        }

        public static async Task additionalCommand(MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Text.ToLower().StartsWith("what is"))
            {
                await whatIs(e);
                return;
            }
            else if (message.Text.ToLower().StartsWith("✏️"))
            {
                await quizCall(e);
            }
            else if (message.Text.ToLower().Trim().StartsWith("translate"))
            {
                await translate(message.Text, e);
            }
            else if (message.Text.ToLower().Trim().StartsWith("delete"))
            {
                await delete(e, message.Text.ToLower());
            }
            else if (message.Text.ToLower().Trim().StartsWith("check:"))
            {
                await checkForGrammar(e);
            }
            else
            {
                await smallTalk(e);
            }
        }

        public static async Task smallTalk(MessageEventArgs eve)
        {
            string phrase = eve.Message.Text;

            var talkResponse = await ApiClient.SmallTalk(phrase);

            if(talkResponse is null || talkResponse == "")
            {
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, "🤖Unknown command", parseMode: ParseMode.Html);
            }
            else
            {
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, $"🤖{talkResponse}", parseMode: ParseMode.Html);
            }
        }

        public static async Task checkForGrammar(MessageEventArgs eve)
        {
            string exp = eve.Message.Text.Trim().Substring(6).Trim();

            var grammarResp = await ApiClient.GrammarCheck(exp);

            if (grammarResp?.matches == null || grammarResp.matches.Count == 0)
            {
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, "<b>I didn't find any mistakes :)</b>", parseMode: ParseMode.Html);
            }
            else
            {
                var rule = grammarResp.matches.FirstOrDefault()?.rule?.description ?? "no rule";

                var message = grammarResp.matches.FirstOrDefault()?.Message ?? "no message";

                if (rule.Contains("spelling"))
                {
                    await BotClient.SendTextMessageAsync(eve.Message.From.Id, $"<b>{rule}</b>", parseMode: ParseMode.Html);
                }
                else
                {
                    if (rule != "no rule")
                    {
                        await BotClient.SendTextMessageAsync(eve.Message.From.Id, $"<b>{rule}</b>", parseMode: ParseMode.Html);
                    }

                    if (message != "no message")
                    {
                        await BotClient.SendTextMessageAsync(eve.Message.From.Id, $"<b>{message}</b>", parseMode: ParseMode.Html);
                    }
                }

            }
        }

        public static async Task wordQuiz(MessageEventArgs eve,string level, WordQuiz wordQuiz)
        {
            var user = TelegramUsers.Find(user => user.Id == eve.Message.From.Id);

            user.GlobalQuiz = wordQuiz;
            string text =
$@"Choose related word:
📚<i>{wordQuiz.Quizlist[user.GameIterator].Quiz[0]}</i>
📚<i>{wordQuiz.Quizlist[user.GameIterator].Quiz[1]}</i>
📚<i>{wordQuiz.Quizlist[user.GameIterator].Quiz[2]}</i>";

            var replyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton($"✏️{wordQuiz.Quizlist[user.GameIterator].Option[0]}")
                    },
                    new[]
                    {
                        new KeyboardButton($"✏️{wordQuiz.Quizlist[user.GameIterator].Option[1]}")
                    }
                });
            await BotClient.SendTextMessageAsync(eve.Message.From.Id, text, parseMode: ParseMode.Html, replyMarkup: replyKeyboard);
            
        }

        public static async Task learnNewWord(MessageEventArgs eve)
        {
            var user = TelegramUsers.Find(user => user.Id == eve.Message.From.Id);
            UserWords word = await ApiClient.LearnWord($"{ApiSetting.Base}new");
            if(word == null || word.EnglishWord == null ||word.Translation == null)
            {
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, @"<b>Something went wrong!</b>", parseMode: ParseMode.Html);
                return;
            }

            user.globalUserWord = word;

            string text;

            if (word.Explanation != "not specified")
            {
                text =
$@"✨New word:
<i>{word.EnglishWord} - {word.Explanation.Replace('/', '*')} - {word.Translation}</i>";
            }
            else
            {
                text =
$@"✨New word:
<i>{word.EnglishWord} - {word.Translation}</i>";
            }

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Add word to vocabulary")
                }
            });


            await BotClient.SendTextMessageAsync(eve.Message.From.Id, text, parseMode: ParseMode.Html, replyMarkup: inlineKeyboard);
        }

        public static async Task showUserVocab(int id,List<UserWords> vocabList)
        {
            if (vocabList == null)
            {
                string text = "<b>Your vocabulary is empty</b>";
                await BotClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html);
                return;
            }

            string vocab = $"Vocabulary: ";

            foreach(var word in vocabList)
            {
                if (word.Explanation == "not specified")
                {
                    string wordItem = $"\n📖 <i>{word.EnglishWord} - {word.Translation}</i>";
                    vocab += wordItem;
                }
                else
                {
                    string wordItem = $"\n📖 <i>{word.EnglishWord} - {word.Explanation.Replace('/', '*')} - {word.Translation}</i>";
                    vocab += wordItem;
                }
            }

            await BotClient.SendTextMessageAsync(id, vocab, parseMode: ParseMode.Html);
        }

        public static async Task quizCall(MessageEventArgs eve)
        {
            var user = TelegramUsers.Find(user => user.Id == eve.Message.From.Id);

            int gameCount = 5;
            string level = "6";
            string choice1 = $"✏️{user.GlobalQuiz.Quizlist[user.GameIterator].Option[0]}";
            string choice2 = $"✏️{user.GlobalQuiz.Quizlist[user.GameIterator].Option[1]}";

            if (eve.Message.Text == choice1)
            {
                if (1 == user.GlobalQuiz.Quizlist[user.GameIterator].Correct)
                {
                    await BotClient.SendTextMessageAsync(eve.Message.Chat, $"Correct!");
                    user.rights++;
                }
                else
                {
                    await BotClient.SendTextMessageAsync(eve.Message.Chat, $"Incorrect!");
                    user.wrongs++;
                }
            }
            else if (eve.Message.Text == choice2)
            {
                if (2 == user.GlobalQuiz.Quizlist[user.GameIterator].Correct)
                {
                    await BotClient.SendTextMessageAsync(eve.Message.Chat, $"Correct!");
                    user.rights++;
                }
                else
                {
                    await BotClient.SendTextMessageAsync(eve.Message.Chat, $"Incorrect!");
                    user.wrongs++;
                }
            }
            else
            {
                await BotClient.SendTextMessageAsync(eve.Message.Chat, $"Unexpected answer");
            }

            user.GameIterator++;
            if (user.GameIterator == gameCount)
            {
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, $"<i>Game is over!\nYour score: {user.rights} ✅ and {user.wrongs} ❌</i>", parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                user.GameIterator = 0;
                user.wrongs = 0;
                user.rights = 0;
                user.GlobalQuiz = null;
                return;
            }
            await wordQuiz(eve, level, user.GlobalQuiz);
            
        }

        public static async Task whatIs(MessageEventArgs e)
        {
            Vocabular vocabInfo = new Vocabular();

            var message = e.Message;

            string expression = message.Text.Substring(7).Trim().ToLower();
            try
            {
                vocabInfo = await ApiClient.GetInfo($"{ApiSetting.Base}word?name={expression}");
            }
            catch
            {

            }

            if (vocabInfo == null || vocabInfo.Word == null)
            {
                string invalid = @"<i>I think you typed it wrong. Please, try again)</i>";
                await BotClient.SendTextMessageAsync(message.From.Id, invalid, parseMode: ParseMode.Html);
                return;
            }

            string def = "";
            string synonyms = "";
            string antonyms = "";
            string usage = "";


            if (vocabInfo.Usage != null && vocabInfo.Usage.Count != 0)
            {
                usage = vocabInfo.Usage.FirstOrDefault();
            }

            foreach (var el in vocabInfo.Definition)
            {
                def = def + el + " ";
                if (def.Split(' ').Length > 10)
                {
                    break;
                }
            }

            if (vocabInfo.Synonyms.Count != 0)
            {
                foreach (var el in vocabInfo.Synonyms)
                {
                    synonyms += $"{el}, ";
                }
            }
            else
            {
                synonyms = "not found";
            }

            if (vocabInfo.Antonyms.Count != 0)
            {
                foreach (var el in vocabInfo.Antonyms)
                {
                    antonyms += $"{el}, ";
                }
            }
            else
            {
                antonyms = "not found";
            }

            await BotClient.SendTextMessageAsync(message.From.Id, $"📕{def}");

            if (usage != "")
            {
                await BotClient.SendTextMessageAsync(message.From.Id, $"📗 Usage example: \n<i>{usage}</i>", parseMode: ParseMode.Html);
            }

            if (synonyms != "not found" && antonyms != "not found")
            {
                await BotClient.SendTextMessageAsync(message.From.Id, $"📙 \n<b>Synonyms:</b> <i>{synonyms}</i>\n<b>Antonyms:</b> <i>{antonyms}</i>", parseMode: ParseMode.Html);
            }

            
        }

        public static async Task translate(string word, MessageEventArgs eve)
        {
            
            string requestWord = word.Substring(15).Trim();

            string lang = word.Split(" ")[2];

            var translation = await ApiClient.GetTranslate($"{ApiSetting.Base}translate?phraseItself={requestWord}&toLang={lang}");

            if (translation == null)
            {
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, @"<b>I can't find translation(</b>", parseMode: ParseMode.Html);
                return;
            }

            string text = $"📖<i>{(requestWord).ToUpper()}</i> - *<b>{translation.data.translation}</b>*";

            await BotClient.SendTextMessageAsync(eve.Message.From.Id, text, parseMode: ParseMode.Html);
        }

        public static async Task delete(MessageEventArgs eve, string word)
        {
            string strDel = word.Trim().Split(" ")[1].Trim();

            UserWords itemDel = new UserWords();

            var userVocabs = await ApiClient.GetVocabFromDb($"{ApiSetting.Base}", eve.Message.From.Id);

            if (userVocabs == null)
            {
                string text = "<b>Your vocabulary is empty</b>";
                await BotClient.SendTextMessageAsync(eve.Message.From.Id, text, parseMode: ParseMode.Html);
                return;
            }

            foreach(var vocab in userVocabs)
            {
                if(vocab.EnglishWord == strDel)
                {
                    itemDel = vocab;
                }
            }

            if(itemDel != null && itemDel.EnglishWord!=null)
            {
                var status = await ApiClient.DeleteWord($"{ApiSetting.Base}{eve.Message.From.Id}", itemDel, eve.Message.From.Id);
                if(status == "NoContent")
                {
                    await BotClient.SendTextMessageAsync(eve.Message.From.Id, "<b>Word was deleted</b>", parseMode: ParseMode.Html);
                    return;
                }
                else
                {
                    await BotClient.SendTextMessageAsync(eve.Message.From.Id, "<b>Word wasn't found</b>", parseMode: ParseMode.Html);
                    return;
                }
            }

            await BotClient.SendTextMessageAsync(eve.Message.From.Id, "<b>Word wasn't found</b>", parseMode: ParseMode.Html);
        }
    }
}
