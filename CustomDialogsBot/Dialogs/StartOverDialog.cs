using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace SuncorCustomDialogsBot.Dialogs
{
    public class StartOverDialog : ComponentDialog
    {
        public StartOverDialog() : base(nameof(StartOverDialog)) 
        {
            AddDialog(new GetActionDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                StartOverInitialAsync,
                StartOverFinalAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        


        private async Task<DialogTurnResult> StartOverInitialAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please tell me what to do."),
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            // Prompt the user for a choice.

        }

        private async Task<DialogTurnResult> StartOverFinalAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(GetActionDialog), null, cancellationToken);
        }
    }
}
