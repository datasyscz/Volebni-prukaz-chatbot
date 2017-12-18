﻿using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VolebniPrukaz.DialogModels
{
    public enum VoteRoundType
    {
        [Describe("První kolo", message: "FirstRound")]
        [Terms("FirstRound")]
        FirstRound,
        [Describe("Druhé kolo", message: "SecondRound")]
        [Terms("SecondRound")]
        SecondRound,
        [Describe("Obě kola", message: "AllRounds")]
        [Terms("AllRounds")]
        AllRounds
    }

    [Serializable]
    public class VoteRound
    {
        [Prompt("Na která kola volby průkaz chcete? {||}")]
        public VoteRoundType? Type { get; set; }
    }
}