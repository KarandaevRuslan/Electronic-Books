using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class SelectionInAccount : Window
    {

        public SelectionInAccount() => this.InitializeComponent();

        private void Description(object sender, RoutedEventArgs e) => this.Action(1);

        private void Download(object sender, RoutedEventArgs e) => this.Action(2);

        private void Delete(object sender, RoutedEventArgs e) => this.Action(3);

        private void Settings(object sender, RoutedEventArgs e) => this.Action(4);

        private void Action(int mode)
        {
            ((Account)this.Owner).currentMode = mode;
            this.Close();
        }



    }
}
