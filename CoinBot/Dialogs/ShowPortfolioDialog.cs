using CoinBot.DAL.DTO;
using CoinBot.DAL.Entities;
using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
                string res = "";
                foreach (var item in _service.Portfolio)
                {
                    res += item.ToString() + "\n\n";
                }
                await context.PostAsync($"Portolio:\n\n {res}");
            }
            else
            {
                await context.PostAsync("Seems like your portfolio is empty now.\n\nYpu can add first currency to it right now!");
            }
            context.Done(String.Empty);
        }

    }
}