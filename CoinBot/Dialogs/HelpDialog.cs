using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<string>
    {
        private const string helpWithCurrenciesOption = "How to manage my portfolio";
        private const string helpWithAlertOption = "How to set alert when portfolio growth";
        private const string HelpWithPortfolioOprion = "How to see my portfolio";
        private const string CancelOption = "Cancel";

        public async Task StartAsync(IDialogContext context)
        {
            this.ShowOptions(context);
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { helpWithCurrenciesOption, helpWithAlertOption, HelpWithPortfolioOprion, CancelOption }, "What can i help you with?", "This is not a valid option", 1);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case helpWithCurrenciesOption:
                        await context.PostAsync("help tip");
                        context.Done<object>(null);
                        break;
                    case helpWithAlertOption:
                        await context.PostAsync("help tip");
                        context.Done<object>(null);
                        break;
                    case HelpWithPortfolioOprion:
                        await context.PostAsync("help tip");
                        context.Done<object>(null);
                        break;
                    case CancelOption:
                        context.Done<object>(null);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                // i see you have some problems
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");
                context.Done(String.Empty);
            }
        }
    }
}