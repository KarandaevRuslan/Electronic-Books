

using CefSharp;
using ElectronicBooks.Heap.Other;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ElectronicBooks.Heap.Behaviors
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        public bool CanDownload(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          string url,
          string requestMethod)
        {
            if (!AnyConverter.IsUrlFileOnDisk(url))
                return true;
            string filePath = AnyConverter.EncodeAndDecodeUrl(url.Substring(8));
            List<string> extensionFromFilePath = AnyConverter.Get_Directory_Name_Extension_FromFilePath(filePath);
            string str = extensionFromFilePath[0] + "\\" + extensionFromFilePath[1] + ".pdf";
            AnyConverter.ConvertionVideoToWebm(filePath.Replace('/', '\\'));
            if (!AnyConverter.CanConvertFileToPDF(extensionFromFilePath[2]))
                return false;
            if (!AnyConverter.ShowMessageBox(extensionFromFilePath[2], "pdf"))
                return false;
            try
            {
                switch (extensionFromFilePath[2])
                {
                    case "doc":
                    case "docx":
                        var wordApp = new Microsoft.Office.Interop.Word.Application();
                        var wordDocument = wordApp.Documents.Open(filePath.Replace('/', '\\'));

                        wordDocument.ExportAsFixedFormat(str, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);

                        wordDocument.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges,
                                           Microsoft.Office.Interop.Word.WdOriginalFormat.wdOriginalDocumentFormat,
                                           false);

                        wordApp.Quit();
                        break;
                    case "ppt":
                    case "pptx":
                        var powerpointApp = new Microsoft.Office.Interop.PowerPoint.Application();

                        var powerpointDocument = powerpointApp.Presentations.Open(filePath.Replace('/', '\\'),
                                        Microsoft.Office.Core.MsoTriState.msoTrue,
                                        Microsoft.Office.Core.MsoTriState.msoFalse,
                                        Microsoft.Office.Core.MsoTriState.msoFalse);

                        powerpointDocument.ExportAsFixedFormat(str,
                                        Microsoft.Office.Interop.PowerPoint.PpFixedFormatType.ppFixedFormatTypePDF);

                        powerpointDocument.Close();
                        powerpointApp.Quit();
                        break;
                    case "xls":
                    case "xlsx":
                        var excelApp = new Microsoft.Office.Interop.Excel.Application();

                        var excelDocument = excelApp.Workbooks.Open(@filePath.Replace('/', '\\'));

                        excelDocument.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF,
                                                          str);

                        excelDocument.Close(false, "", false);
                        excelApp.Quit();
                        break;
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
            return false;
        }

        public void OnBeforeDownload(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          DownloadItem downloadItem,
          IBeforeDownloadCallback callback)
        {
            EventHandler<DownloadItem> beforeDownloadFired = this.OnBeforeDownloadFired;
            if (beforeDownloadFired != null)
                beforeDownloadFired((object)this, downloadItem);
            if (callback.IsDisposed)
                return;
            using (callback)
                callback.Continue(downloadItem.SuggestedFileName, true);
        }

        public void OnDownloadUpdated(
          IWebBrowser chromiumWebBrowser,
          IBrowser browser,
          DownloadItem downloadItem,
          IDownloadItemCallback callback)
        {
            EventHandler<DownloadItem> downloadUpdatedFired = this.OnDownloadUpdatedFired;
            if (downloadUpdatedFired == null)
                return;
            downloadUpdatedFired((object)this, downloadItem);
        }
    }
}
