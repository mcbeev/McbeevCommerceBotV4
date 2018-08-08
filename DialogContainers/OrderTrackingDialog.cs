using System.Collections.Generic;
using System.Threading.Tasks;
using McbeevCommerceBot.Infrastructure;
using McbeevCommerceBot.Models;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Newtonsoft.Json.Linq;

namespace McbeevCommerceBot.DialogContainers
{
    public class OrderTrackingDialog : DialogContainer
    {
        public const string _dialogId = "trackOrder";

        private KenticoRestService _kenticoRestService;

        public OrderTrackingDialog(KenticoRestServiceSettings kenticoRestServiceSettings) : base(_dialogId)
        {
            _kenticoRestService = new KenticoRestService(kenticoRestServiceSettings);

            Dialogs.Add(_dialogId, new WaterfallStep[]
            {
                AskOrderLookupInfo,
                ReturnOrderTrackingNumber
            });
            Dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }

        // This is the first step of the Order History dialog
        private async Task AskOrderLookupInfo(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var dialogState = dc.ActiveDialog.State as IDictionary<string, object>;
            if (args != null)
            {
                //Check if the LuisResult contains our tracking number entity or not
                if (args["LuisResult"] is RecognizerResult luisResult)
                {
                    var orderNumber = GetEntity<string>(luisResult, "ordernumber");
                    if (!string.IsNullOrEmpty(orderNumber))
                    {
                        dialogState.Add("OrderNumber", orderNumber);
                    }
                }
            }

            // Save info back to the dialog instance
            dc.ActiveDialog.State = dialogState;

            //Now if we don't need it, don't ask the user to answer this question
            if (!dialogState.ContainsKey("OrderNumber"))
            {
                await dc.Context.SendActivity("Great, I just need one piece of information from you.");
                await dc.Prompt("textPrompt", "Can I get your order number for the order?");
            }
            else
            {
                await dc.Context.SendActivity($"Great, I see your order number is {dc.ActiveDialog.State["OrderNumber"]}, let me go find your tracking number now.");

                // so go to the next step in the waterfall
                await dc.Continue();
            }
        }

        private async Task ReturnOrderTrackingNumber(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var message = string.Empty;

            TextResult orderNumberResult = (TextResult)args;
            
            var trackingNumber = await _kenticoRestService.GetEcommerceOrderTrackingNumberByOrderNumber(orderNumberResult.Value);
            if (!string.IsNullOrEmpty(trackingNumber))
            {
                message = $"Your tracking number is **{trackingNumber}**\n";
            }
            else
            {
                message = $"Sorry we could not find an order with that Order Number, or there is no Tracking Number yet for Order Number **{orderNumberResult.Value}**\n";
            }

            await dc.Context.SendActivity(message);

            await dc.End(dc.ActiveDialog.State);
        }

        private T GetEntity<T>(RecognizerResult luisResult, string entityKey)
        {
            var data = luisResult.Entities as IDictionary<string, JToken>;
            if (data.TryGetValue(entityKey, out JToken value))
            {
                return value.First.Value<T>();
            }
            return default(T);
        }
    }
}
