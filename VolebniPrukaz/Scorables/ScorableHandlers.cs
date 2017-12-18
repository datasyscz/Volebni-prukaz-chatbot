using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace VolebniPrukaz.Scorables
{
    public class ScorableHandlers : Module
    {
        private readonly IDialog<object> _rootDialog;
        private readonly Func<IActivity, CancellationToken, string> _restartOn;

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder
                .Register(c => new ResetConversationScorable(c.Resolve<IDialogTask>(), _rootDialog, _restartOn))
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();

            builder
                .Register(c => new FAQScorable(c.Resolve<IDialogTask>()))
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
        }

        public ScorableHandlers(IDialog<object> rootDialog, Func<IActivity, CancellationToken, string> restartOn)
        {
            _rootDialog = rootDialog;
            _restartOn = restartOn;
        }

        public static void RegisterModule(IDialog<object> rootDialog, Func<IActivity, CancellationToken, string> restartOn)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ReflectionSurrogateModule());
            builder.RegisterModule(new ScorableHandlers(rootDialog, restartOn));
            builder.Update(Conversation.Container);
        }

        public static void RegisterModule(IDialog<object> rootDialog)
        {
            ScorableHandlers.RegisterModule(
                rootDialog,
                (activity, token) =>
                {
                    var msg = (Activity)activity;

                    if (msg != null && !string.IsNullOrEmpty(msg.Text))
                    {
                        string[] matches = new[] { "GET_STARTED_PAYLOAD", "STARTED_CON", "restart", "reset", "RESET_PAYLOAD" };

                        if (matches.Any(a => a.ToLower() == msg.Text.ToLower()))
                            return msg.Text;
                    }

                    return null;
                });
        }
    }
}