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
    public class RemoveCurrencyDialog : IDialog<string>
    {
        ICurrencyService _service;

        public RemoveCurrencyDialog(ICurrencyService service)
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

            var currency = _service.GetCurrencyByNameOrSymbol(currencyNameOrSymbol);
            bool portfolioContainsCurrency = _service.IsCurrencyInPortfolio(currency);
            if (currency == null || !portfolioContainsCurrency)
            {
                await context.PostAsync("Sorry i can't find that currency in your portfolio.");
            }
            else
            {
                _service.RemoveCurrencyFromPortfolio(currency);
                await context.PostAsync("Currency successfully deleted from your portfolio.");
            }
            context.Done(String.Empty);
        }
    }
}