using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<string>
    {
        private const string helpWithCurrenciesOption = "How to manage my portfolio";
        private const string helpWithAlertOption = "How to turn on set up alert";
        private const string HelpWithPortfolioOprion = "How to see my portfolio";
        private const string CancelOption = "Cancel";

        public async Task StartAsync(IDialogContext context)
        {
            this.ShowOptions(context);
        }

        private void ShowOptions(IDialogContext context)
        {
            var PromptOptions = new List<string>() { helpWithCurrenciesOption, helpWithAlertOption, HelpWithPortfolioOprion, CancelOption };
            PromptDialog.Choice(context, this.OnOptionSelected, PromptOptions, "How can i help you?", "This is not a valid option. Please try again:", 1);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case helpWithCurrenciesOption:
                        string helpWithCurrencies = "In order to add a currency to your portfolio type something like *Add 1.0 BTC or Bitcoin*. \n\n If you want to remove currency from portfolio type *Remove 1.0 BTC*";
                        await context.PostAsync(helpWithCurrencies);
                        context.Done(string.Empty);
                        break;
                    case helpWithAlertOption:
                        string helpWithAlert = "I can notify you when total price of your portfolio will grow on certain percentage. To set reminder write *alert* or *set notification*.";
                        await context.PostAsync(helpWithAlert);
                        context.Done(string.Empty);
                        break;
                    case HelpWithPortfolioOprion:
                        string helpWithPortfolio = "Type something like  like *portfolio* or *show portfolio* to see your portfolio.";
                        await context.PostAsync(helpWithPortfolio);
                        context.Done(string.Empty);
                        break;
                    case CancelOption:
                        context.Done(string.Empty);
                        break;
                }
            }
            catch (TooManyAttemptsException ex) // .PostAsync instead of.Fail because this is scorable dialog
            {
                await context.PostAsync("Looks like you don`t need help anymore. Let's go back to the begining of our dialog.");
                context.Done(string.Empty);
            }
        }
    }
}