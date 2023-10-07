using System.Windows;
using ZoDream.Shared.Loggers;
using ZoDream.ZipCrack.ViewModels;

namespace ZoDream.ZipCrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (ViewModel.Logger is not EventLogger logger)
            {
                return;
            }
            var isLastProgress = false;
            logger.OnLog += (s, e) => {
                isLastProgress = false;
                Dispatcher.Invoke(() => {
                    infoTb.AppendLine(s);
                });
            };
            logger.OnProgress += (s, e) => {
                Dispatcher.Invoke(() => {
                    progressBar.Value = s * 100 / e;
                    if (isLastProgress)
                    {
                        infoTb.ReplaceLine($"{s}/{e}");
                    }
                    else
                    {
                        infoTb.AppendLine($"{s}/{e}");
                    }
                });
                isLastProgress = true;
            };
        }

        public MainViewModel ViewModel => (MainViewModel)DataContext;


    }
}
