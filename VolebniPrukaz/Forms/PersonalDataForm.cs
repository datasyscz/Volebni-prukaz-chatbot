using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Feedback = "Děkuji",
                            Value = response
                        };

                        return result;
                        await Task.CompletedTask;
                    })
                .Field(nameof(PersonalDataDM.BirthDate),
                    prompt: "Zadejte prosím Vaše datum narození.",
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Feedback = "Děkuji",
                            Value = response
                        };

                        return result;
                        await Task.CompletedTask;
                    })
                .Field(nameof(PersonalDataDM.Phone),
                    prompt: "Zadejte prosím Váš telefon.",
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Feedback = "Děkuji",
                            Value = response
                        };

                        return result;
                        await Task.CompletedTask;
                    })
                .Build();
        }
    }
}