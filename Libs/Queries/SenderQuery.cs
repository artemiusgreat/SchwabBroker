namespace Schwab.Queries
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading;

  public partial class SenderQuery
  {
    public string Source { get; set; }
    public HttpMethod Action { get; set; }
    public object Content { get; set; }
    public CancellationToken Cleaner { get; set; }
    public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new();
  }
}
