using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VolebniPrukaz.DialogModels
{
    public enum VotePersonType
    {
        [Describe("Vyzvednu osobně", message: "Personaly")]
        [Terms("Personaly")]
        Personaly,
        [Describe("Vyzvedne někdo jiný", message: "AuthorizedPerson")]
        [Terms("AuthorizedPerson")]
        AuthorizedPerson,
        [Describe("Zaslat na adresu", message: "SendHome")]
        [Terms("SendHome")]
        SendHome
    }

    [Serializable]
    public class VotePerson
    {
        [Describe("Jak Vám bude doručení voličského průkazu nejlépe vyhovovat.")]
        public VotePersonType? Type { get; set; }
    }
}