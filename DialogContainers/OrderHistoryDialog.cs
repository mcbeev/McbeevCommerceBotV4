using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McbeevCommerceBot.Infrastructure;
using McbeevCommerceBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;


namespace McbeevCommerceBot.DialogContainers
{
    public class OrderHistoryDialog : DialogContainer
    {
        public const string _dialogId = "findOrderHistory";

        private KenticoRestService _kenticoRestService;

        public OrderHistoryDialog(KenticoRestServiceSettings kenticoRestServiceSettings) : base(_dialogId)
        {
            _kenticoRestService = new KenticoRestService(kenticoRestServiceSettings);

            Dialogs.Add(_dialogId, new WaterfallStep[]
            {
                AskOrderLookupInfo,
                AskOrderLookupValidationInfo,
                ReturnOrderHistory
            });
            Dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }

        // This is the first step of the Order History dialog
        private async Task AskOrderLookupInfo(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var dialogState = dc.ActiveDialog.State as IDictionary<string, object>;
            if (args != null)
            {
                //Check if the LuisResult contains our email entity or not
                if (args["LuisResult"] is RecognizerResult luisResult)
                {
                    var email = GetEntity<string>(luisResult, "email");
                    if (!string.IsNullOrEmpty(email))
                    {
                        dialogState.Add("EmailAddress", email);
                    }
                }
            }

            // Save info back to the dialog instance
            dc.ActiveDialog.State = dialogState;

            //Now if we don't need it, don't ask the user to answer this question
            if (!dialogState.ContainsKey("EmailAddress"))
            {
                await dc.Context.SendActivity("Great, I just need a few pieces of information from you.");
                await dc.Prompt("textPrompt", "Can I get your email address for the order?");
            }
            else
            {
                await dc.Context.SendActivity($"Great, I see your email address is {dc.ActiveDialog.State["EmailAddress"]}, now I just need one more piece of information from you.");

                // so go to the next step in the waterfall
                await dc.Continue();
            }
        }

        private async Task AskOrderLookupValidationInfo(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            //We might already have this from the LuisResult, if we do, don't try to grab it from the prior prompt
            if (!dc.ActiveDialog.State.ContainsKey("EmailAddress"))
            {
                TextResult emailResult = (TextResult)args;
                dc.ActiveDialog.State["EmailAddress"] = emailResult.Text;
            }

            await dc.Prompt("textPrompt", "Please enter your billing ZipCode");
        }

        private async Task ReturnOrderHistory(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var message = string.Empty;

            //Grab the response and save it to state
            TextResult zipResult = (TextResult)args;
            dc.ActiveDialog.State["ZipCode"] = zipResult.Text;

            var email = dc.ActiveDialog.State["EmailAddress"].ToString();

            //Find customer by email
            Customer c = await _kenticoRestService.GetEcommerceCustomerByEmail(email);
            if (c.CustomerID > 0)
            {
                message += $"Thank you {c.CustomerFirstName}, ";

                List<Order> orders = await _kenticoRestService.GetEcommerceOrdersByCustomer(c.CustomerID, zipResult.Text);
                if (orders.Count > 0)
                {
                    message += $"we found {orders.Count} orders with that email address.\n";

                    int i = 1;
                    foreach (Order o in orders)
                    {
                        message += $"{i}. On {o.OrderDate}\n Order #**{o.OrderNumber}** was placed for a total of **{o.OrderTotalPrice:c}**.\n This order has a status of **{o.OrderStatusName}**.\n";
                        foreach (OrderItem oi in o.OrderItems)
                        {
                            message += $"* {oi.SkuName} ({oi.Quantity} @ {oi.ItemPrice:c})\n";
                        }
                        i++;
                        message += "\n";
                    }
                }
                else
                {
                    message += $"I was unable to find orders for that customer, or the zip code that was provided ({zipResult.Text}) didn't match.";
                }
            }
            else
            {
                message += $"However, I'm sorry we could not find any customers with an email address of {email}";
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
