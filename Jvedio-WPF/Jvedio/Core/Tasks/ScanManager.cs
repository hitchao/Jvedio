using SuperUtils.Framework.Tasks;

namespace Jvedio.Core.Tasks
{
    public class ScanManager : BaseManager
    {

        protected ScanManager() { }

        public new static ScanManager Instance { get; set; }

        public new static ScanManager CreateInstance()
        {
            if (Instance == null)
                Instance = new ScanManager();
            return Instance;
        }

        public override void AddToDispatcher(AbstractTask task)
        {

        }

        public override void ClearDispatcher()
        {

        }
    }
}
