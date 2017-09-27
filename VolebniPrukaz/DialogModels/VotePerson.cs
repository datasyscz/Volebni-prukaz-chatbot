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
        [Describe("Převezmu osobně", message: "Personaly")]
        [Terms("Personaly")]
        Personaly,
        [Describe("Zplnomocněná osoba", message: "AuthorizedPerson")]
        [Terms("AuthorizedPerson")]
        AuthorizedPerson,
        [Describe("Zaslat na adresu", message: "SendHome")]
        [Terms("SendHome")]
        SendHome
    }

    [Serializable]
    public class VotePerson
    {
        public VotePersonType? Type { get; set; }
    }
}