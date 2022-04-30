using ChaoControls.Style;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Jvedio.GlobalMapper;

namespace Jvedio
{
    /// <summary>
    /// Window_EditActor.xaml 的交互逻辑
    /// </summary>
    public partial class Window_EditActor : BaseWindow
    {
        public Window_EditActor()
        {
            InitializeComponent();
        }

        public long ActorID { get; set; }
        public ActorInfo CurrentActorInfo { get; set; }

        public Window_EditActor(long actorID) : this()
        {
            this.ActorID = actorID;
            this.DataContext = this;
            LoadActor();
        }



        public void LoadActor()
        {
            if (this.ActorID <= 0) return;
            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            wrapper.Eq("ActorID", this.ActorID);
            ActorInfo actorInfo = actorMapper.selectById(wrapper);
            ActorInfo.SetImage(ref actorInfo);
            CurrentActorInfo = null;
            CurrentActorInfo = actorInfo;
        }

        private void BaseWindow_ContentRendered(object sender, EventArgs e)
        {

        }

        private void SaveActor(object sender, RoutedEventArgs e)
        {
            int update = actorMapper.updateById(CurrentActorInfo);
            if (update > 0) MessageCard.Success(Jvedio.Language.Resources.Message_Success);
        }
    }
}
