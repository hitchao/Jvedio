using Jvedio.Core.CustomEventArgs;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using Jvedio.Mapper;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.GlobalMapper;

namespace Jvedio.Core.Scan
{
    public class GameScan : ScanTask
    {
        public GameScan(List<string> scanPaths, IEnumerable<string> fileExt = null) : base(scanPaths, null, fileExt)
        {
            pathDict = new Dictionary<string, List<string>>();
        }

        public Dictionary<string, List<string>> pathDict;



        private (List<Game>, List<string>) parseGame()
        {
            // 仅支持根目录
            List<Game> import = new List<Game>();
            List<string> notImport = new List<string>();
            if (pathDict == null || pathDict.Keys.Count == 0) return (null, null);
            foreach (string path in pathDict.Keys)
            {
                List<string> list = pathDict[path];

                Game game = new Game();
                game.DataType = Enums.DataType.Game;
                game.Title = Path.GetFileName(path);
                game.Path = getRealExe(list);
                game.Size = DirHelper.getDirSize(new DirectoryInfo(path));
                game.Hash = Jvedio.Utils.Encrypt.Encrypt.GetFileMD5(game.Path);// 计算哈希
                import.Add(game);
            }

            return (import, notImport);
        }


        private string getRealExe(List<string> exeList)
        {
            return exeList[0];
        }

        public override void doWrok()
        {
            Task.Run(() =>
            {
                stopwatch.Start();
                foreach (string path in ScanPaths)
                {
                    List<string> list = FileHelper.TryGetAllFiles(path, "*.exe").ToList();
                    if (list != null && list.Count > 0)
                        pathDict.Add(path, list);
                }

                try { CheckStatus(); }
                catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); return; }

                ScanHelper scanHelper = new ScanHelper();

                (List<Game> import, List<string> notImport) = parseGame();

                try { CheckStatus(); }
                catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); return; }

                handleImport(import);
                handleNotImport(notImport);


                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                ScanResult.ElapsedMilliseconds = ElapsedMilliseconds;
                Status = System.Threading.Tasks.TaskStatus.RanToCompletion;
            });
        }

        private void handleImport(List<Game> import)
        {

            string sql = GameMapper.BASE_SQL;
            sql = "select metadata.DataID,Hash,Path,GID " + sql;
            List<Dictionary<string, object>> list = gameMapper.select(sql);
            List<Game> existDatas = gameMapper.toEntity<Game>(list, typeof(Game).GetProperties(), false);
            // 1.1 不需要导入
            // 存在同路径、同哈希的 exe
            foreach (var item in import.Where(arg => existDatas.Where(t => arg.Path.Equals(t.Path) && arg.Hash.Equals(t.Hash)).Any()))
            {
                ScanResult.NotImport.Add(item.Path, "同路径、同哈希");
            }
            import.RemoveAll(arg => existDatas.Where(t => arg.Path.Equals(t.Path) && arg.Hash.Equals(t.Hash)).Any());
            // 1.2 需要 update
            // 哈希相同，路径不同
            List<Game> toUpdate = new List<Game>();
            foreach (Game game in import)
            {
                Game existData = existDatas.Where(t => game.Hash.Equals(t.Hash) && !game.Path.Equals(t.Path)).FirstOrDefault();
                if (existData != null)
                {
                    game.DataID = existData.DataID;
                    game.GID = existData.GID;
                    game.LastScanDate = DateHelper.Now();
                    toUpdate.Add(game);
                    ScanResult.Update.Add(game.Path);
                }

            }
            import.RemoveAll(arg => existDatas.Where(t => arg.Hash.Equals(t.Hash) && !arg.Path.Equals(t.Path)).Any());
            // 1.3 需要 update
            // 哈希不同，路径相同
            foreach (Game data in import)
            {
                Game existData = existDatas.Where(t => data.Path.Equals(t.Path) && !data.Hash.Equals(t.Hash)).FirstOrDefault();
                if (existData != null)
                {
                    data.DataID = existData.DataID;
                    data.GID = existData.GID;
                    data.LastScanDate = DateHelper.Now();
                    toUpdate.Add(data);
                    ScanResult.Update.Add(data.Path);
                }

            }
            import.RemoveAll(arg => existDatas.Where(t => arg.Path.Equals(t.Path) && !arg.Hash.Equals(t.Hash)).Any());



            // 1.3 需要 insert
            List<Game> toInsert = import;
            // 1.更新
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            metaDataMapper.updateBatch(toUpdateData, "Title", "Size", "Hash", "Path", "LastScanDate");

            // 2.导入
            foreach (Game data in toInsert)
            {
                data.DBId = GlobalConfig.Main.CurrentDBId;
                data.FirstScanDate = DateHelper.Now();
                data.LastScanDate = DateHelper.Now();
                ScanResult.Import.Add(data.Path);
            }


            List<MetaData> toInsertData = toInsert.Select(arg => arg.toMetaData()).ToList();
            if (toInsertData.Count <= 0) return;
            long.TryParse(metaDataMapper.insertAndGetID(toInsertData[0]).ToString(), out long before);
            toInsertData.RemoveAt(0);
            try
            {
                //开启事务，这样子其他线程就不能更新
                metaDataMapper.executeNonQuery("BEGIN TRANSACTION;");
                metaDataMapper.insertBatch(toInsertData);

            }
            catch (Exception ex)
            {
                Logger.LogD(ex);
                OnError(new MessageCallBackEventArgs(ex.Message));
            }
            finally
            {
                metaDataMapper.executeNonQuery("END TRANSACTION;");
            }

            // 处理 DataID
            foreach (Game data in toInsert)
            {
                data.DataID = before;
                before++;
            }
            try
            {

                gameMapper.executeNonQuery("BEGIN TRANSACTION;");//开启事务，这样子其他线程就不能更新
                gameMapper.insertBatch(toInsert);


            }
            catch (Exception ex)
            {
                Logger.LogD(ex);
                OnError(new MessageCallBackEventArgs(ex.Message));
            }
            finally
            {
                gameMapper.executeNonQuery("END TRANSACTION;");
            }
        }

        private void handleNotImport(List<string> notImport)
        {
            foreach (string path in notImport)
            {
                ScanResult.NotImport.Add(path, "不导入");
            }
        }

    }
}
