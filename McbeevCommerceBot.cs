using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using McbeevCommerceBot.DialogContainers;
using Prompts = Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;
using System.Linq;
using McbeevCommerceBot.Infrastructure;
using Microsoft.Extensions.Configuration;
using McbeevCommerceBot.Models;
using Microsoft.Extensions.Options;

namespace McbeevCommerceBot
{
    public class McbeevCommerceBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet dialogs;

        private readonly IOptions<KenticoRestServiceSettings> _settings;

        public McbeevCommerceBot(IOptions<KenticoRestServiceSettings> kenticoRestServiceSettings)
        {
            _settings = kenticoRestServiceSettings;
            
            dialogs = new DialogSet();
            dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("OrderHistory", new OrderHistoryDialog(_settings.Value));
            dialogs.Add("OrderTrackingNumber", new OrderHistoryDialog(_settings.Value));
            dialogs.Add("PlaceOrder", new OrderHistoryDialog(_settings.Value));
            
        }

        /// <summary>
        /// Main bot OnTurn Implementation
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed for processing this conversation turn. </param>        
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.ConversationUpdate && context.Activity.MembersAdded.FirstOrDefault()?.Id == context.Activity.Recipient.Id)
            {
                await context.SendActivity("Hi! Welcome to the Mcbeev Commerce Bot.");
            }
            else if (context.Activity.Type == ActivityTypes.Message)
            {
                var userState = context.GetUserState<UserState>();

                var state = context.GetConversationState<Dictionary<string, object>>();
                var dialogContext = dialogs.CreateContext(context, state);

                var utterance = context.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        await context.SendActivity("Ok... Cancelled");
                        dialogContext.EndAll();
                    }
                    else
                    {
                        await context.SendActivity("Nothing to cancel.");
                    }
                }

                if (!context.Responded)
                {
                    var dialogArgs = new Dictionary<string, object>();

                    await dialogContext.Continue();

                    if (!context.Responded)
                    {
                        var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                        var (intent, score) = luisResult.GetTopScoringIntent();
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";
                        dialogArgs.Add("LuisResult", luisResult);

                        await dialogContext.Begin(intent, dialogArgs);
                    }
                }
            }
        }

        private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var intent = dialogContext.ActiveDialog.Id;
            return dialogContext.Context.SendActivity($"No intent matches: {intent}");
        }
    }
}
