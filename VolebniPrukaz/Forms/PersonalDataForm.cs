﻿using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using VolebniPrukaz.DialogModels;

namespace VolebniPrukaz.Forms
{
    public class PersonalDataForm
    {
        public static IForm<PersonalDataDM> GetPersonalDataForm()
        {
            return new FormBuilder<PersonalDataDM>()
                .Field(nameof(PersonalDataDM.Name),
                    prompt: "Poprosím Vás o zadání Vašeho celého jména.",
                    validate: async (state, response) =>
                    {
                        string str = (string)response;

                        //Check if name or surname
                        if (str.Split(' ').Length >= 2)
                        {
                            return new ValidateResult
                            {
                                IsValid = true,
                                Feedback = null,
                                Value = response
                            };
                        }
                        else
                        {
                            return new ValidateResult
                            {
                                IsValid = false,
                                Feedback = "Takové jméno neznám. Je nutné zadat celé jméno - jméno a příjmení..",
                                Value = response
                            };
                        }
                    })
                .Field(nameof(PersonalDataDM.BirthDate),
                    prompt: "Poprosím Vás o zadání Vašeho data narození.",
                    validate: async (state, response) =>
                    {
                        state.BirthDate = (string)response;

                        DateTime? date = state.BirthDateConverted;

                        //Check if name or surname
                        if (date != null && date != DateTime.MinValue)
                        {
                            if (date.Value.AddYears(18) <= DateTime.Now && date.Value > DateTime.Now.AddYears(-130))
                            {
                                return new ValidateResult
                                {
                                    IsValid = true,
                                    Feedback = null,
                                    Value = response
                                };
                            }
                            else if (date.Value <= DateTime.Now.AddYears(-130))
                            {
                                return new ValidateResult
                                {
                                    IsValid = false,
                                    Feedback = "Není Vám nějak moc?! Nejsem si jist, zda k volbám dojdete… 😦",
                                    Value = response
                                };
                            }
                            else
                            {
                                return new ValidateResult
                                {
                                    IsValid = false,
                                    Feedback = "Lidé mladší 18 let k volbám nemohou.",
                                    Value = response
                                };
                            }
                            
                        }
                        else
                        {
                            return new ValidateResult
                            {
                                IsValid = false,
                                Feedback = "Bohužel nerozumím, je mi teprve pár dní. Napište mi datum například ve formátu DD.MM.RRRR",
                                Value = response
                            };
                        }
                    })
                .Field(nameof(PersonalDataDM.Phone),
                    prompt: "Poprosím Vás o zadání Vašeho telefonního čísla.",
                    validate: async (state, response) =>
                    {
                        string phone = (string) response;
                        Regex regex = new Regex(@"^(\+420)? ?[1-9][0-9]{2} ?[0-9]{3} ?[0-9]{3}$");

                        //Check if name or surname
                        if (regex.IsMatch(phone))
                        {
                            return new ValidateResult
                            {
                                IsValid = true,
                                Feedback = null,
                                Value = response
                            };
                        }
                        else
                        {
                            return new ValidateResult
                            {
                                IsValid = false,
                                Feedback = "Bohužel nerozumím, je mi teprve pár dní. Napište mi telefonní číslo například ve formátu 606 333 111.",
                                Value = response
                            };
                        }
                    })
                .Build();
        }
    }
}