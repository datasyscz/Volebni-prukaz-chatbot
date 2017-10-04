using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VolebniPrukaz.API.Facebook;

namespace VolebniPrukaz.Dialogs
{
    [Serializable]
    public class ConfirmDialog : IDialog<IMessageActivity>
    {

        public ConfirmDialog(string message, string buttonText, string dontUnderstand = "nerozumim", string[] possibleAnswers = null)
        {
            _message = message;
            _buttonText = buttonText;
            _dontUnderstand = dontUnderstand;
            _possibleAnswers = possibleAnswers ?? new string[0];
        }

        private string _message { get; }
        private string _buttonText { get; }
        private string _dontUnderstand { get; }
        public string[] _possibleAnswers { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            //if (!string.IsNullOrEmpty(_message))
            //{
            //    var msg = CreateConfirmMessage(context);
            //    await context.PostAsync(msg);
            //}

            var msg = CreateConfirmMessage(context);
            await context.PostAsync(msg);

            context.Wait(Resume);
        }

        private async Task Resume(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var msgText = (await result).Text;
            var msgWords = msgText
                .Replace(".", string.Empty)
                .Replace(",", string.Empty)
                .Replace("?", string.Empty)
                .Replace("!", string.Empty)
                .Split(' ');

            if (msgText.ToLower() == _buttonText.ToLower() || _possibleAnswers.Any(a => msgWords.Any(w => w.ToLower() == a.ToLower())))
            {
                context.Done(await result);
            }
            else
            {
                await context.PostAsync(_dontUnderstand);

                //if (!string.IsNullOrEmpty(_message))
                //{
                //    var msg = CreateConfirmMessage(context);
                //    await context.PostAsync(msg);
                //}

                var msg = CreateConfirmMessage(context);
                await context.PostAsync(msg);

                context.Wait(Resume);
            }
        }

        private IMessageActivity CreateConfirmMessage(IDialogContext context)
        {
            var msg = context.MakeMessage();

            if (msg.ChannelId == "facebook")
            {
                var quickReplay = new QuickReplyData();
                quickReplay.text = _message;
                quickReplay.quick_replies = new Quick_Replies[1] { new Quick_Replies { content_type = "text", payload = _buttonText, title = _buttonText } };
                    

                msg.ChannelData = quickReplay;
            }
            else
            {
                msg.Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = "application/vnd.microsoft.card.hero",
                        Content = new HeroCard
                        {
                            Text = _message,
                            Buttons = new List<CardAction>
                            {
                                new CardAction
                                {
                                    Value = _buttonText,
                                    Type = "imBack",
                                    Title = _buttonText
                                }
                            }
                        }
                    }
                };
            }
            return msg;
        }
    }
}