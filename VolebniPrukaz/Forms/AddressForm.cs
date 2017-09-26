using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VolebniPrukaz.DialogModels;

namespace VolebniPrukaz.Forms
{
    public class AddressForm
    {
        public static IForm<AddressDM> BuildAddressForm()
        {
            return new FormBuilder<AddressDM>()
                .Field(nameof(AddressDM.City),
                    prompt: "Zadejte prosím obec.",
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Value = response
                        };
                        return result;
                    })
                .Field(nameof(AddressDM.Zip),
                    prompt: "Zadejte prosím PSČ.",
                    validate: async (state, response) =>
                    {
                        string zipStr = (string) response;
                        if (int.TryParse((string) response, out int zip) &&
                            (zipStr.Trim().Replace(" ", "").Length == 5))
                        {
                            return new ValidateResult
                            {
                                IsValid = true,
                                Value = response
                            };
                        }
                        else
                        {
                            return new ValidateResult
                            {
                                IsValid = false,
                                Value = response,
                                Feedback = "Tak tomu nerozumím, potřebuji smerovací číslo, například ve formátu 130 00"
                            };
                        }
                    })
                .Field(nameof(AddressDM.Street),
                    prompt: "Zadejte prosím ulici (bez čísla domu).",
                    validate: async (state, response) => {
                        string adress = (string)response;
                        if (adress.Length >= 2)
                        {
                            return new ValidateResult
                            {
                                IsValid = true,
                                Value = response
                            };
                        }
                        else
                        {
                            return new ValidateResult
                            {
                                IsValid = false,
                                Value = response,
                                Feedback = "Takovou ulici neznam. Asi mě zkoušíte že?"
                            };
                        }
                    })
                .Field(nameof(AddressDM.HouseNumber),
                    prompt: "Zadejte prosím číslo domu.",
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Value = response
                        };
                        return result;
                    })
                .Build();
        }
    }
}