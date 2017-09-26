using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using VolebniPrukaz.DialogModels;
using Microsoft.Bot.Builder.FormFlow;

namespace VolebniPrukaz.Dialogs
{
    [Serializable]
    public class PersonalDataDialog : IDialog<PersonalDataDM>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Ahoj, jsem Volební Průkaz bot a rád ti pomůžu s vydáním volebního průkazu.");

            var personalDataFormDialog = FormDialog.FromForm(BuildPersonalDataForm, FormOptions.PromptInStart);
            context.Done(personalDataFormDialog);
        }

        private IForm<PersonalDataDM> BuildPersonalDataForm()
        {
            return new FormBuilder<PersonalDataDM>()
                .Build();
        }
    }
}