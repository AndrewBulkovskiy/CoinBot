using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinBot.ProactiveDialogs;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class SetAlertDialog : IDialog<string>
    {
        ICurrencyService _service;
        int attempts = 3;

        public SetAlertDialog(ICurrencyService service)
        {
            _service = service;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (_service.Portfolio.Count > 0)
            {
                await context.PostAsync("How much percentage of your portfolio should increase in order to receive a notification?");
                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                context.Done("Your portfolio seems to be empty.\n\nPlease add at least one currency to start tracking portfolio growth.");
            }
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

            bool percentageRecognised = Decimal.TryParse(message.Text, out decimal percentageValue);

            if (percentageRecognised)
            {
                if (percentageValue > 0.0m && percentageValue <= 100.0m)
                {
                    _service.StartTrackingPortfolio(percentageValue);
                    context.Done($"Ok, i will send a notification as soon as your portfolio will grow on {percentageValue} percents.");
                }
                else
                {
                    --attempts;
                    if (attempts > 0)
                    {
                        await context.PostAsync("Please check your input (it should be a number greater than zero and less than one hundred.)");
                        context.Wait(this.MessageReceivedAsync);
                    }
                    else
                    {
                        await context.PostAsync("There seems to be a misunderstanding between us. Let's go back.");
                        context.Done(string.Empty);
                    }
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
                    context.Done("It still doesn't look like a number, let's return to the beginning of conversation.");
                }
            }
        }

    }
}