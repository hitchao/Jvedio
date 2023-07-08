using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.Media
{
    // todo 检视
    public class Gif
    {
        private string GifPath = string.Empty;

        public Gif(string path)
        {
            this.GifPath = path;
        }

        /// <summary>
        /// 获得帧的元数据
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public FrameMetadata GetFrameMetadata(BitmapFrame frame)
        {
            var metadata = (BitmapMetadata)frame.Metadata;
            var delay = TimeSpan.FromMilliseconds(100);
            var metadataDelay = metadata.GetQueryOrDefault("/grctlext/Delay", 10);
            if (metadataDelay != 0)
                delay = TimeSpan.FromMilliseconds(metadataDelay * 10);
            var frameMetadata = new FrameMetadata {
                Left = metadata.GetQueryOrDefault("/imgdesc/Left", 0),
                Top = metadata.GetQueryOrDefault("/imgdesc/Top", 0),
                Width = metadata.GetQueryOrDefault("/imgdesc/Width", frame.PixelWidth),
                Height = metadata.GetQueryOrDefault("/imgdesc/Height", frame.PixelHeight),
                Delay = delay,
            };
            return frameMetadata;
        }

        /// <summary>
        /// 根据上一帧计算当前帧的图像
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rawFrame"></param>
        /// <param name="metadata"></param>
        /// <param name="baseFrame"></param>
        /// <returns></returns>
        public BitmapSource MakeFrame(int width, int height, BitmapSource rawFrame, FrameMetadata metadata, BitmapSource baseFrame)
        {
            DrawingVisual visual = new DrawingVisual();
            using (var context = visual.RenderOpen()) {
                if (baseFrame != null) {
                    var fullRect = new Rect(0, 0, width, height);
                    context.DrawImage(baseFrame, fullRect);
                }

                var rect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
                context.DrawImage(rawFrame, rect);
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);

            var result = new WriteableBitmap(bitmap);

            if (result.CanFreeze && !result.IsFrozen)
                result.Freeze();
            return result;
        }

        public TimeSpan GetTotalDuration()
        {
            GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri(GifPath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var totalDuration = TimeSpan.Zero;
            if (decoder != null && decoder.Frames.Count > 0) {
                for (int i = 0; i < decoder.Frames.Count; i++) {
                    var metadata = GetFrameMetadata(decoder.Frames[i]);
                    totalDuration += metadata.Delay;
                }
            }

            return totalDuration;
        }

        /// <summary>
        /// 根据 GIF 的元数据信息，计算出所有帧的图像，并获得每一帧的间隔（GIF的每一帧是相对于上一帧改变的像素，是压缩过的）
        /// </summary>
        /// <returns></returns>
        public (List<BitmapSource> BitmapSources, List<TimeSpan> TimeSpans) GetAllFrame()
        {
            List<BitmapSource> bitmapSources = new List<BitmapSource>();
            List<TimeSpan> spans = new List<TimeSpan>();
            if (!File.Exists(GifPath))
                return (bitmapSources, spans);
            GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri(GifPath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            if (decoder != null && decoder.Frames.Count > 0) {
                int index = 0;
                BitmapSource baseFrame = null;
                foreach (var rawFrame in decoder.Frames) {
                    var metadata = GetFrameMetadata(decoder.Frames[index]);
                    int width = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Width", 0);
                    int height = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Height", 0);
                    var frame = MakeFrame(width, height, rawFrame, metadata, baseFrame);
                    baseFrame = frame;
                    bitmapSources.Add(frame);
                    spans.Add(metadata.Delay);
                    index++;
                }
            }

            return (bitmapSources, spans);
        }

        public BitmapSource GetFirstFrame()
        {
            GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri(GifPath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BitmapSource result = null;
            if (decoder != null && decoder.Frames.Count > 0) {
                var frame = decoder.Frames[0];
                var metadata = GetFrameMetadata(frame);
                int width = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Width", 0);
                int height = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Height", 0);
                result = MakeFrame(width, height, frame, metadata, frame);
            }

            return result;
        }

        public GifBitmapDecoder GetDecoder()
        {
            return new GifBitmapDecoder(new Uri(GifPath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
    }

    public class FrameMetadata
    {
        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public TimeSpan Delay { get; set; }
    }
}
