using SuperUtils.Reflections;

namespace Jvedio.Entity
{
    public class Serializable
    {
        public override string ToString()
        {
            return ClassUtils.ToString(this);
        }
    }
}
