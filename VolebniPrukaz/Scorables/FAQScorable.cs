using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    return "Na nápovědě se pracuje. Prosíme o strpení.";

                if (kontaktMatches.Any(a => a.ToLower() == msg.Text.ToLower()))
                    return "Na kontaktních údajích se pracuje. Prosíme o strpení.";
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
    }
}