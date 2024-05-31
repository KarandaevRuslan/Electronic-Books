
using CefSharp;


namespace ElectronicBooks.Heap.Behaviors
{
  internal class CustomLifeSpanHandler : ILifeSpanHandler
  {
    public bool OnBeforePopup(
      IWebBrowser chromiumWebBrowser,
      IBrowser browser,
      IFrame frame,
      string targetUrl,
      string targetFrameName,
      WindowOpenDisposition targetDisposition,
      bool userGesture,
      IPopupFeatures popupFeatures,
      IWindowInfo windowInfo,
      IBrowserSettings browserSettings,
      ref bool noJavascriptAccess,
      out IWebBrowser newBrowser)
    {
      browser.MainFrame.LoadUrl(targetUrl);
      newBrowser = (IWebBrowser) null;
      return true;
    }

    bool ILifeSpanHandler.DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser) => false;

    void ILifeSpanHandler.OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
    }

    void ILifeSpanHandler.OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
    }
  }
}
