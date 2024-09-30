using System;
using System.Collections.Generic;
using System.Diagnostics;
using WinDurango.UI.Utils;
using WinUI3Localizer;

namespace WinDurango.UI.Localization
{
    public static class Locale
    {
        public static string GetLocalizedText(string name, params object[] args)
        {
            return string.Format(name.GetLocalizedString(), args);
        }
    }
}
