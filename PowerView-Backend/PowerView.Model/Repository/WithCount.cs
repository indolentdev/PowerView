using System;

namespace PowerView.Model.Repository
{
  public class WithCount<TResult> where TResult: class
  {
    public WithCount(int totalCount, TResult result)
    {
      ArgumentNullException.ThrowIfNull(result);

      TotalCount = totalCount;
      Result = result;
    }

    public int TotalCount { get; private set; }
    public TResult Result { get; private set; }
  }
}
