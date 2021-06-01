using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using EnglishBot.TgModels;
using Newtonsoft.Json;

namespace EnglishBot
{
    class ApiClient
    {
        public static async Task<Vocabular> GetInfo(string url)
        {
            using (HttpResponseMessage response = await ApiSetting.EngApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<Vocabular>(content);
                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<List<UserWords>> GetVocabFromDb(string url, int chatId)
        {
            var res = await ApiSetting.EngApiClient.GetAsync($"{url}{chatId}");
            res.EnsureSuccessStatusCode();


            if(res.ToString() == "NoContent")
            {
                return null;
            }
            
            string responseBody = await res.Content.ReadAsStringAsync();

            if (responseBody == null || responseBody=="")
            {
                return null;
            }

            var user = JsonConvert.DeserializeObject<DbUser>(responseBody);

            return user.VocabItems;
            
        }

        public static async Task<string> DeleteWord( string url, UserWords userWord, int chatId)
        {
            
            var json = JsonConvert.SerializeObject(userWord);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ApiSetting.EngApiClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = data });

            response.EnsureSuccessStatusCode();

            return (response.StatusCode.ToString());
        }

        public static async Task<WordQuiz> PlayWordQuiz(string url)
        {
            using (HttpResponseMessage response = await ApiSetting.EngApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<WordQuiz>(content);
                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<Translation> GetTranslate(string url)
        {
            using (HttpResponseMessage response = await ApiSetting.EngApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<Translation>(content);
                    if (result == null || result.data == null)
                    {
                        return null;
                    }

                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<Grammar> GrammarCheck(string exp)
        {
            string url = $"{ApiSetting.Base}grammar?exp={exp}";

            using (HttpResponseMessage response = await ApiSetting.EngApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<Grammar>(content);
                    if (result?.matches == null || result.matches.Count == 0)
                    {
                        return null;
                    }

                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<string> SmallTalk(string phrase)
        {
            string url = $"{ApiSetting.Base}talk?phrase={phrase}";

            using (HttpResponseMessage response = await ApiSetting.EngApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    return content;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<UserWords> LearnWord(string url)
        {
            
            using (HttpResponseMessage response = await ApiSetting.EngApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<UserWords>(content);
                    if (result?.EnglishWord == null)
                    {
                        return null;
                    }
                    
                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task GetByChatId(string url, int chatId, UserWords userWords)
        {
            var signedUser = ApiSetting.EngApiClient.GetAsync($"{url}{chatId}");

            var st = signedUser.Result.StatusCode;

            if((st).ToString() == "NoContent")
            {
                await CreateUserVocab(url, chatId);
            }

            /*if ()
            {
                await CreateUserVocab(url, chatId);
            }*/

            await AddToVocab(url, chatId, userWords);

        }

        public static async Task CreateUserVocab(string url, int chatId)
        {
            
            var newUser = new DbUser
            {
                ChatId = chatId,
                VocabItems = new List<UserWords>()
            };


            var json = JsonConvert.SerializeObject(newUser);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ApiSetting.EngApiClient.PostAsync(url, data);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Succesfully saved");
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            
        }

        public static async Task AddToVocab(string url, int chatId, UserWords userWords)
        {
            var json = JsonConvert.SerializeObject(userWords);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ApiSetting.EngApiClient.PostAsync($"{url}{chatId}", data);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Succesfully saved");
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}
