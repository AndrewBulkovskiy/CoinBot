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
        private const string YesOption = "Yes";
        private const string NoOption = "No";

        public SetAlertDialog(ICurrencyService service)
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

            double percentageValue = 0.0;
            bool percentageRecognised = Double.TryParse(message.Text, out percentageValue);

            if (percentageRecognised)
            {
                if (percentageValue > 0.0 && percentageValue <= 100.0)
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
                        attempts = 2;
                        await context.PostAsync("There seems to be a misunderstanding between us.");
                        var PromptOptions = new List<string>() { YesOption, NoOption };
                        PromptDialog.Choice(context, this.OnOptionSelected, PromptOptions, "Let's return to the beginning of conversation?", "Let's return to the beginning of conversation?", 1);
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
                    context.Done("Let's return to the beginning of conversation.");
                }
            }
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case YesOption:
                        context.Done(string.Empty);
                        break;
                    case NoOption:
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Fail(new TooManyAttemptsException("It looks like a little misunderstanding. Let's start over."));
            }

        }

    }
}