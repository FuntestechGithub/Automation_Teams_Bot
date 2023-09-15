// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;
using SuncorCustomDialogsBot.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuncorCustomDialogsBot
{
    /// <summary>
    /// This is an example root dialog. Replace this with your applications.
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        private readonly UserState _userState;

        public MainDialog(UserState userState)
            : base(nameof(MainDialog))
        {
            _userState = userState;

            // Add all dialogs
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new GetActionDialog());
            AddDialog(new NoMatchingDialog());

            // This array defines how the Waterfall will execute.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(GetActionDialog), null, cancellationToken);
        }
    }
}
