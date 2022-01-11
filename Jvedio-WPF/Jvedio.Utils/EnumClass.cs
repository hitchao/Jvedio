using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils
{
    #region "enum"
    public enum ViewType { 默认, 有图, 无图 }
    public enum MySearchType { 识别码, 名称, 演员 }
    public enum MyImageType { 缩略图, 海报图, 预览图, 动态图, 列表模式 }
    public enum MovieStampType { 无, 高清中字, 无码流出 }

    public enum VedioType { 所有, 步兵, 骑兵, 欧美 }

    public enum ImageType { SmallImage, BigImage, ExtraImage, ActorImage }

    public enum JvedioWindowState { Normal, Minimized, Maximized, FullScreen, None }

    public enum WebSite { Bus, BusEu, Library, DB, FC2, Jav321, DMM, MOO, None }

    public enum Skin { 黑色, 白色, 蓝色 }

    public enum MyLanguage { 中文, English, 日本語 }

    public enum Sort { 识别码, 文件大小, 创建时间, 导入时间, 喜爱程度, 名称, 访问次数, 发行日期, 评分, 时长, 演员 }
    #endregion
}
