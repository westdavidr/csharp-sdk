using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MultiMcpServer.Tools;

/// <summary>
/// Tools for the utility server instance
/// </summary>
[McpServerToolType]
public class UtilityTools
{
    [McpServerTool(Name = "echo"), Description("Echo back the input text")]
    public static string Echo(string text)
    {
        return $"Echo: {text}";
    }

    [McpServerTool(Name = "timestamp"), Description("Get current timestamp")]
    public static string GetTimestamp()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
    }

    [McpServerTool(Name = "uuid"), Description("Generate a new UUID")]
    public static string GenerateUuid()
    {
        return Guid.NewGuid().ToString();
    }

    [McpServerTool(Name = "encode_base64"), Description("Encode text to Base64")]
    public static string EncodeBase64(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes);
    }

    [McpServerTool(Name = "decode_base64"), Description("Decode Base64 to text")]
    public static string DecodeBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}