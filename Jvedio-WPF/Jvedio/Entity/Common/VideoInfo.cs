namespace Jvedio.Entity
{
    /// <summary>
    /// 视频信息
    /// </summary>
    public class VideoInfo
    {
        /* 视频信息 */
        public string Format { get; set; }// 视频格式

        public string BitRate { get; set; }// 总码率

        public string Duration { get; set; }

        public string FileSize { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public string Resolution { get; set; }

        public string DisplayAspectRatio { get; set; }// 宽高比

        public string FrameRate { get; set; }// 帧率

        public string BitDepth { get; set; }// 位深度

        public string PixelAspectRatio { get; set; }// 像素宽高比

        public string Encoded_Library { get; set; }// 编码库

        public string FrameCount { get; set; }// 总帧数

        /* 音频信息 */
        public string AudioFormat { get; set; }

        public string AudioBitRate { get; set; }// 码率

        public string AudioSamplingRate { get; set; }// 采样率

        public string Channel { get; set; }// 声道数

        public string Extension { get; set; }

        public string FileName { get; set; }

        public VideoInfo()
        {
            foreach (var item in typeof(VideoInfo).GetProperties()) {
                item.SetValue(this, string.Empty);
            }
        }
    }
}
