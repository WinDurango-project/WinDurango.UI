using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Linq;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Dialogs
{
    public sealed partial class AppListDialog : ContentDialog
    {
        public AppListDialog()
        {
            this.InitializeComponent();

            var packages = Packages.GetInstalledPackages().ToList();

            Logger.Instance.WriteDebug($"Loaded {packages.Count} packages.");
            // trying to do ANYTHING with appListView crashes...
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
