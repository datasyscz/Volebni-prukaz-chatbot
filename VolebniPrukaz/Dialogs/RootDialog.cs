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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace VolebniPrukaz.Dialogs
{
    public class RootDialog
    {
        protected enum ConversationDataProperties
        {
            PersonalData,
            Address,
            Office,
            VotePeson,
            MainChainFirstPass
        }

        public static IDialog<object> StartWithHelloChain()
        {
            return Chain.Return("Ahoj, jsem Volební Průkaz bot a rád Vám pomůžu s vydáním volebního průkazu.")
                .PostToUser()
                .ContinueWith(async (ctx, res) =>
                {
                    await res;
                    return MainChain(ctx);
                });
        }

        public static IDialog<object> StartOverChain(IBotContext ctx)
        {
            return MainChain(ctx, isFirstPass: false);
        }

        public static IDialog<object> MainChain(IBotContext context, bool isFirstPass = true)
        {
            context.ConversationData.SetValue(ConversationDataProperties.MainChainFirstPass.ToString(), isFirstPass);

            return Chain.Return("Pojďme na to.")
                 .PostToUser()
                 .ContinueWith(async (ctx, res) =>
                 {
                     await res;
                     var isFirstPassData = ctx.ConversationData.GetValue<bool>(ConversationDataProperties.MainChainFirstPass.ToString());

                     if (isFirstPassData)
                     {
                         return Chain.From(() =>
                            FormDialog.FromForm(() =>
                            {
                                return new FormBuilder<VotePerson>()
                                .AddRemainingFields()
                                .Confirm("", state => false)
                                .Build();
                            }, FormOptions.None));
                     }
                     else
                     {
                         return Chain.From(() =>
                            FormDialog.FromForm(() =>
                            {
                                return new FormBuilder<VotePerson>()
                                .AddRemainingFields()
                                .Confirm("", state => false)
                                .Build();
                            }, FormOptions.PromptInStart));
                     }
                 })
                .ContinueWith(async (ctx, res) =>
                {
                    ctx.ConversationData.SetValue(ConversationDataProperties.VotePeson.ToString(), await res);
                    return Chain.From(() => FormDialog.FromForm(() => PersonalDataForm.GetPersonalDataForm(), FormOptions.PromptInStart));
                })
                .ContinueWith<PersonalDataDM, AddressDM>(async (ctx, res) =>
                {
                    var personalData = await res;
                    ctx.ConversationData.SetValue(ConversationDataProperties.PersonalData.ToString(), personalData);
                    return new AddressDialog("Napište mi prosím adresu Vašeho trvalého bydliště..");
                })
                .ContinueWith<AddressDM, object>(async (ctx, res) =>
                {
                    var addressDM = await res;
                    var office = new OfficesContext(HttpContext.Current.Server.MapPath("/offices.json"))
                                        .GetOffices(addressDM.Zip).FirstOrDefault();

                    var personalData = ctx.ConversationData.GetValue<PersonalDataDM>(ConversationDataProperties.PersonalData.ToString());
                    var voterPerson = ctx.ConversationData.GetValue<VotePerson>(ConversationDataProperties.VotePeson.ToString());

                    var voterPassServiceUri = GetVoterPassUri(addressDM, personalData, office, voterPerson);
                    var warrantServiceUri = GetWarrantPassUri();

                    if (office == null)
                    {
                        var officeNotFoundMsg = ctx.MakeMessage();
                        officeNotFoundMsg.Text = "Bohužel jsem nemohl najít městský úřad odpovídající Vašemu bydlišti. Vyplňte ho prosím do vygenerované žádosti.";
                        await ctx.PostAsync(officeNotFoundMsg);
                    }

                    var replyMessage = ctx.MakeMessage();
                    replyMessage.Text = "Zde si můžete stáhnout";

                    if (replyMessage.ChannelId == ChannelIds.Facebook)
                    {
                        var buttons = new List<CardAction>();

                        buttons.Add(new CardAction()
                        {
                            Type = "openUrl",
                            Value = voterPassServiceUri.ToString(),
                            Title = "Žádost o volební průkaz"
                        });

                        if (voterPerson.Type == VotePersonType.AuthorizedPerson)
                        {
                            buttons.Add(new CardAction()
                            {
                                Type = "openUrl",
                                Value = warrantServiceUri.ToString(),
                                Title = "Plnou moc"
                            });
                        }

                        HeroCard hc = new HeroCard()
                        {
                            Buttons = buttons
                        };

                        replyMessage.Attachments.Add(hc.ToAttachment());
                    }
                    else
                    {
                        replyMessage.Attachments.Add(new Attachment()
                        {
                            ContentUrl = voterPassServiceUri.ToString(),
                            ContentType = "application/msword",
                            Name = "zadost-o-vp.docx"
                        });

                        if (voterPerson.Type == VotePersonType.AuthorizedPerson)
                        {
                            replyMessage.Attachments.Add(new Attachment()
                            {
                                ContentUrl = warrantServiceUri.ToString(),
                                ContentType = "application/msword",
                                Name = "plna-moc-volby-2017.docx"
                            });
                        }
                    }

                    await ctx.PostAsync(replyMessage);

                    return Chain.Return(string.Empty);
                })
                .ContinueWith(async (ctx, res) => {
                    await res;

                    return Chain.Return("Žádost o volební průkaz můžete zaslat poštou, nebo pomocí datové schránky.")
                    .PostToUser();
                })
                .ContinueWith(async (ctx, res) =>
                {
                    await Task.Run(() => Thread.Sleep(5000));
                    await res;

                    return new ConfirmDialog("Přejete si pokračovat vytvořením nové žádosti?", 
                        "Pokračovat", 
                        "Bohužel nerozumím", 
                        possibleAnswers: new[] { "ano", "jo", "pokračovat", "přeji", "přeju" })
                    .ContinueWith(async (ctx2, res2) =>
                    {
                        await res2;
                        return StartOverChain(ctx2);
                    });
                });
        }

        public static IMessageActivity AddAttachmentUrl(IMessageActivity msg, Uri uri, string fileName, string fbTitle = "Stáhnout", string contentType = "application/msword")
        {
            if (msg.ChannelId == ChannelIds.Facebook)
            {
                HeroCard hc = new HeroCard()
                {
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Type = "openUrl",
                            Value = uri.ToString(),
                            Title = fbTitle
                        }
                    }
                };
                msg.Attachments.Add(hc.ToAttachment());
            }
            else
            {
                msg.Attachments.Add(new Attachment()
                {
                    ContentUrl = uri.ToString(),
                    ContentType = contentType,
                    Name = fileName
                });
            }

            return msg;
        }

        private static Uri GetVoterPassUri(AddressDM address, PersonalDataDM personalData, Office office, VotePerson votePerson)
        {
            string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
            string controllerPath = "/api/file";

            string query = $"?name={personalData.Name}";
            query += $"&birthDate={personalData.BirthDate}";
            query += $"&phone={personalData.Phone}";
            query += $"&permanentAddress={address.ToAddressString()}";
            query += $"&officeName={office?.name ?? string.Empty}";
            query += $"&officeAddress={office?.address ?? string.Empty}";
            query += $"&officePostalCode={office?.zip ?? string.Empty}";
            query += $"&officeCity={office?.city ?? string.Empty}";
            query += $"&voterPersonType={votePerson.Type}";

            return new Uri(baseUrl + controllerPath + query);
        }

        private static Uri GetWarrantPassUri()
        {
            string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
            string controllerPath = "/api/file";

            return new Uri(baseUrl + controllerPath);
        }
    }
}