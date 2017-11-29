using CoinBot.DAL.Interfaces;
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
        ICurrencyService _service;

        public ShowOptionsDialog(ICurrencyService service)
        {
            _service = service;
        }

        private const string MyPortfolioOption = "My portfolio";
        private const string AddCurrencyOption = "Add currency";
        private const string RemoveCurrencyOption = "Remove currency";
        private const string AlertOption = "Add alert";
        private const string CancelOption = "Cancel";

        public async Task StartAsync(IDialogContext context)
        {
            this.ShowOptions(context);
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { MyPortfolioOption, AddCurrencyOption, RemoveCurrencyOption, AlertOption, CancelOption }, "What do you want to do?", "This is not a valid option, please try again", 1);
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
                        context.Call(new AddCurrencyDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case RemoveCurrencyOption:
                        context.Call(new RemoveCurrencyDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case AlertOption:
                        context.Call(new AddAlertDialog(_service), this.ResumeAfterOptionDialog);
                        break;
                    case CancelOption:
                        context.Done(String.Empty);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync("Looks like you can`t decide. Let's start our conversation from the beginning.");
                context.Done(String.Empty);
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            var resultFromShowOptionsDialog = await result;
            context.Done(resultFromShowOptionsDialog);
        }
    }
}