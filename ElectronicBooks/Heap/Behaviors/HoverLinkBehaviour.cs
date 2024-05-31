

using CefSharp;
using CefSharp.Wpf;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace ElectronicBooks.Heap.Behaviors
{
    public class HoverLinkBehaviour : Behavior<ChromiumWebBrowser>
    {
        public static readonly DependencyProperty HoverLinkProperty = DependencyProperty.Register(nameof(HoverLink), typeof(string), typeof(HoverLinkBehaviour), new PropertyMetadata((object)string.Empty));

        public string HoverLink
        {
            get => (string)this.GetValue(HoverLinkBehaviour.HoverLinkProperty);
            set => this.SetValue(HoverLinkBehaviour.HoverLinkProperty, (object)value);
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.StatusMessage += new EventHandler<StatusMessageEventArgs>(this.OnStatusMessageChanged);
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.StatusMessage -= new EventHandler<StatusMessageEventArgs>(this.OnStatusMessageChanged);
        }

        private void OnStatusMessageChanged(object sender, StatusMessageEventArgs e)
        {
            (sender as ChromiumWebBrowser).Dispatcher.BeginInvoke(new Action(() => this.HoverLink = e.Value));
        }
    }
}
