﻿using CoinBot.DAL.DTO;
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
        static bool isFirstCurrency = true;

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
            decimal currencyMultiplier = 1.0m;

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
                    decimal.TryParse(entities[0], out currencyMultiplier);
                    currencyNameOrSymbol = entities[1];
                }
            }
            else
            {
                foreach (var entity in _luisResult.Entities)
                {
                    if (entity.Type == "Currency.Multiplier" && entity.Entity != null)
                        decimal.TryParse(entity.Entity.Replace(" ", string.Empty), out currencyMultiplier);

                    if (entity.Type == "Currency.Symbol" && entity.Entity != null)
                        currencyNameOrSymbol = entity.Entity;

                    if (entity.Type == "Currency.Name" && entity.Entity != null)
                        currencyNameOrSymbol = entity.Entity;
                }
            }

            // Final input checking
            if (currencyMultiplier > 0.0m && currencyNameOrSymbol != null && !string.IsNullOrWhiteSpace(currencyNameOrSymbol))
            {
                // Checking if user entered right curryncy name or symbol
                bool isCurrencyAvaliable = _service.IsCurrencyAvaliable(currencyNameOrSymbol);
                var currency = _service.GetCurrencyByNameOrSymbol(currencyNameOrSymbol);

                if (isCurrencyAvaliable && currency != null)
                {
                    // Setting custom multiplier
                    currency.Multiplier = currencyMultiplier;

                    _service.AddCurrencyToPortfolio(currency);
                    await context.PostAsync("Portfolio successfully updated!");

                    if (isFirstCurrency) // Seems like it is the first user currency added, Congratulations :)
                    {
                        await context.PostAsync("Great! You added your first currency!");
                        var optionsList = new List<string>() { RemoveCurrencyOption, ShowPortfolioOption, SetAlertOption };
                        var options = new PromptOptions<string>("Ok, now you can: ",
                            "Please select one of the options below:",
                            "It looks like a little misunderstanding. Lets move to the begining of conversation.",
                            optionsList,
                            1);
                        PromptDialog.Choice(context, this.OnOptionSelected, options);
                        isFirstCurrency = false;
                    }
                    else
                    {
                        context.Done(string.Empty);
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
            var resultFromDialog = await result;
            context.Done(resultFromDialog);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case RemoveCurrencyOption:
                        await context.PostAsync("Plesa type currency and value (e.g. *1.0 BTC*)");
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
                // TooManyAttemptsException message still will be displayed
                context.Done(string.Empty);
            }
        }

    }
}