using Jvedio.Core.Enums;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class MetaDataMapper : BaseMapper<MetaData>
    {



        public int deleteDataByIds(List<string> idList)
        {
            if (idList == null || idList.Count == 0) return 0;
            int c1 = GlobalMapper.metaDataMapper.deleteByIds(idList);
            int c2 = 0;
            DataType dataType = GlobalVariable.CurrentDataType;
            if (dataType == DataType.Picture)
            {
                c2 = GlobalMapper.pictureMapper.deleteByIds(idList);
            }
            else if (dataType == DataType.Comics)
            {
                c2 = GlobalMapper.comicMapper.deleteByIds(idList);
            }
            else if (dataType == DataType.Game)
            {
                c2 = GlobalMapper.gameMapper.deleteByIds(idList);
            }



            StringBuilder builder = new StringBuilder();
            string ids = string.Join(",", idList);
            builder.Append("begin;");
            builder.Append($"delete from metadata_to_translation where DataID in ({ids});");
            builder.Append($"delete from metadata_to_tagstamp where DataID in ({ids});");
            builder.Append($"delete from metadata_to_actor where DataID in ({ids});");
            builder.Append($"delete from metadata_to_label where DataID in ({ids});");
            builder.Append($"delete from metadata_to_translation where DataID in ({ids});");
            builder.Append("commit;");


            if (dataType == DataType.Picture)
            {
                GlobalMapper.pictureMapper.executeNonQuery(builder.ToString());
            }
            else if (dataType == DataType.Comics)
            {
                GlobalMapper.comicMapper.executeNonQuery(builder.ToString());
            }
            else if (dataType == DataType.Game)
            {
                GlobalMapper.gameMapper.executeNonQuery(builder.ToString());
            }
            if (c1 == c2 && idList.Count == c1) return c1;
            else
            {
                // todo 日志

            }
            return 0;

        }

        public void SaveLabel(MetaData metaData, List<string> oldLabels)
        {
            List<string> newLabels = metaData.LabelList;
            if (oldLabels == null) oldLabels = new List<string>();
            if (newLabels == null) newLabels = new List<string>();
            if (newLabels.SequenceEqual(oldLabels)) return;
            // 删除，新增
            oldLabels = oldLabels.OrderBy(arg => arg).ToList();
            newLabels = newLabels.OrderBy(arg => arg).ToList();
            List<string> to_delete = oldLabels.Except(newLabels).ToList();
            List<string> to_create = newLabels.Except(oldLabels).ToList();

            //删除
            if (to_delete.Count > 0)
            {
                // ('1','2','3')
                string sql = $"delete from metadata_to_label " +
                    $"where DataID={metaData.DataID} " +
                    $"and LabelName in ('{string.Join("','", to_delete)}')";
                executeNonQuery(sql);
            }

            // 新增
            if (to_create.Count > 0)
            {
                List<string> create = new List<string>();
                to_create.ForEach(arg =>
                {
                    create.Add($"({metaData.DataID},'{arg}')");
                });

                string sql = $"insert or ignore into metadata_to_label(DataID,LabelName) " +
                    $"values {string.Join(",", create)}";
                executeNonQuery(sql);
            }
        }



    }
}
