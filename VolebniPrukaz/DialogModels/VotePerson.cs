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
        [Describe("Zaslat domů", message: "SendHome")]
        [Terms("SendHome")]
        SendHome,
        [Describe("Zaslat jinam", message: "SendOnDifferentAddress")]
        [Terms("SendOnDifferentAddress")]
        SendToDifferentAddress,
        [Describe("Někdo jej vyzvedne", message: "AuthorizedPerson")]
        [Terms("AuthorizedPerson")]
        AuthorizedPerson
    }

    [Serializable]
    public class VotePerson
    {
        [Prompt("Jakým způsobem k Vám má být voličský průkaz doručen? {||}")]
        [Template(TemplateUsage.NotUnderstood, "\"{0}\" není platbou volbou. Jak Vám tedy bude doručení voličského průkazu nejlépe vyhovovat?")]
        public VotePersonType? Type { get; set; }
    }
}