using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using System;
using SuncorCustomDialogsBot.Dialogs;
using System.Collections;
using Microsoft.AspNetCore.Http;

namespace SuncorCustomDialogsBot.Dialogs
{
    public class CollectJsonDataDialog : ComponentDialog
    {
        private readonly IList _MissingWords;
        public CollectJsonDataDialog() : base(nameof(CollectJsonDataDialog))
        {
            AddDialog(new MissingWordFilling());
            AddDialog(new SendAttachment());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CollectDataStepAsync,
                AttachmentStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        public async Task<DialogTurnResult> CollectDataStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            BotAction.ActionData = Clients._jsonOps.GetJsonFromFile(string.Format(@".\ReferenceFiles\{0}.json", BotAction.ActionName));
            BotAction.MissingWords = Clients._jsonOps.GetNullOrEmptyProperties(BotAction.ActionData);
            BotAction.ActionHelperData = Clients._jsonOps.GetJsonFromFile(string.Format(@".\ReferenceFiles\{0}_Helper.json", BotAction.ActionName));

            return await stepContext.BeginDialogAsync(nameof(MissingWordFilling), null, cancellationToken);
            //return await stepContext.EndDialogAsync();
        }
        public async Task<DialogTurnResult> AttachmentStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(SendAttachment), null, cancellationToken);
        }
        
    }
}
