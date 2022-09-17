using SuperUtils.Reflections;

namespace Jvedio.Entity
{
    public class Serilizable
    {
        public override string ToString()
        {
            return ClassUtils.ToString(this);
        }
    }
}
