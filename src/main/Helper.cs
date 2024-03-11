using Blazored.Toast.Services;
using ei8.Cortex.Library.Common;
using Microsoft.AspNetCore.Components;
using neurUL.Common.Domain.Model;
using neurUL.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Plugins.Kanban
{
    internal static class Helper
    {
        internal static void ReinitializeOption(Action<ContextMenuOption> optionSetter)
        {
            optionSetter(ContextMenuOption.NotSet);
            optionSetter(ContextMenuOption.New);
        }

        internal static bool ValidateExists(this Neuron value, IToastService toastService, string errorMessage)
        {
            bool result = true;

            if (value == null)
            {
                toastService.ShowError(errorMessage);
                result = false;
            }

            return result;
        }
    }
}
