using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System;




namespace SuncorCustomDialogsBot
{
    [Route("api/attachement")]
    public class MessagesController : Controller
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //File that Bot will send to the user. You can keep a file in your local directory
                //and can paste complete path here.
                string filePath = @"C:\Users\naxiao\source\repos\OpenAI_Project_Suncor\SuncorCustomDialogsBot\ReferenceFiles\Create_ServiceNow_Incident.json";
                //Convert to Uri. Bot can send absolute Uri path only as attachment.
                var uri = new Uri(filePath);
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity message = activity.CreateReply("Please check the image you have requested for.");
                //Create attachment
                Attachment attachment = new Attachment();
                attachment.Name = "Nate.JSON";
                // attachment.Content = "";
                attachment.ContentType = "application/json";
                //Content Url for the attachment can only be Absolute Uri
                attachment.ContentUrl = uri.AbsoluteUri;
                //Add attachment to the message
                message.Attachments.Add(attachment);
                //Send Reply message along with the attachment
                await connector.Conversations.ReplyToActivityAsync(message);
            }
            else
            {
                HandleSystemMessage(activity);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }

            return null;
        }
    }
}
