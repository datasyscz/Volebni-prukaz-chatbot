using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace VolebniPrukaz.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        public class UserElectionsData
        {
            [ReplaceTag("%JMENO%")]
            public string Name { get; set; }

            [ReplaceTag("%ADRESA%")]
            public string Address { get; set; }

            public string FieldForDifferentReason { get; set; }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var data = new UserElectionsData()
            {
                Name = "David",
                Address = "Král Jelimán 223",
                FieldForDifferentReason = "foo"
            };


            var tempDoc = new TemplateDocument(new FileInfo("./vzor-zadosti-o-vp (4).docx"));
            var replacedDoc = await tempDoc.ReplaceAllTagsAsync(data);

            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }
    }
}