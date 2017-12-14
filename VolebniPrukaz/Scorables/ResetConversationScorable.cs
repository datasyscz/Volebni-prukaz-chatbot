using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace VolebniPrukaz.Scorables
{
    public sealed class ResetConversationScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialog<object> _rootDialog;
        private readonly Func<IActivity, CancellationToken, string> _restartOn;
        private readonly IDialogTask task;

        public ResetConversationScorable(IDialogTask task, IDialog<object> rootDialog, Func<IActivity, CancellationToken, string> restartOn)
        {
            _rootDialog = rootDialog;
            _restartOn = restartOn;
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            return _restartOn(activity, token);
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
            this.task.Reset();
            var commonResponsesDialog = _rootDialog;
            var interruption = commonResponsesDialog.Void<object, IMessageActivity>();
            await this.task.Forward(interruption, null, null, token);
            //await this.task.PollAsync(token);

        }
        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}