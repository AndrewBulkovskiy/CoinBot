using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using Newtonsoft.Json;
using CoinBot.ProactiveDialogs;


namespace CoinBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        ICurrencyService _service;

        public RootDialog(ICurrencyService service)
        {
            _service = service;
        }

        public Task StartAsync(IDialogContext context)
        {
            _service.OnPortfolioGrowth += PortfolioGrowthHandler;
            //await SendWelcomeMessageAsync(context);
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string command = message.Text.ToLower();

            if (command == "res")
            {
                _service.RefreshPortfolio();
                return;
            }

            if (command.StartsWith("add"))
            {
                if (command.Contains("alert"))
                {
                    context.Call(new AddAlertDialog(_service), this.ResumeAfterAddAlertDialog);
                    return;
                }
                else
                {
                    await context.Forward(new AddCurrencyDialog(_service), this.ResumeAfterAddCurrencyDialog, message, CancellationToken.None);
                    return;
                }

            }
            if (command.StartsWith("remove"))
            {
                await context.Forward(new RemoveCurrencyDialog(_service), this.ResumeAfterRemoveCurrencyDialog, message, CancellationToken.None);
                return;
            }
            if (command.StartsWith("options"))
            {
                context.Call(new ShowOptionsDialog(_service), this.ResumeAfterShowOptionsDialog);
                return;
            }
            if (command.StartsWith("show"))
            {
                context.Call(new ShowPortfolioDialog(_service), this.ResumeAfterShowPortfoliotDialog);
                return;
            }

            await context.PostAsync($"You said: {message.Text}");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterAddCurrencyDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromAddCurrencyDialog = await result;
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterRemoveCurrencyDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromRemoveCurrencyDialog = await result;
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterAddAlertDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromAddlertDialog = await result;
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterShowPortfoliotDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromShowPortfoliotDialog = await result;
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterShowOptionsDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromShowOptionsDialog = await result;
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            await context.PostAsync("Hi! My **name** is # H1 CoinBot.\n\nYouy can add coin, remove coin, see your portfolio or add alert when to update you.");


            await context.PostAsync("You can #H2 always ask me for **help** if you need so.");
        }

        // This event handler invokes kind a Proactive Dialog
        private void PortfolioGrowthHandler()
        {
            PortfolioNotifier.Notify();
        }

    }
}