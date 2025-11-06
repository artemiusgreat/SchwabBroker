using System.Collections.Generic;

namespace Schwab.Extensions
{
  public static class HashtableExtensions
  {
    /// <summary>
    /// Access by key
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="input"></param>
    /// <param name="index"></param>
    public static V Get<K, V>(this IDictionary<K, V> input, K index)
    {
      return index is not null && input.TryGetValue(index, out var value) ? value : default;
    }
  }
}
