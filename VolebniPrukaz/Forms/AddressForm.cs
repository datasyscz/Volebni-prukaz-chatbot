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
                        await Task.CompletedTask;
                    })
                .Field(nameof(AddressDM.Zip),
                    prompt: "Zadejte prosím PSČ.",
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Value = response
                        };

                        return result;
                        await Task.CompletedTask;
                    })
                .Field(nameof(AddressDM.Street),
                    prompt: "Zadejte prosím ulici (bez čísla domu).",
                    validate: async (state, response) => {
                        ValidateResult result = new ValidateResult
                        {
                            IsValid = true,
                            Value = response
                        };

                        return result;
                        await Task.CompletedTask;
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
                        await Task.CompletedTask;
                    })
                .Build();
        }
    }
}