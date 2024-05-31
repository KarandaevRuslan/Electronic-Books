using CefSharp;
using CefSharp.Wpf;
using ElectronicBooks.Heap.Other;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ElectronicBooks.Heap.Behaviors
{
    internal class CustomMenuHandler : IContextMenuHandler
    {
        private string linkUrl;
        private string sourceUrl;
        private ContextMenuMediaType mt;
        private string selectionText;

        void IContextMenuHandler.OnBeforeContextMenu(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          IFrame frame,
          IContextMenuParams parameters,
          IMenuModel model)
        {
            this.linkUrl = parameters.LinkUrl;
            this.sourceUrl = parameters.SourceUrl;
            this.mt = parameters.MediaType;
            this.selectionText = parameters.SelectionText;
            model.Clear();
            model.AddItem(CefMenuCommand.Back, "Назад");
            model.AddItem(CefMenuCommand.Forward, "Вперед");
            model.AddItem(CefMenuCommand.Reload, "Перезагрузить");
            model.AddSeparator();
            model.AddItem(CefMenuCommand.Copy, "Копировать");
            model.AddItem((CefMenuCommand)26504, "Копировать изображение");
            model.AddItem((CefMenuCommand)26503, "Скачать изображение");
            model.AddSeparator();
            model.AddItem(CefMenuCommand.Print, "Печать");
            model.AddItem(CefMenuCommand.Find, "Найти");
        }

        bool IContextMenuHandler.OnContextMenuCommand(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          IFrame frame,
          IContextMenuParams parameters,
          CefMenuCommand commandId,
          CefEventFlags eventFlags)
        {
            return false;
        }

        void IContextMenuHandler.OnContextMenuDismissed(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          IFrame frame)
        {
            ChromiumWebBrowser webBrowser = (ChromiumWebBrowser)chromiumWebBrowser;
            webBrowser.Dispatcher.Invoke((Action)(() => webBrowser.ContextMenu = (System.Windows.Controls.ContextMenu)null));
        }

        bool IContextMenuHandler.RunContextMenu(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          IFrame frame,
          IContextMenuParams parameters,
          IMenuModel model,
          IRunContextMenuCallback callback)
        {
            ChromiumWebBrowser webBrowser = (ChromiumWebBrowser)chromiumWebBrowser;
            List<Tuple<string, CefMenuCommand, bool>> menuItems = CustomMenuHandler.GetMenuItems(model).ToList<Tuple<string, CefMenuCommand, bool>>();
            webBrowser.Dispatcher.Invoke((Action)(() =>
            {
                System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu()
                {
                    IsOpen = true
                };
                RoutedEventHandler handler = (RoutedEventHandler)null;
                handler = (RoutedEventHandler)((s, e) =>
          {
              menu.Closed -= handler;
              if (callback.IsDisposed)
                  return;
              callback.Cancel();
          });
                menu.Closed += handler;
                foreach (Tuple<string, CefMenuCommand, bool> tuple in menuItems)
                {
                    Tuple<string, CefMenuCommand, bool> item = tuple;
                    if (item.Item2 == CefMenuCommand.NotFound && string.IsNullOrWhiteSpace(item.Item1))
                    {
                        menu.Items.Add((object)new Separator());
                    }
                    else
                    {
                        ItemCollection items = menu.Items;
                        items.Add((object)new System.Windows.Controls.MenuItem()
                        {
                            Header = (object)item.Item1.Replace("&", "_"),
                            IsEnabled = item.Item3,
                            Command = (ICommand)new RelayCommand((Action)(() =>
                      {
                          switch (item.Item2)
                          {
                              case CefMenuCommand.Back:
                                  browser.GoBack();
                                  break;
                              case CefMenuCommand.Forward:
                                  browser.GoForward();
                                  break;
                              case CefMenuCommand.Reload:
                                  browser.Reload();
                                  break;
                              case CefMenuCommand.ReloadNoCache:
                                  browser.Reload(true);
                                  break;
                              case CefMenuCommand.StopLoad:
                                  browser.StopLoad();
                                  break;
                              case CefMenuCommand.Undo:
                                  browser.FocusedFrame.Undo();
                                  break;
                              case CefMenuCommand.Redo:
                                  browser.FocusedFrame.Redo();
                                  break;
                              case CefMenuCommand.Cut:
                                  browser.FocusedFrame.Cut();
                                  break;
                              case CefMenuCommand.Copy:
                                  browser.FocusedFrame.Copy();
                                  break;
                              case CefMenuCommand.Paste:
                                  browser.FocusedFrame.Paste();
                                  break;
                              case CefMenuCommand.SelectAll:
                                  browser.FocusedFrame.SelectAll();
                                  break;
                              case CefMenuCommand.Find:
                                  browser.GetHost().Find(this.selectionText, true, false, false);
                                  break;
                              case CefMenuCommand.Print:
                                  browser.GetHost().Print();
                                  break;
                              case CefMenuCommand.ViewSource:
                                  browser.FocusedFrame.ViewSource();
                                  break;
                              case CefMenuCommand.AddToDictionary:
                                  browser.GetHost().AddWordToDictionary(parameters.MisspelledWord);
                                  break;
                              case (CefMenuCommand)26501:
                                  browser.GetHost().ShowDevTools();
                                  break;
                              case (CefMenuCommand)26502:
                                  browser.GetHost().CloseDevTools();
                                  break;
                              case (CefMenuCommand)26503:
                                  if (this.linkUrl.Length > 0)
                                      System.Windows.Clipboard.SetText(this.linkUrl);
                                  if (this.mt != ContextMenuMediaType.Image)
                                      break;
                                  System.Windows.Clipboard.SetText(this.sourceUrl);
                                  this.SaveImage(this.sourceUrl, ImageFormat.Bmp);
                                  break;
                              case (CefMenuCommand)26504:
                                  if (this.linkUrl.Length > 0)
                                      System.Windows.Clipboard.SetText(this.linkUrl);
                                  if (this.mt != ContextMenuMediaType.Image)
                                      break;
                                  System.Windows.Clipboard.SetText(this.sourceUrl);
                                  break;
                          }
                      }), true)
                        });
                    }
                }
                webBrowser.ContextMenu = menu;
            }));
            return true;
        }

        private static IEnumerable<Tuple<string, CefMenuCommand, bool>> GetMenuItems(IMenuModel model)
        {
            for (int i = 0; i < model.Count; ++i)
                yield return new Tuple<string, CefMenuCommand, bool>(model.GetLabelAt(i), model.GetCommandIdAt(i), model.IsEnabledAt(i));
        }

        public void SaveImage(string imageUrl, ImageFormat format)
        {
            WebClient webClient = new WebClient();
            Stream stream = webClient.OpenRead(imageUrl);
            try
            {
                Bitmap bitmap = new Bitmap(stream);
                string extensionFromFilePath = AnyConverter.Get_NameAndExtension_FromFilePath(imageUrl);
                Microsoft.Win32.SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Выберите папку сохранения";
                saveFileDialog.FileName = extensionFromFilePath;
                saveFileDialog.InitialDirectory = "C:\\Users";
                saveFileDialog.ShowDialog();
                ImageFormat format1;
                switch (AnyConverter.Get_Directory_Name_Extension_FromFilePath(saveFileDialog.FileName)[2])
                {
                    case "jpg":
                        format1 = ImageFormat.Jpeg;
                        break;
                    case "bmp":
                        format1 = ImageFormat.Bmp;
                        break;
                    default:
                        format1 = ImageFormat.Png;
                        break;
                }
                bitmap?.Save(saveFileDialog.FileName, format1);
                stream.Flush();
                stream.Close();
                webClient.Dispose();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
