using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await ShowIntroMessageAsync(context); // тимчасово тут
            var message = await result;

            string command = message.Text.ToLower();
            switch (command)
            {
                case "add":
                    await context.Forward(new AddCurrencyDialog(), this.ResumeAfterAddCurrencyDialog, message, CancellationToken.None);
                    break;
                case "options":
                    await context.Forward(new ShowOptionsDialog(), this.ResumeAfterShowOptionsDialog, message, CancellationToken.None);
                    break;
                default:
                    await context.PostAsync($"You said: {message.Text}");
                    break;
            }
        }

        private async Task ResumeAfterAddCurrencyDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromAddCurrencyDialog = await result;

            await context.PostAsync($"AddCurrencyDialog: {resultFromAddCurrencyDialog}");

            // Again, wait for the next message from the user.
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterShowOptionsDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromShowOptionsDialog = await result;

            await context.PostAsync($"ShowOptionsDialog: {resultFromShowOptionsDialog}");

            // Again, wait for the next message from the user.
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ShowIntroMessageAsync(IDialogContext context)
        {

            // add nice formating here
            await context.PostAsync("Hi! My name is CoinBot.\n\nYouy can add coin, remove coin, see your portfolio or add alert when to update you.");
            await context.PostAsync("In order to add coin please use comand \"Add 1.0 BTC\"");
            //context.Wait(MessageReceivedAsync);
        }
    }
}