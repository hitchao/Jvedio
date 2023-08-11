namespace Jvedio.Entity
{
    /// <summary>
    /// 视频信息
    /// </summary>
    public class VideoInfo
    {
        #region "属性"

        /// <summary>
        /// 视频格式
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 总码率
        /// </summary>
        public string BitRate { get; set; }

        public string Duration { get; set; }

        public string FileSize { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public string Resolution { get; set; }


        /// <summary>
        /// 宽高比
        /// </summary>
        public string DisplayAspectRatio { get; set; }

        /// <summary>
        /// 帧率
        /// </summary>
        public string FrameRate { get; set; }

        /// <summary>
        /// 位深度
        /// </summary>
        public string BitDepth { get; set; }


        /// <summary>
        /// 像素宽高比
        /// </summary>
        public string PixelAspectRatio { get; set; }


        /// <summary>
        /// 编码库
        /// </summary>
        public string Encoded_Library { get; set; }

        /// <summary>
        /// 总帧数
        /// </summary>
        public string FrameCount { get; set; }

        /// <summary>
        /// 音频信息
        /// </summary>
        public string AudioFormat { get; set; }

        /// <summary>
        /// 码率
        /// </summary>
        public string AudioBitRate { get; set; }


        /// <summary>
        /// 采样率
        /// </summary>
        public string AudioSamplingRate { get; set; }

        /// <summary>
        /// 声道数
        /// </summary>
        public string Channel { get; set; }

        public string Extension { get; set; }

        public string FileName { get; set; }
        #endregion
        public VideoInfo()
        {
            foreach (var item in typeof(VideoInfo).GetProperties()) {
                item.SetValue(this, string.Empty);
            }
        }
    }
}
