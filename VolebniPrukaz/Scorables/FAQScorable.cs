using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace VolebniPrukaz.Scorables
{
    public sealed class FAQScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask task;

        public FAQScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var msg = (Activity)activity;

            if (msg != null && !string.IsNullOrEmpty(msg.Text))
            {
                string[] helpMatches = new[] { "HELP_PAYLOAD", "pomoc", "help", "nechápu", "nápověda", "napoveda" };
                string[] kontaktMatches = new[] { "CONTACT_PAYLOAD", "contact", "kontakt", "kontaktní údaje" };

                if (helpMatches.Any(a => a.ToLower() == msg.Text.ToLower()))
                    return GetHelpText();

                if (kontaktMatches.Any(a => a.ToLower() == msg.Text.ToLower()))
                    return GetContactText();
            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var settingsDialog = Chain.Return(state).PostToUser();
            var interruption = settingsDialog.Void<object, IMessageActivity>();
            this.task.Call(interruption, null);
            await this.task.PollAsync(token);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected string GetContactText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Chatbota “Volební průkaz” pro vás ve spolupráci s Hlídačem státu vyvinula společnost Datasys.\n\n");
            sb.AppendLine("V případě dotazů se na nás neváhejte obracet.\n\n");
            sb.AppendLine("Datasys - +420 225 308 111,  datasys@datasys.cz, www.datasys.cz\n\n");
            sb.AppendLine("Hlídač státu - Michal Bláha - +420 777 737 811, michal@michalblaha.cz, www.hlidacstatu.cz\n\n");
            sb.AppendLine("Děkujeme!");
            return sb.ToString();
        }

        protected string GetHelpText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Volit prezidenta můžete v ČR všude, v zahraničí na české ambasádě. Potřebujete k tomu voličský průkaz. Ten získáte, pokud na úřad vašeho trvalého bydliště doručíte do 5.1. žádost, kterou si zde vygenerujete. Pošlete ji tam datovkou anebo poštou s úředně ověřeným podpisem. Průkaz si potom vyzvednete osobně, úřad vám ji zašle na určenou adresu anebo Vám jej vyzvedne zmocněná osoba. Do 10.1. si ho na úřadě můžete vyzvednou i osobně.");
            return sb.ToString();
        }
    }
}