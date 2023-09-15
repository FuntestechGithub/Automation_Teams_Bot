using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Diagnostics;
using Microsoft.Identity.Client;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace SuncorCustomDialogsBot
{
    public class MissingWordFilling : ComponentDialog
    {
        private string _missingWord;
        private JToken _helperData;
        private JObject _responseConvert2Json;
        private string _userResponse;
        public MissingWordFilling() : base(nameof(MissingWordFilling))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            var waterfallSteps = new WaterfallStep[]
            {
                QAStepAsync,
                ReviewStepAsync,
                LoopStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);      
        }


        public async Task<DialogTurnResult> QAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If there is still 
            if (BotAction.MissingWords.Any())
            { 
                _missingWord = BotAction.MissingWords.First();
                _helperData = BotAction.ActionHelperData[_missingWord];
                string textPrompt = string.Format(@"What is {0}?", _helperData);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(textPrompt) }, cancellationToken);
                // return await stepContext.NextAsync();
            }
            return await stepContext.EndDialogAsync();
        }

        public async Task<DialogTurnResult> ReviewStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var currPrompt = Clients._openAIClient.createPrompt2Fill((string)stepContext.Result, _missingWord);
            string response = Clients._openAIClient.OnPostAsync(currPrompt);
            // convert to json
            _responseConvert2Json = Clients._jsonOps.ConvertString2Jobject(response);
            _userResponse = (string)_responseConvert2Json.SelectToken(_missingWord);
            string textPrompt = string.Format(@"Your input value of ' {0} ' is {1}. Can you confirm if this is correct?", _helperData, _userResponse);
            return await stepContext.PromptAsync(nameof(ChoicePrompt), 
                new PromptOptions { 
                    Prompt = MessageFactory.Text(textPrompt),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Correct", "Incorrect" }),
                }, cancellationToken);
        }

        public async Task<DialogTurnResult> LoopStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            if (choice.Value == "Correct")
            {
                Clients._jsonOps.SetNullOrEmptyValue(BotAction.ActionData, _responseConvert2Json, _missingWord);
                BotAction.MissingWords.Remove(_missingWord);
            }
            else
            {
                await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("OK, let us try the question again."),
                }, cancellationToken);
            }
            return await stepContext.ReplaceDialogAsync(nameof(MissingWordFilling), null, cancellationToken);
        }
    }
}
