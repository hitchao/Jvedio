using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomTask
{
    public interface ITask
    {
        void Start();
        void Stop();
        void Pause();
        void Cancel();

        void Finished();

    }
}
