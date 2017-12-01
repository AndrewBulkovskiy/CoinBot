using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class ShowPortfolioDialog : IDialog<string>
    {
        ICurrencyService _service;

        public ShowPortfolioDialog(ICurrencyService service)
        {
            _service = service;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await this.ShowPortfolio(context);
        }

        private async Task ShowPortfolio(IDialogContext context)
        {
            if (_service.Portfolio.Count > 0)
            {
                _service.RefreshPortfolio();
                string portfolioString = "**Your Portfolio:** \n\n";
                double portfolioTotalValue = 0.0;
                foreach (var item in _service.Portfolio)
                {
                    portfolioString += $"* {item} \n\n";
                    portfolioTotalValue += item.Multiplier * item.Price;
                }
                portfolioString += $"**Total value:** {portfolioTotalValue} USD";
                context.Done(portfolioString);
            }
            else
            {
                context.Done("Seems like your portfolio is empty now.\n\nYou can add first currency to it right now!");
            }
        }

    }
}