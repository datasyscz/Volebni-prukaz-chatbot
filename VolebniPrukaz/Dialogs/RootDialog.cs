using System;
using System.IO;
using System.Threading.Tasks;
using Google.Maps;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

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

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            var activity = await result as Activity;

            // calculate something for us to return
            var length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
          

           // context.Wait(MessageReceivedAsync);


            var voteForm = FormDialog.FromForm(this.BuildVoterPassForm, FormOptions.PromptInStart);

            await context.PostAsync($"Ahoj, jsem Volební Průkaz bot a rád ti pomůžu s vydáním volebního průkazu.");

            // context.Call(voteForm, Done);
            await context.PostAsync($"Napište mi prosím adresu.");
            context.Wait(GetAddress);
            //context.Done("");
        }


        private Task GetAddress(IDialogContext context, IAwaitable<IMessageActivity> awaitable)
        {

        }

        private Task Done(IDialogContext context, IAwaitable<VoterPass> awaitable)
        {
            context.Done("");
            return Task.CompletedTask;
        }

        private IForm<VoterPass> BuildVoterPassForm()
        {
            //OnCompletionAsyncDelegate<VoterPass> processHotelsSearch = async (context, state) =>
            //{
            //    await context.PostAsync(
            //        $"Ok. Searching for Hotels in {state.Destination} from {state.CheckIn.ToString("MM/dd")} to {state.CheckIn.AddDays(state.Nights).ToString("MM/dd")}...");
            //};

            return new FormBuilder<VoterPass>()
                // .Message("Připravuji pro Vás žádost o voličský průkaz...")
                .Field(nameof(VoterPass.PernamentAddress),
                        validate: async (state, response) =>
                        {
                            GoogleMapsClient googleMapClient = new GoogleMapsClient();
                            var addressFromGoogle = await googleMapClient.GetGeocodeData((string) response);
                            ValidateResult result;
                            if (addressFromGoogle.CorrectResponse)
                            {
                                result = new ValidateResult
                                {
                                    IsValid = true,
                                    Feedback = "Skvěle, adresa vypadá že je v pořádku."
                                };
                            }
                            else
                            {
                                result = new ValidateResult
                                {
                                    IsValid = false,
                                    Feedback = "Takovou adresu jsem nikde nenašel. :( zkuste ji prosim zadat ještě jednou trochu jinak."
                                };
                            }
                           
                            //await Task.Delay(1);
                            //var dirty = (Dirty)response;
                            //if (dirty == Dirty.CleanLikeJon)
                            //{
                            //    result.Feedback = "I am sorry but there are no Garmin Jokes that are that clean. Try again or you can cancel.";
                            //    result.IsValid = false;
                            //}
                            //if (dirty == Dirty.DirtyAsKen)
                            //{
                            //    result.Feedback = "Garmin Jokes aren't that dirty... but I'll see if I can get one of the better ones involving Ken";
                            //    result.IsValid = true;
                            //}
                            return result;
                        })
                .AddRemainingFields()
               // .OnCompletion(processHotelsSearch)
                .Build();
        }

        [Serializable]
        public class VoterPass
        {
            [ReplaceTag("%JMENO%")]
            public string Name { get; set; }
            [ReplaceTag("%NAROZENI%")]
            public string BirthDate { get; set; }
            [ReplaceTag("%TRVALAADRESA%")]
            public string PernamentAddress { get; set; }
            [ReplaceTag("%TELEFON%")]
            public string Phone { get; set; }


            [ReplaceTag("%URAD%")]
            public string OfficeName { get; set; }
            [ReplaceTag("%ADRESA%")]
            public string OfficeAddress { get; set; }
            [ReplaceTag("%PSC%")]
            public int OfficePostalCode { get; set; }
            [ReplaceTag("%MESTO%")]
            public string OfficeCity { get; set; }
        }
    }
}