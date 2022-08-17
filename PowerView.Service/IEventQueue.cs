
namespace PowerView.Service
{
    internal interface IEventQueue : IDisposable
  {
    void Enqueue(Action action);

    void InsertFirst(Action action);
  }
}

