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
using VolebniPrukaz.OfficesManager;
using System.Linq;

namespace VolebniPrukaz.Dialogs
{
    public class RootDialog
    {
        protected enum ConversationDataProperties
        {
            PersonalDataDM,
            AddressDM
        }

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
                    var personalData = await res;
                    ctx.ConversationData.SetValue(ConversationDataProperties.PersonalDataDM.ToString(), personalData);
                    return new AddressDialog("Napište mi prosím adresu Vašeho trvalého bydliště.");
                })
                .ContinueWith<AddressDM, object>(async (ctx, res) => {
                    var addressDM = await res;
                    var office = new OfficesContext(HttpContext.Current.Server.MapPath("/offices.json"))
                                        .GetOffices(addressDM.Zip).FirstOrDefault();
                    var personalData = ctx.ConversationData.GetValue<PersonalDataDM>(ConversationDataProperties.PersonalDataDM.ToString());

                    var voterPassServiceUri = GetVoterPassUri(addressDM, personalData, office);

                    var replyMessage = ctx.MakeMessage();
                    replyMessage.Text = "Zde je Váš vygenerovaný volební průkaz.";
                    replyMessage.Attachments.Add(new Attachment()
                    {
                        ContentUrl = voterPassServiceUri.ToString(),
                        ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        Name = "zadost-o-vp.docx"
                    });

                    await ctx.PostAsync(replyMessage);
                    return Chain.Return(string.Empty);
                })
                .ContinueWith(async (ctx, res) => {
                     await res;
                     return Chain.From(() => new PromptDialog.PromptConfirm("Chcete vygenerovat nový volební průkaz?", "Bohužel nerozumím.", 3))
                     .ContinueWith(async (ctx2, res2) => {
                         var isConfirmed = await res2;

                         if (isConfirmed)
                             return StartOverChain();
                         else
                             return Chain.Return("Rád jsem pomohl a děkuji.");
                     });
                });
        }

        private static Uri GetVoterPassUri(AddressDM address, PersonalDataDM personalData, Office office)
        {
            string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
            string controllerPath = "/api/file";

            string query = $"?name={personalData.Name}";
            query += $"&birthDate={personalData.BirthDate}";
            query += $"&phone={personalData.Phone}";
            query += $"&permanentAddress={address.ToAddressString()}";
            query += $"&officeName={office.name}";
            query += $"&officeAddress={office.address}";
            query += $"&officePostalCode={office.zip}";
            query += $"&officeCity={office.city}";

            return new Uri(baseUrl + controllerPath + query);
        }
    }
}