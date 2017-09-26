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
            string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
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
}