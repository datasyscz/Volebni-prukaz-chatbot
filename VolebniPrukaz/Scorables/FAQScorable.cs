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
            sb.AppendLine("Voličský průkaz je důležitý pro všechny, kteří vědí, že v době prezidentských voleb (tedy 12. a 13. ledna 2018) " +
                "nebudou v místě svého trvalého bydliště. S platným voličským průkazem lze volit v jakémkoliv městě v Česku anebo v zahraničí na české ambasádě. " +
                "S naším chatbotem vytvoříte žádost o volební průkaz během pár minut. Stačí zadat Vaše osobní údaje a chatbot Vám žádost automaticky vygeneruje.\n"
                );
            sb.AppendLine("CO S ŽÁDOSTÍ O VOLIČSKÝ PRŮKAZ UDĚLAT?\n");
            sb.AppendLine("Je nutné ji nejpozději do 5.1.2018 doručit na úřad v místě Vašeho trvalého bydliště. " +
                "Můžete ji tam zaslat buď datovou schránkou anebo ji vytisknout, podepsat, podpis nechat úředně ověřit " +
                "(např. na úřadech a v CzechPOINTech České pošty - zdarma) a odeslat poštou.\n"
                );
            sb.AppendLine("Pro voličský průkaz si pak na úřad můžete dojít osobně anebo si ho nechat zaslat na předem určenou adresu. " +
                "Můžete také pověřit člena rodiny či známého, aby průkaz vyzvedl za Vás. " +
                "V tom případě ovšem musíte tuto osobu vybavit plnou mocí s Vaším úředně ověřeným podpisem. " +
                "I s vygenerováním této plné moci Vám náš chatbot ovšem rád pomůže.\n"
                );
            sb.AppendLine("Do 10. ledna můžete o svůj voličský průkaz na příslušném úřadě požádat také osobně. " +
                "V tom případě nemusíte sepisovat papírovou žádost, úředník jen zkontroluje Vaši totožnost, " +
                "požadavek si zaznamená a domluví se s Vámi na způsobu předání průkazu."
                );
            return sb.ToString();
        }
    }
}