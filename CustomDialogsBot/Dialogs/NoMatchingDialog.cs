using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using SuncorCustomDialogsBot.Dialogs;

namespace SuncorCustomDialogsBot
{
    public class NoMatchingDialog : ComponentDialog
    {
        public NoMatchingDialog() : base(nameof(NoMatchingDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new GetActionDialog());
            AddDialog(new StartOverDialog());
            AddDialog(new CollectJsonDataDialog());
            AddDialog(new SendAttachment());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SelectionStepAsync,
                LoopStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            BotAction.SupportedActions.Add("Start_Over");
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("We current don't support the action you picked. Please pick one of the following supported actions."),
                RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                Choices = ChoiceFactory.ToChoices(BotAction.SupportedActions),
            };
            // Prompt the user for a choice.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            
            var choice = (FoundChoice)stepContext.Result;
            BotAction.SupportedActions.Remove("Start_Over");
            BotAction.ActionName = choice.Value;
            switch (choice.Value)
            {
                case "Create_ServiceNow_Incident":
                    return await stepContext.BeginDialogAsync(nameof(SendAttachment), null, cancellationToken);
                case "Create_NSG_TF_File":
                    // use picked value to call next Dialog.
                    return await stepContext.BeginDialogAsync(nameof(CollectJsonDataDialog), null, cancellationToken);
            }

            return await stepContext.BeginDialogAsync(nameof(StartOverDialog), null, cancellationToken);
        }
    }
}
