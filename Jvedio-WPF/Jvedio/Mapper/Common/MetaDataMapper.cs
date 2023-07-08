using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Mapper.BaseMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jvedio.Mapper
{
    public class MetaDataMapper : BaseMapper<MetaData>
    {
        public int deleteDataByIds(List<string> idList)
        {
            if (idList == null || idList.Count == 0)
                return 0;
            int c1 = MapperManager.metaDataMapper.DeleteByIds(idList);
            int c2 = 0;
            DataType dataType = Main.CurrentDataType;
            if (dataType == DataType.Picture) {
                c2 = MapperManager.pictureMapper.DeleteByIds(idList);
            } else if (dataType == DataType.Comics) {
                c2 = MapperManager.comicMapper.DeleteByIds(idList);
            } else if (dataType == DataType.Game) {
                c2 = MapperManager.gameMapper.DeleteByIds(idList);
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

            if (dataType == DataType.Picture) {
                MapperManager.pictureMapper.ExecuteNonQuery(builder.ToString());
            } else if (dataType == DataType.Comics) {
                MapperManager.comicMapper.ExecuteNonQuery(builder.ToString());
            } else if (dataType == DataType.Game) {
                MapperManager.gameMapper.ExecuteNonQuery(builder.ToString());
            }

            if (c1 == c2 && idList.Count == c1)
                return c1;
            else {
                // todo 日志
            }

            return 0;
        }

        public void SaveLabel(MetaData metaData)
        {
            string selectSql = $"select LabelName from metadata_to_label where DataID={metaData.DataID}";
            List<Dictionary<string, object>> list = Select(selectSql);
            List<string> labels = list.Select(arg => arg["LabelName"].ToString()).ToList();
            List<string> oldLabels = new List<string>();
            List<string> newLabels = new List<string>();
            if (metaData.LabelList != null)
                newLabels = metaData.LabelList.Select(arg => arg.Value).ToList();
            if (labels != null)
                oldLabels = labels.ToList();

            if (newLabels.SequenceEqual(oldLabels))
                return;

            // 删除，新增
            oldLabels = oldLabels.OrderBy(arg => arg).ToList();
            newLabels = newLabels.OrderBy(arg => arg).ToList();
            List<string> to_delete = oldLabels.Except(newLabels).ToList();
            List<string> to_create = newLabels.Except(oldLabels).ToList();

            // 删除
            if (to_delete.Count > 0) {
                // ('1','2','3')
                string sql = $"delete from metadata_to_label " +
                    $"where DataID={metaData.DataID} " +
                    $"and LabelName in ('{string.Join("','", to_delete)}')";
                ExecuteNonQuery(sql);
            }

            // 新增
            if (to_create.Count > 0) {
                List<string> create = new List<string>();
                to_create.ForEach(arg => {
                    create.Add($"({metaData.DataID},'{arg}')");
                });

                string sql = $"insert or ignore into metadata_to_label(DataID,LabelName) " +
                    $"values {string.Join(",", create)}";
                ExecuteNonQuery(sql);
            }
        }
    }
}
