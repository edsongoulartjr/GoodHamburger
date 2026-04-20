using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoodHamburger.Web;

public static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
