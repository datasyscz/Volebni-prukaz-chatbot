using System;
using System.IO;
using System.Threading.Tasks;
using Google.Maps;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using VolebniPrukaz.DialogModels;
using VolebniPrukaz.Forms;
using System.Web;
using System.Web.Hosting;

namespace VolebniPrukaz.Dialogs
{
    public class RootDialog
    {
        protected static PersonalDataDM personalDataDM;
        protected static AddressDM addressDM;

        public static IDialog<object> StartWithHelloChain()
        {
            return Chain.Return("Ahoj, jsem Volební Průkaz bot a rád Vám pomůžu s vydáním volebního průkazu.")
                .PostToUser()
                .ContinueWith(async (ctx, res) =>
                {
                    await res;
                    return MainChain();
                });
        }

        public static IDialog<object> StartOverChain()
        {
            return Chain.Return(MainChain())
                .PostToUser();
        }

        public static IDialog<object> MainChain()
        {
            return Chain.Return("Pojďme na to.")
                .PostToUser()
                .WaitToBot()
                .ContinueWith(async (ctx, res) => {
                    await res;
                    return Chain.From(() => FormDialog.FromForm(() => PersonalDataForm.GetPersonalDataForm(), FormOptions.None));
                })
                .ContinueWith<PersonalDataDM, AddressDM>(async (ctx, res) => {
                    personalDataDM = await res;
                    ctx.ConversationData.SetValue(nameof(personalDataDM), personalDataDM);
                    return new AddressDialog("Napište mi prosím adresu Vašeho trvalého bydliště.");
                })
                .ContinueWith<AddressDM, object>(async (ctx, res) => {
                    var addressDM = await res;
                    personalDataDM = ctx.ConversationData.GetValue<PersonalDataDM>(nameof(personalDataDM));
                    var voterPassServiceUri = GetVoterPassUri(addressDM, personalDataDM);
                    return Chain.Return(voterPassServiceUri.ToString()).PostToUser();
                })
                .ContinueWith(async (ctx, res) => {
                     await res;
                     return Chain.From(() => new PromptDialog.PromptConfirm("Chcete vygenerovat nový volební průkaz?", "Bohužel nerozumím.", 3))
                     .ContinueWith(async (ctx2, res2) => {
                         var isConfirmed = await res2;

                         if (isConfirmed)
                         {
                             return StartOverChain();
                         }
                         else
                             return Chain.Return("Rád jsem pomohl a děkuji.");
                     });
                });
        }

        private static Uri GetVoterPassUri(AddressDM address, PersonalDataDM personalData)
        {
            string baseUrl = HttpContext.Current.Request.Url.LocalPath;

            string controllerPath = "/api/file";

            string query = $"?name={personalData.Name}";
            query += $"&birthDate={personalData.BirthDate}";
            query += $"&phone={personalData.Phone}";
            query += $"&permanentAddress={address.ToAddressString()}";
            query += $"&officeName=officeName";
            query += $"&officeAddress=officeAddress";
            query += $"&officePostalCode=officePostalCode";
            query += $"&officeCity=officeCity";

            return new Uri(baseUrl + controllerPath + query);
        }
    }

    //[Serializable]
    //public class RootDialog : IDialog<object>
    //{
    //    public Task StartAsync(IDialogContext context)
    //    {
    //        context.Wait(MessageReceivedAsync);

    //        return Task.CompletedTask;
    //    }

    //    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
    //    {
    //        var activity = await result as Activity;

    //        await context.PostAsync($"Ahoj, jsem Volební Průkaz bot a rád ti pomůžu s vydáním volebního průkazu.");

    //        context.Call(new PersonalDataDialog(), GetAddress);
    //    }
    //    private Task GetAddress(IDialogContext context, IAwaitable<PersonalDataDM> awaitable)
    //    {
    //        context.Call(new AddressDialog("Napište mi prosím adresu."), Done);
    //        return Task.CompletedTask;
    //    }

    //    private Task Done(IDialogContext context, IAwaitable<AddressDM> awaitable)
    //    {
    //        context.EndConversation("0");
    //        return Task.CompletedTask;
    //    }

    //    private IForm<VoterPass> BuildVoterPassForm()
    //    {
    //        //OnCompletionAsyncDelegate<VoterPass> processHotelsSearch = async (context, state) =>
    //        //{
    //        //    await context.PostAsync(
    //        //        $"Ok. Searching for Hotels in {state.Destination} from {state.CheckIn.ToString("MM/dd")} to {state.CheckIn.AddDays(state.Nights).ToString("MM/dd")}...");
    //        //};

    //        return new FormBuilder<VoterPass>()
    //            // .Message("Připravuji pro Vás žádost o voličský průkaz...")
    //            .Field(nameof(VoterPass.PernamentAddress),
    //                    validate: async (state, response) =>
    //                    {
    //                        GoogleMapsClient googleMapClient = new GoogleMapsClient();
    //                        var addressFromGoogle = await googleMapClient.GetGeocodeData((string) response);
    //                        ValidateResult result;
    //                        if (addressFromGoogle.CorrectResponse)
    //                        {
    //                            result = new ValidateResult
    //                            {
    //                                IsValid = true,
    //                                Feedback = "Skvěle, adresa vypadá že je v pořádku."
    //                            };
    //                        }
    //                        else
    //                        {
    //                            result = new ValidateResult
    //                            {
    //                                IsValid = false,
    //                                Feedback = "Takovou adresu jsem nikde nenašel. :( zkuste ji prosim zadat ještě jednou trochu jinak."
    //                            };
    //                        }

    //                        //await Task.Delay(1);
    //                        //var dirty = (Dirty)response;
    //                        //if (dirty == Dirty.CleanLikeJon)
    //                        //{
    //                        //    result.Feedback = "I am sorry but there are no Garmin Jokes that are that clean. Try again or you can cancel.";
    //                        //    result.IsValid = false;
    //                        //}
    //                        //if (dirty == Dirty.DirtyAsKen)
    //                        //{
    //                        //    result.Feedback = "Garmin Jokes aren't that dirty... but I'll see if I can get one of the better ones involving Ken";
    //                        //    result.IsValid = true;
    //                        //}
    //                        return result;
    //                    })
    //            .AddRemainingFields()
    //           // .OnCompletion(processHotelsSearch)
    //            .Build();
    //    }

    //    [Serializable]
    //    public class VoterPass
    //    {
    //        [ReplaceTag("%JMENO%")]
    //        public string Name { get; set; }
    //        [ReplaceTag("%NAROZENI%")]
    //        public string BirthDate { get; set; }
    //        [ReplaceTag("%TRVALAADRESA%")]
    //        public string PernamentAddress { get; set; }
    //        [ReplaceTag("%TELEFON%")]
    //        public string Phone { get; set; }


    //        [ReplaceTag("%URAD%")]
    //        public string OfficeName { get; set; }
    //        [ReplaceTag("%ADRESA%")]
    //        public string OfficeAddress { get; set; }
    //        [ReplaceTag("%PSC%")]
    //        public int OfficePostalCode { get; set; }
    //        [ReplaceTag("%MESTO%")]
    //        public string OfficeCity { get; set; }
    //    }
    //}
}