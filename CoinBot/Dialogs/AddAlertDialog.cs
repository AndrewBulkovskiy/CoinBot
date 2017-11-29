using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CoinBot.ProactiveDialogs;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class AddAlertDialog : IDialog<string>
    {
        ICurrencyService _service;
        int attempts = 3;

        public AddAlertDialog(ICurrencyService service)
        {
            _service = service;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("How much percentage of your portfolio should increase in order to receive a notification?");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            // Proactive Dialog (notification about portfolio growth)
            PortfolioNotifier.toId = message.From.Id;
            PortfolioNotifier.toName = message.From.Name;
            PortfolioNotifier.fromId = message.Recipient.Id;
            PortfolioNotifier.fromName = message.Recipient.Name;
            PortfolioNotifier.serviceUrl = message.ServiceUrl;
            PortfolioNotifier.channelId = message.ChannelId;
            PortfolioNotifier.conversationId = message.Conversation.Id;

            double percentageValue;
            bool percentageRecognised = Double.TryParse(message.Text, out percentageValue);

            if (percentageRecognised)
            {
                if (percentageValue > 0.0 && percentageValue <= 100.0)
                {
                    _service.StartTrackingPortfolio(percentageValue);
                    await context.PostAsync($"Ok, i will send a notification as soon as your portfolio will grow on {percentageValue} percents.");
                    context.Done(String.Empty);
                }
                else 
                {
                    await context.PostAsync("Are you sure that you entered correct value? (It should be aa number greater than zero and less than one hundred.");
                    context.Wait(this.MessageReceivedAsync);
                }
            }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("Sorry, it doesn't look like a number. Please try again.");
                    context.Wait(this.MessageReceivedAsync);
                }
                else
                {
                    await context.PostAsync("There seems to be a misunderstanding between us. Let`s start over our dialog.");
                    await context.PostAsync("let's return to the beginning of conversation.");
                    context.Done(String.Empty);
                }
            }

        }
    }
}