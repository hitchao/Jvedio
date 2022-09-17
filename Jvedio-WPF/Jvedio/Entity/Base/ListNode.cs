namespace Jvedio.Entity.Base
{
    public class ListNode<T>
    {
        public T Data { get; set; }

        public ListNode<T> Next { get; set; }

        public ListNode<T> Head { get; set; }

        public ListNode(T item)
        {
            Data = item;
        }
    }
}
