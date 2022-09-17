namespace Jvedio.Core.CustomTask
{
    public interface ITask
    {
        void Start();

        void Stop();

        void Pause();

        void Cancel();

        void Restart();

        void Finished();

    }
}
