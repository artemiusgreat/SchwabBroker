using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;

namespace Schwab.Services
{
  public class MapService
  {
    /// <summary>
    /// Serialization options
    /// </summary>
    public virtual JsonSerializerOptions Options { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public MapService()
    {
      Options = new JsonSerializerOptions
      {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
        {
          new Converters.MessageConverter<bool>(),
          new Converters.MessageConverter<byte>(),
          new Converters.MessageConverter<sbyte>(),
          new Converters.MessageConverter<short>(),
          new Converters.MessageConverter<ushort>(),
          new Converters.MessageConverter<int>(),
          new Converters.MessageConverter<uint>(),
          new Converters.MessageConverter<long>(),
          new Converters.MessageConverter<ulong>(),
          new Converters.MessageConverter<float>(),
          new Converters.MessageConverter<double>(),
          new Converters.MessageConverter<decimal>(),
          new Converters.MessageConverter<char>(),
          new Converters.MessageConverter<string>(),
          new Converters.MessageConverter<DateOnly>(),
          new Converters.MessageConverter<TimeOnly>(),
          new Converters.MessageConverter<DateTime>(),
          new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)
        },
        TypeInfoResolver = GetResolver()
      };
    }

    /// <summary>
    /// Create more resolver with more permissive naming policy for better matching
    /// </summary>
    protected virtual DefaultJsonTypeInfoResolver GetResolver() => new()
    {
      Modifiers =
      {
        contract => contract.Properties.ToList().ForEach(property =>
        {
          var name = Regex.Replace(property.Name,"(.)([A-Z])","$1_$2");

          if (string.Equals(name, property.Name))
          {
            return;
          }

          var o = contract.CreateJsonPropertyInfo(property.PropertyType, name);

          o.Set = property.Set;
          o.AttributeProvider = property.AttributeProvider;

          contract.Properties.Add(o);
        })
      }
    };
  }
}
