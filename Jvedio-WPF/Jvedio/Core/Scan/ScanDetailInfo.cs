namespace Jvedio.Core.Scan
{
    public class ScanDetailInfo
    {
        public string Reason { get; set; }
        public string Detail { get; set; }
        public ScanDetailInfo(string reason, string detail = "")
        {
            Reason = reason;
            Detail = detail;
        }

        public ScanDetailInfo()
        {
            Reason = "";
            Detail = "";
        }
    }
}
