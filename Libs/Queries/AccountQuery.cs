namespace Schwab.Queries
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class AccountQuery
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("accountCode")]
    public string AccountCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("fields")]
    public IList<string> Fields { get; set; }
  }
}
