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
            return Chain.Return("Zdravím Vás! Já jsem chatovací robot. Možná takové jako jsem já ještě neznáte. Nebojte, nejsem sice žijící tvor, stejně se ale domluvíme. 👍")
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

            return Chain.Return("Pojďme na to! Jsem tu od toho, abych Vám pomohl získat Váš voličský průkaz. Položím Vám proto několik otázek.")
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
                    return new AddressDialog("Výborně. V tuto chvíli potřebuji ještě adresu Vašeho trvalého bydliště.", 
                        confirmText: "Děkuji, tam jsem ještě nebyl! 👀 Je tato adresa podle mapy správně?",
                        questionAgainText: "Aha. Někde se tedy stala chyba. Je z Vaší strany adresa napsána skutečně správně? Zkuste to prosím ještě jednou jinak, podrobněji.",
                        addressNotFoundByGoogleText: "Tomu bohužel nerozumím. Pojďme tedy Vaši adresu rozebrat postupně.");
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
                    replyMessage.Text = "Tak hotovo! 👌 Žádost o Váš voličský průkaz je tu.";

                    if (replyMessage.ChannelId == ChannelIds.Facebook)
                    {
                        var buttons = new List<CardAction>();

                        buttons.Add(new CardAction()
                        {
                            Type = "openUrl",
                            Value = voterPassServiceUri.ToString(),
                            Title = "Stáhnout žádost"
                        });

                        if (voterPerson.Type == VotePersonType.AuthorizedPerson)
                        {
                            buttons.Add(new CardAction()
                            {
                                Type = "openUrl",
                                Value = warrantServiceUri.ToString(),
                                Title = "Stáhnout plnou moc"
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
                .ContinueWith<object, object>(async (ctx, res) =>
                {
                    await res;
                    return new ConfirmDialog("Bylo mi velkým potěšením.",
                        "Více informací",
                        "Tomu bohužel nerozumím :(",
                        possibleAnswers: new[] { "pokračovat", "informace" });
                })
                .ContinueWith(async (ctx, res) => {
                    await res;

                    return Chain.Return("Já a moji druzi se snažíme zjednodušovat Váš styk s úřady. Víte, že už nyní jde s úřady komunikovat datovou schránkou? " +
                        "Díky ní už nemusíte na úřady fyzicky chodit, spousta formulářů se dá odeslat pomocí portálu https://podejto.cz/. " +
                        "Mrkněte na to, Váš čas je přeci drahý!")
                        .PostToUser();
                })
                .ContinueWith(async (ctx, res) =>
                {
                    await Task.Run(() => Thread.Sleep(5000));
                    await res;

                    return new ConfirmDialog("Přejete si pokračovat vytvořením další žádosti?", 
                        "Pokračovat", 
                        "Tomu bohužel nerozumím :(", 
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

            string query = $"?name={Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(personalData.Name.ToLower())}";
            query += $"&birthDate={((DateTime)personalData.BirthDateConverted).ToShortDateString()}";
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