using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PowerView.Service
{
  internal class EventQueue : IEventQueue
  {
    private readonly ILogger logger;
    private readonly object eventsSyncRoot;
    private readonly List<Action> events;
    private Task itemTask;
    private volatile bool stop;

    public EventQueue(ILogger<EventQueue> logger)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      eventsSyncRoot = new object();
      events = new List<Action>();

      stop = false;
    }

    public void Enqueue(Action action)
    {
      lock (eventsSyncRoot)
      {
        events.Add(action);
        StartNextItemTaskIfAny();
      }
    }

    public void InsertFirst(Action action)
    {
      lock (eventsSyncRoot)
      {
        events.Insert(0, action);
        StartNextItemTaskIfAny();
      }
    }

    private void StartNextItemTaskIfAny()
    {
      if (!stop && itemTask == null && events.Count > 0)
      {
        itemTask = Task.Factory.StartNew(ProcessItemTask).ContinueWith(FinishProcessItemTask);
      }
    }

    private void ProcessItemTask()
    {
      Action action;
      lock (eventsSyncRoot)
      {
        action = events[0];
        events.RemoveAt(0);
      }
      action();
    }

    private void FinishProcessItemTask(Task processItemTask)
    {
      try
      {
        processItemTask.Wait(); // trigger exception - if any
      }
      catch (AggregateException e)
      {
        logger.LogError(e, "Exception occurred while processing event queue");
      }

      lock (eventsSyncRoot)
      {
        itemTask = null;
        StartNextItemTaskIfAny();
      }
    }

    #region IDisposable implementation
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~EventQueue() 
    {
      // Finalizer calls Dispose(false)
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) 
      {
        stop = true;

        Task localItemTask = null;
        lock (eventsSyncRoot)
        {
          if (itemTask != null)
          {
            localItemTask = itemTask;
          }
        }
        if (localItemTask != null)
        {
          localItemTask.Wait();
        }
        events.Clear();
      }
      // free native resources if there are any.
    }
    #endregion


  }
}

