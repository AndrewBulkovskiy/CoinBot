using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class RemoveCurrencyDialog : IDialog<string>
    {
        ICurrencyService _service;
        LuisResult _luisResult;

        public RemoveCurrencyDialog(ICurrencyService service, LuisResult luisResult)
        {
            _service = service;
            _luisResult = luisResult;
        }

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
            string currencyNameOrSymbol = string.Empty;
            decimal currencyMultiplier = 1.0m;

            // Message forwarded from luis or not
            if (_luisResult == null)
            {
                var message = await result;
                string[] entities = message.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                // If input contains currencyMultiplier and currencyNameOrSymbol
                if (entities.Length < 2)
                {
                    context.Fail(new ArgumentException("Sorry i can't recognize value and currency (command should look like *Remove 1.0 BTC*)."));
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
                        decimal.TryParse(entity.Entity.Replace(" ", String.Empty), out currencyMultiplier);

                    if (entity.Type == "Currency.Symbol" && entity.Entity != null)
                        currencyNameOrSymbol = entity.Entity;

                    if (entity.Type == "Currency.Name" && entity.Entity != null)
                        currencyNameOrSymbol = entity.Entity;
                }
            }

            // Final input checking
            if (currencyMultiplier > 0.0m && currencyNameOrSymbol != null && !string.IsNullOrWhiteSpace(currencyNameOrSymbol))
            {
                var currency = _service.GetCurrencyByNameOrSymbol(currencyNameOrSymbol);

                if (currency != null)
                {
                    currency.Multiplier = currencyMultiplier;
                    bool portfolioContainsCurrency = _service.IsCurrencyInPortfolio(currency);
                    if (portfolioContainsCurrency)
                    {
                        _service.RemoveCurrencyFromPortfolio(currency);
                        context.Done("Portfolio updated!.");
                    }
                    else
                    {
                        context.Fail(new ArgumentException("Sorry i can't find that currency in your portfolio."));
                    }
                }
            }
            else
            {
                context.Fail(new ArgumentException("Sorry i can't recognize value and currency (e.g. 1.0 BTC)."));
            }

        }

    }
}