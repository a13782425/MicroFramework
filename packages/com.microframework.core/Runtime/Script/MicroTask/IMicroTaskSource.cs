using System;


namespace MFramework.Task
{

    public interface IMicroTaskSource
    {

        MicroTaskStatus GetStatus(short token);

        void GetResult(short token);

        void OnCompleted(Action<object> continuation, object state, short token);

        bool TrySetCanceled(short token);

    }

    public interface IMicroTaskSource<out T> : IMicroTaskSource
    {
        new T GetResult(short token);
    }


    public interface IMicroTaskInstruction
    {
        bool IsCompleted();
    }

    public interface IMicroTaskInstruction<T> : IMicroTaskInstruction
    {
        T GetResult();
    }
}