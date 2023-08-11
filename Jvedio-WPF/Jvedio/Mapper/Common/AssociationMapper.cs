using Jvedio.Entity.Base;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper.BaseMapper;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Mapper
{
    public class AssociationMapper : BaseMapper<Association>
    {
        public List<Association> Associations { get; set; }

        public Dictionary<long, ListNode<long>> AdjacencyList { get; set; }

        public void InitAdjacencyList()
        {
            Associations = SelectList();
            AdjacencyList = new Dictionary<long, ListNode<long>>();

            // 构造邻接表
            // 邻接表适合存储稀疏图（顶点较多、边较少）
            if (Associations != null && Associations.Count > 0) {
                foreach (Association item in Associations) {
                    long dataId = item.MainDataID;
                    if (AdjacencyList.ContainsKey(dataId)) {
                        ListNode<long> node = AdjacencyList[dataId];
                        while (true) {
                            if (node.Next == null) {
                                node.Next = new ListNode<long>(item.SubDataID);
                                break;
                            } else {
                                node = node.Next;
                            }
                        }
                    } else {
                        ListNode<long> node = new ListNode<long>(item.SubDataID);
                        ListNode<long> head = new ListNode<long>(-1);
                        head.Next = node;
                        node.Head = head;
                        AdjacencyList.Add(dataId, node);
                    }
                }

                // 打印邻接矩阵
                // foreach (long key in AdjacencyList.Keys)
                // {
                //    StringBuilder builder = new StringBuilder();
                //    builder.Append($"{key}->");
                //    ListNode<long> head = AdjacencyList[key].Head;
                //    ListNode<long> node = head.Next;
                //    while (node != null)
                //    {
                //        builder.Append($"{node.Data}->");
                //        node = node.Next;
                //        if (node == null) builder.Remove(builder.Length - 2, 2);
                //    }
                //    Console.WriteLine(builder);
                // }
            }

            // Console.WriteLine();
        }

        public HashSet<long> GetAssociationDatas(long dataID)
        {
            InitAdjacencyList();
            Dictionary<long, ListNode<long>> dict = AdjacencyList;
            HashSet<long> set = new HashSet<long>();
            HashSet<long> foundList = new HashSet<long>();
            if (dict != null && dict.Keys.Count > 0) {
                FindAssocData(ref set, dict, dataID, ref foundList);
            }

            set.Remove(dataID);
            return set;
        }

        private void FindAssocData(ref HashSet<long> set, Dictionary<long, ListNode<long>> dict,
            long target, ref HashSet<long> foundList)
        {
            if (foundList.Contains(target))
                return;
            foreach (long key in dict.Keys) {
                if (key.Equals(target)) {
                    ListNode<long> head = dict[target].Head;
                    ListNode<long> node = head.Next;
                    while (node != null) {
                        set.Add(node.Data);
                        node = node.Next;
                    }
                } else {
                    // 遍历链表，找到符合 target 的节点
                    ListNode<long> head = dict[key].Head;
                    bool found = false;
                    ListNode<long> node = head.Next;
                    while (node != null) {
                        if (node.Data == target) {
                            found = true;
                            break;
                        } else {
                            node = node.Next;
                        }
                    }

                    if (found) {
                        set.Add(key);
                        node = head.Next;
                        while (node != null) {
                            set.Add(node.Data);
                            node = node.Next;
                        }
                    }
                }
            }

            foundList.Add(target);

            // bfs
            foreach (long item in set.ToArray()) {
                FindAssocData(ref set, dict, item, ref foundList);
            }
        }
    }
}
