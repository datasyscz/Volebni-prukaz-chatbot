using Microsoft.Bot.Builder.FormFlow;
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
                    prompt: "Zadejte prosím Vaše celé jméno.",
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
                                Feedback = "Zadejte prosím jméno a přijmení, ne pouze jméno nebo přijmení. Potřebuji to do volebního průkazu.",
                                Value = response
                            };
                        }
                    })
                .Field(nameof(PersonalDataDM.BirthDate),
                    prompt: "Zadejte prosím Vaše datum narození.",
                    validate: async (state, response) =>
                    {
                        state.BirthDate = (string)response;

                        DateTime? date = state.BirthDateConverted;

                        //Check if name or surname
                        if (date != null && date != DateTime.MinValue)
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
                                Feedback = "Bohužel nerozumím, je mi teprve pár dní. Zadejte prosím datum například ve formátu 6.5.1991",
                                Value = response
                            };
                        }
                    })
                .Field(nameof(PersonalDataDM.Phone),
                    prompt: "Zadejte prosím Váš telefon.",
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
                                Feedback = "Bohužel nerozumím, je mi teprve pár dní. Rozumím například tomuto formátu telefoního čísla 654 987 321",
                                Value = response
                            };
                        }
                    })
                .Build();
        }
    }
}