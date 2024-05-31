using System.Windows;

namespace ElectronicBooks.Views.Windows
{
    public partial class ModeSelection : Window
    {

        public ModeSelection() => this.InitializeComponent();

        private void Edit(object sender, RoutedEventArgs e) => this.Action(1);

        private void View(object sender, RoutedEventArgs e) => this.Action(2);

        private void Export(object sender, RoutedEventArgs e) => this.Action(3);

        private void Delete(object sender, RoutedEventArgs e) => this.Action(4);

        private void Action(int mode)
        {
            ((MainWindow)this.Owner).currentMode = mode;
            this.Close();
        }

        private void Description(object sender, RoutedEventArgs e) => this.Action(5);
    }
}
