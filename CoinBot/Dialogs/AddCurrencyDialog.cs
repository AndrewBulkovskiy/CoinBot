using CoinBot.DAL.DTO;
using CoinBot.DAL.Entities;
using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
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
            var message = await result;

            // -------------- Parsing data, soon LUIS will be here :) ----------------
            string[] arr = message.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

            // TRY AND CATCH
            string currencyNameOrSymbol;
            double currencyValue;

            currencyValue = Convert.ToDouble(arr[1]);
            currencyNameOrSymbol = arr[2];
            // -------------- End of parsing data

            // Checking if user entered right curryncy name or symbol
            bool isCurrencyAvaliable = _service.IsCurrencyAvaliable(currencyNameOrSymbol);
            if (isCurrencyAvaliable)
            {
                var currency = _service.GetCurrencyByNameOrSymbol(currencyNameOrSymbol);
                currency.Multiplier = currencyValue;
                bool portfolioContainsCurrency = _service.IsCurrencyInPortfolio(currency);

                // Checking if users portfolio already contains that currency
                if (!portfolioContainsCurrency)
                {
                    if (currency != null)
                    {
                        _service.AddCurrencyToPortfolio(currency);
                        await context.PostAsync("Currency successfully added to your portfolio!");
                    }
                    if (_service.Portfolio.Count == 1) // Seems like it is the first user currency added, Congratulations :)
                    {
                        await context.PostAsync("Great! You added your first currency!");
                    }
                }
                else
                {
                    await context.PostAsync("You already have that currency in portfolio.");
                }
            }
            else
            {
                await context.PostAsync("Sorry i can't find that currency.");
            }
            context.Done(String.Empty);
        }
    }
}