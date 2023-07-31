using SuperUtils.Framework.ORM.Wrapper;
using System;

namespace Jvedio.Core.CustomEventArgs
{
    public class WrapperEventArg<T> : EventArgs
    {

        public WrapperEventArg() { }


        public WrapperEventArg(IWrapper<T> wrapper)
        {
            Wrapper = wrapper;
        }
        public IWrapper<T> Wrapper { get; set; }

        public string SQL { get; set; }
    }
}
