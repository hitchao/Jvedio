using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.UserControls
{
    public interface IViewVideoFieldVisitor
    {
        void SetImageMode(int mode, int imageWidth);
    }
}
