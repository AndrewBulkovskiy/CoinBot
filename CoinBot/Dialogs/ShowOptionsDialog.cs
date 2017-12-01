using CoinBot.DAL.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinBot.Dialogs
{
    [Serializable]
    public class ShowOptionsDialog : IDialog<string>
    {
        ICurrencyService _service;

        public ShowOptionsDialog(ICurrencyService service)
        {
            _service = service;
        }

        private const string MyPortfolioOption = "See portfolio";
        private const string AddCurrencyOption = "Add currency";
        private const string RemoveCurrencyOption = "Remove currency";
        private const string AlertOption = "Set alert";
        private const string HelpOption = "Get help";
        private const string CancelOption = "Cancel";

        public async Task StartAsync(IDialogContext context)
        {
            this.ShowOptions(context);
        }

        private void ShowOptions(IDialogContext context)
        {
            var PromptOptions = new List<string>() { MyPortfolioOption, AddCurrencyOption, RemoveCurrencyOption, AlertOption, HelpOption, CancelOption };
            PromptDialog.Choice(context, this.OnOptionSelected, PromptOptions, "What would you like to do?", "This is not a valid option, please try again.", 1);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case MyPortfolioOption:
                        context.Call(new ShowPortfolioDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case AddCurrencyOption:
                        await context.PostAsync("Plesa type currency and value (e.g. *1.0 BTC*)");
                        context.Call(new AddCurrencyDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case RemoveCurrencyOption:
                        await context.PostAsync("Plesa type currency and value (e.g. *1.0 BTC*)");
                        context.Call(new RemoveCurrencyDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case AlertOption:
                        context.Call(new SetAlertDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case HelpOption:
                        context.Call(new HelpDialog(), this.ResumeAfterOptionDialog);
                        break;
                    case CancelOption:
                        context.Done(string.Empty);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Seems like you can`t decide. Let's go back."));
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<string> result)
        {
            var resultFromShowOptionsDialog = await result;
            context.Done(resultFromShowOptionsDialog);
        }
    }
}