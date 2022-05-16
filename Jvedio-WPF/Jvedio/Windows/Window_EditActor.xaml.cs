using ChaoControls.Style;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Utils;
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
        private Window_EditActor()
        {
            InitializeComponent();
        }

        public long ActorID { get; set; }
        public ActorInfo CurrentActorInfo { get; set; }

        public Window_EditActor(long actorID) : this()
        {
            this.ActorID = actorID;
            this.DataContext = this;
            CurrentActorInfo = new ActorInfo();
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
            if (string.IsNullOrEmpty(CurrentActorInfo.ActorName))
            {
                MessageCard.Error("演员名称不可为空！");
                return;
            }
            if (ActorID > 0)
            {
                int update = actorMapper.updateById(CurrentActorInfo);
                if (update > 0) MessageCard.Success(Jvedio.Language.Resources.Message_Success);
            }
            else
            {
                // 新增
                // 检查是否存在
                SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
                wrapper.Eq("ActorName", CurrentActorInfo.ActorName.ToProperSql());
                ActorInfo actorInfo = actorMapper.selectOne(wrapper);
                bool insert = true;
                if (actorInfo != null && !string.IsNullOrEmpty(actorInfo.ActorName))
                {
                    insert = (bool)new Msgbox(this, $"数据库中已有和 {actorInfo.ActorName} 同名的演员，是否继续添加？").ShowDialog();
                }
                if (insert)
                {
                    actorMapper.insert(CurrentActorInfo);
                    if (CurrentActorInfo.ActorID > 0)
                    {
                        MessageCard.Success("成功添加！");
                        this.DialogResult = true;
                    }
                    else
                        MessageCard.Success("添加失败！");
                }
            }

        }
    }
}
