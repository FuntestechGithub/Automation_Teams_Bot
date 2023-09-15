using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.IO;
using System;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;

namespace SuncorCustomDialogsBot
{
    public class SendAttachment : ComponentDialog
    {
        public SendAttachment() : base(nameof(SendAttachment))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                UploadAttachmentAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        [HttpPost]
        // Creates an "Attachment" to be sent from the bot to the user from an uploaded file.
        private static async Task<DialogTurnResult> UploadAttachmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var reply = await HandleOutgoingAttachment(stepContext.Context, stepContext.Context.Activity, cancellationToken);
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        private static async Task<IMessageActivity> HandleOutgoingAttachment(ITurnContext turnContext, IMessageActivity activity, CancellationToken cancellationToken)
        {
            IMessageActivity reply = null;
            var uploadedAttachment = await GetUploadedAttachmentAsync(turnContext, activity.ServiceUrl, activity.Conversation.Id, cancellationToken);

            reply = MessageFactory.Text("This is an attachment for the action you picked.");
            reply.Attachments = new List<Attachment>() { uploadedAttachment };
            return reply;
        }

        private static async Task<Attachment> GetUploadedAttachmentAsync(ITurnContext turnContext, string serviceUrl, string conversationId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }


            var connector = turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;
            var attachments = new Attachments(connector);

            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                conversationId,
                new AttachmentData
                {
                    Name = @"Create_ServiceNow_Incident.json",
                    // OriginalBase64 = File.ReadAllBytes(filePath),
                    // converting string to Base64 OriginalBase64 = System.Text.Encoding.UTF8.GetBytes(content),
                    OriginalBase64 = System.Text.Encoding.UTF8.GetBytes(BotAction.ActionData.ToString(Formatting.None)),
                    Type = "application/word",
                },
                cancellationToken);


            var attachmentUri = attachments.GetAttachmentUri(response.Id);

            return new Attachment
            {
                Name = @"Create_ServiceNow_Incident.json",
                ContentType = "application/word",
                ContentUrl = attachmentUri,
            };
        }
    }
}
