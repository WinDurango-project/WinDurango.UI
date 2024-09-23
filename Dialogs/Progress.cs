using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinDurango.UI.Dialogs
{
    public class ProgressDialog : ContentDialog
    {
        private string _text;
        private string _title;
        private double _progress;
        private bool _isIndeterminate;
        private ProgressBar _progressBar;
        private TextBlock _textBlock;

        public ProgressDialog(string content, string title = "Information", bool isIndeterminate = true)
        {
            _text = content;
            _title = title;
            _isIndeterminate = isIndeterminate;

            _progressBar = new ProgressBar
            {
                IsIndeterminate = _isIndeterminate,
                Width = 300,
                Value = _progress
            };

            _textBlock = new TextBlock
            {
                Text = _text,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Content = new StackPanel
            {
                Children =
                {
                    _progressBar,
                    _textBlock
                },
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            Title = _title;
            XamlRoot = App.MainWindow.Content.XamlRoot;
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                UpdateDialog();
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                UpdateDialog();
            }
        }

        private void UpdateDialog()
        {
            if (_textBlock != null)
            {
                _textBlock.Text = _text;
            }

            if (_progressBar != null)
            {
                _progressBar.Value = _progress;
            }
        }
    }
}
