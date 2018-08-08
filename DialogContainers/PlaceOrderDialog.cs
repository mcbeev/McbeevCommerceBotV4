using System.Collections.Generic;
using System.Threading.Tasks;
using McbeevCommerceBot.Infrastructure;
using McbeevCommerceBot.Models;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;

namespace McbeevCommerceBot.DialogContainers
{
    public class PlaceOrderDialog : DialogContainer
    {
        public const string _dialogId = "placeOrder";

        private KenticoRestService _kenticoRestService;

        public PlaceOrderDialog(KenticoRestServiceSettings kenticoRestServiceSettings) : base(_dialogId)
        {
            _kenticoRestService = new KenticoRestService(kenticoRestServiceSettings);

            Dialogs.Add(_dialogId, new WaterfallStep[]
            {
                ChooseProductsToBuy,
                AskHowManyToBuy,
                PassProductToShoppingCartOnWebsite
            });

            var dynamicPrompt = new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English);
            dynamicPrompt.Style = ListStyle.Auto;
            //dynamicPrompt.ChoiceOptions.IncludeNumbers = false;

            Dialogs.Add("productsPrompt", dynamicPrompt);
            Dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            Dialogs.Add("numberPrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
        }

        private async Task ChooseProductsToBuy(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var cardPrompt = new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(Culture.English)
            {
                Style = Microsoft.Bot.Builder.Prompts.ListStyle.List
            };
            ChoicePromptOptions cardOptions = GenerateOptions(dc);
            
            await dc.Prompt("productsPrompt", "Which of our popular items would you like to add to your order:", cardOptions).ConfigureAwait(false);           
        }

        private async Task AskHowManyToBuy(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var chosenChoice = (FoundChoice)args["Value"];
            dc.ActiveDialog.State.Add("chosenChoice", chosenChoice.Index);

            await dc.Prompt("numberPrompt", "How many would you like to purchase?", new PromptOptions()
            {
                RetryPromptString = "Sorry, please specify the quantity in a number format."
            }).ConfigureAwait(false);
        }

        private async Task PassProductToShoppingCartOnWebsite(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            List<SKU> skus = (List<SKU>)dc.ActiveDialog.State["skus"];

            var chosenSku = skus[(int)dc.ActiveDialog.State["chosenChoice"]];

            var qtyrResult = (NumberResult<int>)args;

            await dc.Context.SendActivity($"Thank you, but we are not ready to receive payment right now. Please continue to our e-commerce store to finalize your purchase.");

            var activity = MessageFactory.Attachment(
                new HeroCard(
                    title: "",
                    images: new CardImage[] { new CardImage(url: $"http://dg11.bizstreamcms.com/getmedia/a9d1644a-1cdf-467a-afae-668213d9ee7d/dg_logo.aspx") },
                    buttons: new CardAction[]
                    {
                        new CardAction(
                            title: "Continue Purchase", 
                            type: ActionTypes.OpenUrl,
                            value: $"http://dg11.bizstreamcms.com/Store/Checkout/Shopping-cart?skuid={chosenSku.SKUID}&qty={qtyrResult.Value}"
                        )
                    })
                .ToAttachment());

            await dc.Context.SendActivity(activity);

            await dc.End(dc.ActiveDialog.State);
        }

        private ChoicePromptOptions GenerateOptions(DialogContext dc)
        {
            var choices = new List<Choice>();

            Task<List<SKU>> response = Task.Run<List<SKU>>(
              async () => await _kenticoRestService.GetTopSKUsAsync(5)
            );

            List<SKU> skus = response.Result;

            dc.ActiveDialog.State.Add("skus", skus);

            skus.ForEach(x => choices.Add( 
                new Choice() {
                    Value = $"{x.SKUName.Replace("+", "and")} {x.SKUPrice:C} USD".Replace("$", ""),
                    Synonyms = new List<string> { x.SKUID.ToString() }
                })
            );

            var promptOptions = new ChoicePromptOptions()
            {
                Choices = choices,
                RetryPromptString = "Please choose a product."
            };
            
            return promptOptions;
         }
        
    }
}
