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


    /// <summary>
    /// 选项卡通用方法
    /// </summary>
    internal interface ITabItemControl
    {
        void Refresh(int page = -1);
        void SetSearchFocus();
        void NextPage();
        void PreviousPage();
        void GoToTop();
        void GoToBottom();
        void FirstPage();
        void LastPage();
    }
}
