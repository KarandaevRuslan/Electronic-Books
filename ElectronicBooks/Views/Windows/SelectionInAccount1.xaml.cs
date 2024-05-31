using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class SelectionInAccount1 : Window
    {

        public SelectionInAccount1() => this.InitializeComponent();

        private void Description(object sender, RoutedEventArgs e) => this.Action(1);

        private void Download(object sender, RoutedEventArgs e) => this.Action(2);

        private void Action(int mode)
        {
            ((Account1)this.Owner).currentMode = mode;
            this.Close();
        }
    }
}
