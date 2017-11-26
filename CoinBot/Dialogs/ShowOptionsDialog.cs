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
    public class ShowOptionsDialog : IDialog<string>
    {
        private const string MyPortfolioOption = "My portfolio";
        private const string AddCurrencyOption = "Add currency";
        private const string RemoveCurrencyOption = "Remove currency";
        private const string AlertOption = "Add alert";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            this.ShowOptions(context);
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { MyPortfolioOption, AlertOption }, "options", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case MyPortfolioOption:
                        context.Call(new ShowPortfolioDialog(), this.ResumeAfterOptionDialog);
                        break;
                    case AddCurrencyOption:
                        context.Call(new AddCurrencyDialog(), this.ResumeAfterOptionDialog);
                        break;
                    case RemoveCurrencyOption:
                        context.Call(new RemoveCurrencyDialog(), this.ResumeAfterOptionDialog);
                        break;
                    case AlertOption:
                        context.Call(new AddAlertDialog(), this.ResumeAfterOptionDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            var resultFromShowOptionsDialog = await result;
            await context.PostAsync($"OptionDialog: : {resultFromShowOptionsDialog}");
            //context.Wait(this.MessageReceivedAsync); // якщо ця опція, а не нижча, то ми залишаємось на рівні опцій і прийматимемо повідомлення
            //context.Done(resultFromShowOptionsDialog);
            ShowOptions(context); // отак взагалі безвихідь - завжди опції приходять по завершенні роботи
        }
    }
}