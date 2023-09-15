using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace SuncorCustomDialogsBot
{
    public class OpenAIAPI
    {

        private static readonly HttpClient httpClient;
        private static readonly dynamic appsettingClient;
        private static readonly string openAIApiKey;

        // Constructor
        static OpenAIAPI()
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            };
            httpClient = new HttpClient(socketsHandler);
            appsettingClient = new AppsettingReader();
            openAIApiKey = appsettingClient.ReadSecret<string>("OpenAIAPIKey");
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAI_Token);
        }

        public string getActionList()
        {
            string commands_list = "";
            BotAction.SupportedActions = appsettingClient.ReadSecret<List<string>>("SupportedActions");
            foreach (string action in BotAction.SupportedActions)
            {
                commands_list = String.Join(
                    Environment.NewLine,
                    $"{commands_list}",
                    $"- {action}"
                );
            }
            return commands_list;
        }

        // create_prompt overload for initial question
        public string createPrompt2Ask(string statement)
        {
            string pattern = @"
Statement:
{0}
----
Choose a command that matches the statement from the list below: 
{1}

You can choose 'nothing' if there is no match.
----
";
            string commands_list = getActionList();
            string prompt = String.Format(pattern, statement, commands_list);
            return prompt;
        }

        // create_prompt overload for creating prompt for missing words
        public string createPrompt2Ask(string missing_word, string action_name)
        {
            // missing_word must be string.
            // action_name is from initial question.
            // this is to ask OpenAI to generate a question which includes missing JSON keys.

            string pattern = @"
            Statement:
            {0}
            ----
            Use the keyword to ask a question about {1}.
            -----
            ";

            string prompt = String.Format(pattern, missing_word, action_name);
            return prompt;
        }

        public string createPrompt2Fill(string response, string missing_word)
        {
            // missing_word must be string.
            // action_name is from initial question.
            // this is to ask OpenAI to generate a question which includes missing JSON keys.

            string pattern = @"
Statement:
{0}
----
put this message into a JSON with key {1}.
-----
            ";

            string prompt = String.Format(pattern, response, missing_word);
            return prompt;
        }

        public string createPrompt2Parse(string response, string missing_word)
        {
            // missing_word must be string.
            // action_name is from initial question.
            // this is to ask OpenAI to generate a question which includes missing JSON keys.

            string pattern = @"
Statement:
{1} value is {0}
----
put this message into a JSON with key {1}.
-----
            ";
            string prompt = String.Format(pattern, response, missing_word);
            return prompt;
        }

        public string OnPostAsync(string curr_prompt)
        {
            try
            {
                var request = new OpenAIRequest
                {
                    Model = "text-davinci-003",
                    Prompt = curr_prompt, //this variable value changes to serve different query purposes.
                    Temperature = 0f,
                    MaxTokens = 100,
                    Top_P = 0f
                };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("api-key",openAIApiKey);
                using HttpResponseMessage response = httpClient.PostAsync("https://edwdevarmoaieu001.openai.azure.com/openai/deployments/text-davinci-003/completions?api-version=2022-12-01", content).Result;
                string resjson = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonConvert.DeserializeObject<OpenAIErrorResponse>(resjson);
                    throw new System.Exception(errorResponse.Error.Message);
                }

                dynamic data = JsonConvert.DeserializeObject(resjson);
                Console.WriteLine(data.choices[0].text);
                return data.choices[0].text;
            }
            catch (System.Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
    //  OpenAI Property Class. 
    public class OpenAIRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("temperature")]
        public float Temperature { get; set; }

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonProperty("top_p")]
        public float Top_P { get; set; }
        
    }

    public class OpenAIErrorResponse
    {
        [JsonProperty("error")]
        public OpenAIError Error { get; set; }
    }

    public class OpenAIError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("param")]
        public string Param { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
    }
    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
    public class Choice
    {
        public string text { get; set; }
        public int index { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class Clients
    {
        //  Initialize the object will be used to fill dialog
        // private JsonOps _jsonOps;
        public static readonly JsonOps _jsonOps = new JsonOps();
        public static readonly OpenAIAPI _openAIClient = new OpenAIAPI();
        public static readonly AppsettingReader _appSettingReaderClient = new AppsettingReader();
    }
}
