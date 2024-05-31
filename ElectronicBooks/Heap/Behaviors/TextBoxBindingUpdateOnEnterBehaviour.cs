
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectronicBooks.Heap.Behaviors
{
    public class TextBoxBindingUpdateOnEnterBehaviour : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.KeyDown += new KeyEventHandler(this.OnTextBoxKeyDown);
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.KeyDown -= new KeyEventHandler(this.OnTextBoxKeyDown);
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
