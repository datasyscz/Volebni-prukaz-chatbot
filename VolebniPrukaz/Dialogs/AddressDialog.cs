using Google.Maps;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VolebniPrukaz.API.Facebook;
using VolebniPrukaz.DialogModels;
using VolebniPrukaz.Forms;

namespace VolebniPrukaz.Dialogs
{
    [Serializable]
    public class AddressDialog : IDialog<AddressDM>
    {
        protected readonly GoogleMapsClient _mapApiClient;

        public string _questionText { get; set; }
        public string _deliveryPersonNameText { get; set; }
        public string _confirmText { get; set; }
        public string _addressNotFound { get; set; }
        public string _questionAgainText { get; set; }
        private string _dontUnderstoodText { get; set; }
        public string _addressNotFoundByGoogleText { get; set; }

        public AddressDM _recognizedAddress { get; set; }

        public AddressDialog(string questionText,
            string deliveryPersonNameText = "Zadejte prosím celé jméno nového příjemce.",
            string confirmText = "Je tato adresa správně?",
            string addressNotFoundText = "Bohužel jsem tuto adresu nenašel. Je opravdu správně?",
            string questionAgainText = "Tak nám prosím napište správnou adresu. Nebo ji zkuste napsat podrobněji.",
            string dontUnderstoodText = "Bohužel nerozumím. Ano, nebo ne?",
            string addressNotFoundByGoogleText = "S touto adresou si bohužel nevím rady. Pojďmě si ji projít postupně."
            )
        {
            _mapApiClient = new GoogleMapsClient();
            _recognizedAddress = new AddressDM();

            _questionText = questionText;
            _deliveryPersonNameText = deliveryPersonNameText;
            _confirmText = confirmText;
            _addressNotFound = addressNotFoundText;
            _dontUnderstoodText = dontUnderstoodText;
            _questionAgainText = questionAgainText;
            _addressNotFoundByGoogleText = addressNotFoundByGoogleText;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(_questionText);
            context.Wait(ReadAddressAsync);
        }

        private async Task ReadAddressAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var addressActivity = await result;

            var geocodeResult = await _mapApiClient.GetGeocodeData(addressActivity.Text);
            var firstResultAddress = ((Geocode)geocodeResult.Data).results.FirstOrDefault();

            Activity replyToConversation = (Activity)context.MakeMessage();
            Attachment card;

            if (geocodeResult.CorrectResponse)
            {
                _recognizedAddress = firstResultAddress.MapGeocodeToAddressDM();
                var plAttachment = await GetAddressConfirmationAttachment(_recognizedAddress, context);

                replyToConversation.AttachmentLayout = AttachmentLayoutTypes.List;
                replyToConversation.Attachments = new List<Microsoft.Bot.Connector.Attachment>();

                replyToConversation.Attachments.Add(plAttachment);
                await context.PostAsync(replyToConversation);

                context.Wait(ConfirmRecognition);
            }
            else
            {
                await context.SayAsync(_questionText);
                var hotelsFormDialog = FormDialog.FromForm(AddressForm.BuildAddressForm, FormOptions.PromptInStart);
                context.Call(hotelsFormDialog, SetAddressFormToDM);
            }
        }

        public async Task<Attachment> GetAddressConfirmationAttachment(string address, IDialogContext context)
        {
            Attachment attch;
            if (context.MakeMessage().ChannelId == ChannelIds.Facebook)
            {
                var msg = context.MakeMessage();
                //Send quick replay
            }
            else
            {

                var mapImageDataResult = await _mapApiClient.GetMapImageData(address, zoom: 17);

                List<CardImage> cardImages = new List<CardImage>();

                if (mapImageDataResult.CorrectResponse)
                    cardImages.Add(new CardImage {Url = (string) mapImageDataResult.Data});

                List<CardAction> cardButtons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Value = "Ano",
                        Title = "Ano",
                        Type = ActionTypes.ImBack
                    },
                    new CardAction()
                    {
                        Value = "Ne",
                        Title = "Ne",
                        Type = ActionTypes.ImBack
                    }
                };

                var cardText = $"{_confirmText}\r\n\r\n*{address}*";

                attch = new HeroCard()
                {
                    Images = cardImages,
                    Text = cardText,
                    Buttons = cardButtons,
                }.ToAttachment();
            }

            return attch;
        }

        public async Task<Attachment> GetAddressConfirmationAttachment(AddressDM address, IDialogContext context)
        {
            var addressString = $"{address.Street} {address.HouseNumber}, {address.Zip} {address.City}, Česká republika";
            return await GetAddressConfirmationAttachment(addressString, context);
        }

        private async Task SetAddressFormToDM(IDialogContext context, IAwaitable<AddressDM> result)
        {
            _recognizedAddress = await result;
            context.Done(_recognizedAddress);
        }

        private async Task ConfirmRecognition(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var yesMatches = Microsoft.Bot.Builder.Resource.Resources.MatchYes.Split(';');
            var activity = await result;
            var text = activity.Text;

            foreach (var item in yesMatches)
            {
                if (item.ToLower().Equals(text.ToLower()))
                {
                    context.Done(_recognizedAddress);
                    return;
                }
            }

            var noMatches = Microsoft.Bot.Builder.Resource.Resources.MatchNo.Split(';');

            foreach (var item in noMatches)
            {
                if (item.ToLower().Equals(text.ToLower()))
                {
                    await context.PostAsync(_questionAgainText);
                    context.Wait(ReadAddressAsync);
                    return;
                }
            }

            await context.PostAsync("Bohužel nerozumím. Ano, nebo ne?");
            context.Wait(ConfirmRecognition);
        }
    }
}