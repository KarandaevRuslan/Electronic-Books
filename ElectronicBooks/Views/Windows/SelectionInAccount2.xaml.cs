using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class SelectionInAccount2 : Window
    {

        public SelectionInAccount2() => this.InitializeComponent();

        private void Description(object sender, RoutedEventArgs e) => this.Action(1);

        private void Download(object sender, RoutedEventArgs e) => this.Action(2);

        private void Action(int mode)
        {
            ((BookSearch)this.Owner).currentMode = mode;
            this.Close();
        }

        private void Author(object sender, RoutedEventArgs e) => this.Action(3);
    }
}
