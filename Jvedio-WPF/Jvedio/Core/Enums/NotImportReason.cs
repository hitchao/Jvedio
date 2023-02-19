namespace Jvedio.Core.Enums
{
    public enum NotImportReason
    {
        NotInExtension,
        SizeTooSmall,
        SizeTooLarge,

        /// <summary>
        /// 重复的 hash
        /// </summary>
        RepetitiveVideo,

        /// <summary>
        /// 重复的 VID
        /// </summary>
        RepetitiveVID,
    }
}
