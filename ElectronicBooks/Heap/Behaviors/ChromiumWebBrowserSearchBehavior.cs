
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;

namespace ElectronicBooks.Heap.Behaviors
{
  public class ChromiumWebBrowserSearchBehavior : Behavior<ChromiumWebBrowser>
  {
    private bool _isSearchEnabled;
    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(nameof (SearchText), typeof (string), typeof (ChromiumWebBrowserSearchBehavior), new PropertyMetadata((object) null, new PropertyChangedCallback(ChromiumWebBrowserSearchBehavior.OnSearchTextChanged)));
    public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(nameof (NextCommand), typeof (ICommand), typeof (ChromiumWebBrowserSearchBehavior), new PropertyMetadata((object) null));
    public static readonly DependencyProperty PreviousCommandProperty = DependencyProperty.Register(nameof (PreviousCommand), typeof (ICommand), typeof (ChromiumWebBrowserSearchBehavior), new PropertyMetadata((object) null));

    public ChromiumWebBrowserSearchBehavior()
    {
      this.NextCommand = (ICommand) new DelegateCommand(new Action(this.OnNext));
      this.PreviousCommand = (ICommand) new DelegateCommand(new Action(this.OnPrevious));
    }

    private void OnNext() => this.AssociatedObject.Find(this.SearchText, true, false, true);

    private void OnPrevious() => this.AssociatedObject.Find(this.SearchText, false, false, true);

    protected override void OnAttached()
    {
      this.AssociatedObject.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(this.ChromiumWebBrowserOnFrameLoadEnd);
    }

    private void ChromiumWebBrowserOnFrameLoadEnd(
      object sender,
      FrameLoadEndEventArgs frameLoadEndEventArgs)
    {
      this._isSearchEnabled = frameLoadEndEventArgs.Frame.IsMain;
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!this._isSearchEnabled || string.IsNullOrEmpty(this.SearchText))
          return;
        this.AssociatedObject.Find(this.SearchText, true, false, false);
      }));
    }

    public string SearchText
    {
      get => (string) this.GetValue(ChromiumWebBrowserSearchBehavior.SearchTextProperty);
      set => this.SetValue(ChromiumWebBrowserSearchBehavior.SearchTextProperty, (object) value);
    }

    public ICommand NextCommand
    {
      get => (ICommand) this.GetValue(ChromiumWebBrowserSearchBehavior.NextCommandProperty);
      set => this.SetValue(ChromiumWebBrowserSearchBehavior.NextCommandProperty, (object) value);
    }

    public ICommand PreviousCommand
    {
      get => (ICommand) this.GetValue(ChromiumWebBrowserSearchBehavior.PreviousCommandProperty);
      set
      {
        this.SetValue(ChromiumWebBrowserSearchBehavior.PreviousCommandProperty, (object) value);
      }
    }

    private static void OnSearchTextChanged(
      DependencyObject dependencyObject,
      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      if (!(dependencyObject is ChromiumWebBrowserSearchBehavior browserSearchBehavior) || !browserSearchBehavior._isSearchEnabled)
        return;
      string newValue = dependencyPropertyChangedEventArgs.NewValue as string;
      if (string.IsNullOrEmpty(newValue))
        browserSearchBehavior.AssociatedObject.StopFinding(true);
      else
        browserSearchBehavior.AssociatedObject.Find(newValue, true, false, false);
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.FrameLoadEnd -= new EventHandler<FrameLoadEndEventArgs>(this.ChromiumWebBrowserOnFrameLoadEnd);
    }
  }
}
