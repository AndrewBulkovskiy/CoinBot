using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using CoinBot.DAL.Interfaces;
using CoinBot.ProactiveDialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;

namespace CoinBot.Dialogs
{
    [LuisModel("7129a745-5a5a-4920-bc65-3ba46fa9373b", "aa344ddbf6ad4fb0bbbde05d6625b22b", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        ICurrencyService _service;
        private bool isFirstMessage = true;
        int attempts = 3;
        private const string YesOption = "Yes";
        private const string NoOption = "No";

        public RootDialog(ICurrencyService service)
        {
            _service = service;
            _service.OnPortfolioGrowth += PortfolioGrowthHandler;
        }

        [LuisIntent("Currency.Add")]
        public async Task AddCurrency(IDialogContext context, LuisResult result)
        {
            await context.Forward(new AddCurrencyDialog(_service, result), this.ResumeAfterStringResultDialog, null, CancellationToken.None);
        }

        [LuisIntent("Currency.Remove")]
        public async Task RemoveCurrency(IDialogContext context, LuisResult result)
        {
            await context.Forward(new RemoveCurrencyDialog(_service, result), this.ResumeAfterStringResultDialog, null, CancellationToken.None);
        }

        [LuisIntent("ShowPortfolio")]
        public async Task ShowPortfolio(IDialogContext context, LuisResult result)
        {
            context.Call(new ShowPortfolioDialog(_service), this.ResumeAfterStringResultDialog);
        }

        [LuisIntent("SetAlert")]
        public async Task SetAlert(IDialogContext context, LuisResult result)
        {
            context.Call(new SetAlertDialog(_service), this.ResumeAfterStringResultDialog);
        }

        [LuisIntent("ShowOptions")]
        public async Task ShowOptions(IDialogContext context, LuisResult result)
        {
            context.Call(new ShowOptionsDialog(_service), this.ResumeAfterStringResultDialog);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            if (isFirstMessage)
            {
                await SendWelcomeMessageAsync(context);
            }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("Sorry i do not understand you.");
                }
                else
                {
                    attempts = 3;
                    var PromptOptions = new List<string>() { YesOption, NoOption };
                    PromptDialog.Choice(context, this.OnOptionSelected, PromptOptions, "Maybye want to see all available options?", "Please select one of the options below: ", 1);
                }
            }

            isFirstMessage = false;
        }

        private async Task ResumeAfterStringResultDialog(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var resultFromDialog = await result;
                if (resultFromDialog != null && !string.IsNullOrWhiteSpace(resultFromDialog))
                {
                    await context.PostAsync(resultFromDialog);
                }
            }
            catch (Exception ex) // .PostAsync instead of .Fail because we are in RootDialog - the bottom of the stack and nobody can handle this exception.
            {
                await context.PostAsync(ex.Message);
            }
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            string welcomeMessage = "Hi! My name is **CoinBot**.\n\n I am here to help you with financial portfolio management. \n\n";
            welcomeMessage += "You can *add/remove* coin to your portfolio, ask me to *show* it or *set alert* to notify you when portfolio will grow on certain percentage. \n\n";
            await context.PostAsync(welcomeMessage);
            string tipMessage = "And of course you can ask me for *help* at any time you need it ;)";
            await context.PostAsync(tipMessage);
            string addCoinMessage = "In order to add coin please use command like *Add 1.0 BTC* or *Add 1.0 Bitcoin*";
            await context.PostAsync(addCoinMessage);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case YesOption:
                        context.Call(new ShowOptionsDialog(_service), this.ResumeAfterStringResultDialog);
                        break;
                    case NoOption:
                        break;
                }
            }
            catch (TooManyAttemptsException ex) // again .PostAsync instead of .Fail because we are in RootDialog - the bottom of the stack and nobody can handle this exception.
            {
                await context.PostAsync("It looks like a little misunderstanding. Lets move to the begining of conversation.");
            }
        }

        // This event handler invokes kind a Proactive Dialog
        private void PortfolioGrowthHandler()
        {
            PortfolioNotifier.NotifyPortfolioGrowth();
        }

    }
}