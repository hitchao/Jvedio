using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.UserControls
{


    public enum TabActionType
    {
        None = 0,
        Search = 1,
        NextPage,
        PreviousPage,
        FirstPage,
        LastPage,
        GoToTop,
        GoToBottom
    }


    internal interface ITabItemControl
    {
        void Refresh();
        void SetSearchFocus();
        void NextPage();
        void PreviousPage();
        void GoToTop();
        void GoToBottom();
        void FirstPage();
        void LastPage();
    }
}
