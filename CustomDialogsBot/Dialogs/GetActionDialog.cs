using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using System;
using SuncorCustomDialogsBot.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using System.Diagnostics;

namespace SuncorCustomDialogsBot
{
    public class GetActionDialog : ComponentDialog
    {
        public GetActionDialog():
            base(nameof(GetActionDialog))
        {
            AddDialog(new CollectJsonDataDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetActionNameAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetActionNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var question4Action = Clients._openAIClient.createPrompt2Ask(stepContext.Context.Activity.Text);
            BotAction.ActionName = Clients._openAIClient.OnPostAsync(question4Action);
            

            if (BotAction.ActionName.Contains("Nothing"))
            {
                // If no matching action, list supported actions and let user pick
                return await stepContext.BeginDialogAsync(nameof(NoMatchingDialog), BotAction.SupportedActions, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(CollectJsonDataDialog), null, cancellationToken);
            }
        }
    }
}
