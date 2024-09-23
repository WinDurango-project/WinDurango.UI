using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace WinDurango.UI.Dialogs
{
    public class Confirmation
    {
        private string _content;
        private string _title;
        private ContentDialog _confirmationDialog;

        public Confirmation(string content, string title = "Information")
        {
            _content = content;
            _title = title;
        }

        public async Task<Dialog.BtnClicked> Show()
        {
            _confirmationDialog = new ContentDialog
            {
                Content = _content,
                Title = _title,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await _confirmationDialog.ShowAsync();

            return result switch
            {
                ContentDialogResult.Primary => Dialog.BtnClicked.Yes,
                ContentDialogResult.Secondary => Dialog.BtnClicked.No,
                _ => throw new InvalidOperationException("Unexpected result."),
            };
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                UpdateDialog();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                UpdateDialog();
            }
        }

        private void UpdateDialog()
        {
            if (_confirmationDialog != null)
            {
                _confirmationDialog.Content = _content;
                _confirmationDialog.Title = _title;
            }
        }

        public void Remove()
        {
            _confirmationDialog = null;
        }
    }
}
