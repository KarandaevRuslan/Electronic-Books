using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace ElectronicBooks.Views.Windows
{
    public partial class ConversionProgress : Window
    {
        private bool isFinish;
        private bool reached100;
        private List<string> IO;
        private CancellationTokenSource cancellationTokenSourse;

        public ConversionProgress(List<string> IO, string inputPath, string outputPath)
        {
            this.InitializeComponent();
            this.IO = new List<string>((IEnumerable<string>)IO);
            this.message.Text = "Конвертация файла \"" + IO[0] + "\" в файл \"" + IO[1] + "\".";
            this.Closing += new CancelEventHandler(this.OnClosingWindow);
            this.SetConversion(inputPath, outputPath);
        }

        private async void SetConversion(string inputPath, string outputPath)
        {
            cancellationTokenSourse = new CancellationTokenSource();
            Xabe.FFmpeg.IMediaInfo mediaInfo = await Xabe.FFmpeg.FFmpeg.GetMediaInfo(inputPath);
            IStream stream1 = (IStream)mediaInfo.VideoStreams.FirstOrDefault<IVideoStream>()?.SetCodec(VideoCodec.vp9);
            IStream stream2 = (IStream)mediaInfo.AudioStreams.FirstOrDefault<IAudioStream>()?.SetCodec(AudioCodec.libvorbis);
            IConversion conversion = Xabe.FFmpeg.FFmpeg.Conversions.New().AddStream<IStream>(stream2, stream1).SetOutput(outputPath);
            conversion.OnProgress += (sender, args) =>
            {
                var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetProgressValue(percent);
                }));
            };
            try
            {
                IConversionResult conversionResult = await conversion.Start(cancellationTokenSourse.Token);
            }
            catch (Exception ex)
            {
            }
        }

        private void OnClosingWindow(object sender, CancelEventArgs e)
        {
            if (this.isFinish)
                return;
            switch (MessageBox.Show("Конвертация еще не завершена. Вы хотите прервать конвертацию?", "Конвертация не завершена", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation))
            {
                case MessageBoxResult.None:
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                case MessageBoxResult.OK:
                    this.cancellationTokenSourse.Cancel();
                    break;
            }
        }

        public void SetProgressValue(int value)
        {
            this.pb.Value = (double)value;
            this.progress.Text = value.ToString() + " %";
            if (value != 100 || this.reached100)
                return;
            this.reached100 = true;
            int num = (int)MessageBox.Show("Файл \"" + this.IO[0] + "\" был сохранен как \"" + this.IO[1] + "\".", "Файл конвертирован", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            this.isFinish = true;
            this.Close();
        }
    }
}
