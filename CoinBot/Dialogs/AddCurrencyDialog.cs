using CoinBot.DAL.DTO;
using CoinBot.DAL.Entities;
using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class AddCurrencyDialog : IDialog<string>
    {
        ICurrencyService _service;
        LuisResult _luisResult;
        int attempts = 3;
        private const string RemoveCurrencyOption = "Remove currency";
        private const string ShowPortfolioOption = "See your portfolio";
        private const string SetAlertOption = "Set alert";

        public AddCurrencyDialog(ICurrencyService service, LuisResult luisResult)
        {
            _service = service;
            _luisResult = luisResult;
        }

        public AddCurrencyDialog(ICurrencyService service)
        {
            _service = service;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            string currencyNameOrSymbol = String.Empty;
            double currencyMultiplier = 1.0;

            // Message forwarded from luis or not
            if (_luisResult == null)
            {
                var message = await result;
                string[] entities = message.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                // If input contains currencyMultiplier and currencyNameOrSymbol
                if (entities.Length < 2)
                {
                    context.Fail(new ArgumentException("Sorry i can't recognize value and currency (e.g. 1.0 BTC)."));
                }
                else
                {
                    double.TryParse(entities[0], out currencyMultiplier);
                    currencyNameOrSymbol = entities[1];
                }
            }
            else
            {
                foreach (var entity in _luisResult.Entities)
                {
                    if (entity.Type == "Currency.Multiplier" && entity.Entity != null)
                        double.TryParse(entity.Entity.Replace(" ", string.Empty), out currencyMultiplier);

                    if (entity.Type == "Currency.Symbol" && entity.Entity != null)
                        currencyNameOrSymbol = entity.Entity;

                    if (entity.Type == "Currency.Name" && entity.Entity != null)
                        currencyNameOrSymbol = entity.Entity;
                }
            }

            // Final input checking
            if (currencyMultiplier > 0.0 && currencyNameOrSymbol != null && !string.IsNullOrWhiteSpace(currencyNameOrSymbol))
            {
                // Checking if user entered right curryncy name or symbol
                bool isCurrencyAvaliable = _service.IsCurrencyAvaliable(currencyNameOrSymbol);

                if (isCurrencyAvaliable)
                {
                    var currency = _service.GetCurrencyByNameOrSymbol(currencyNameOrSymbol);

                    // Setting custom multiplier
                    currency.Multiplier = currencyMultiplier;

                    // Checking if users portfolio already contains that currency
                    bool portfolioContainsCurrency = _service.IsCurrencyInPortfolio(currency);
                    if (!portfolioContainsCurrency)
                    {
                        if (currency != null)
                        {
                            _service.AddCurrencyToPortfolio(currency);
                            await context.PostAsync("Currency has been successfully added to your portfolio");
                        }
                        if (_service.Portfolio.Count == 1) // Seems like it is the first user currency added, Congratulations :)
                        {
                            await context.PostAsync("Great! You added your first currency!");
                            var PromptOptions = new List<string>() { RemoveCurrencyOption, ShowPortfolioOption, SetAlertOption };
                            PromptDialog.Choice(context, this.OnOptionSelected, PromptOptions, "Ok, now you can: ", "Please select one of the options below: ", 1);
                        }
                        else
                        {
                            context.Done(string.Empty);
                        }
                    }
                    else
                    {
                        context.Fail(new InvalidOperationException("Your portfolio already contains that currency."));
                    }
                }
                else
                {
                    --attempts;
                    if (attempts > 0)
                    {
                        await context.PostAsync("Sorry i can't find that currency. Please check your input and try again.");
                        context.Wait(this.MessageReceivedAsync);
                    }
                    else
                    {
                        context.Fail(new InvalidOperationException("Sorry i can't find that currency. You can check if currency is available [here](https://coinmarketcap.com/)."));
                    }
                }
            }
            else
            {
                context.Fail(new ArgumentException("Sorry i can't recognize value and currency (command should look like *Add 1.0 BTC*)."));
            }

        }

        private async Task ResumeArfetStringResultDialog(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var resultFromDialog = await result;
                if (resultFromDialog != null && !string.IsNullOrWhiteSpace(resultFromDialog))
                {
                    await context.PostAsync(resultFromDialog);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An unknown error occured. Please try again.");
            }
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case RemoveCurrencyOption:
                        context.Call(new RemoveCurrencyDialog(_service), this.ResumeArfetStringResultDialog);
                        break;
                    case ShowPortfolioOption:
                        context.Call(new ShowPortfolioDialog(_service), this.ResumeArfetStringResultDialog);
                        break;
                    case SetAlertOption:
                        context.Call(new SetAlertDialog(_service), this.ResumeArfetStringResultDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Fail(new TooManyAttemptsException("It looks like a little misunderstanding. Lets move to the begining of conversation."));
            }
        }

    }
}