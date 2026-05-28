using System.Net;
using System.Text;
using System.Text.Json;

namespace Hoard.Mcp;

public class HoardApiClient(HttpClient http)
{
    public Task<string> GetAsync(string path, CancellationToken ct = default) =>
        SendAsync(() => http.GetAsync(path, ct), ct);

    public Task<string> PostAsync(string path, object body, CancellationToken ct = default) =>
        SendAsync(() => http.PostAsync(path, JsonBody(body), ct), ct);

    public Task<string> PutAsync(string path, object body, CancellationToken ct = default) =>
        SendAsync(() => http.PutAsync(path, JsonBody(body), ct), ct);

    public Task<string> DeleteAsync(string path, CancellationToken ct = default) =>
        SendAsync(() => http.DeleteAsync(path, ct), ct);

    private static StringContent JsonBody(object body) =>
        new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    private static async Task<string> SendAsync(Func<Task<HttpResponseMessage>> send, CancellationToken ct)
    {
        try
        {
            var response = await send();
            return await HandleResponse(response, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // The caller cancelled the request; let it propagate.
            throw;
        }
        catch (OperationCanceledException)
        {
            // Timed out via the resilience pipeline rather than caller cancellation.
            return "Error: the request to the Hoard API timed out. The service may be slow or unavailable.";
        }
        catch (HttpRequestException ex)
        {
            return $"Error: could not reach the Hoard API ({ex.Message}). The service may be unavailable.";
        }
        catch (Exception ex)
        {
            // Catch-all so a transport failure surfaces as a tool error rather than crashing the MCP server.
            return $"Error: the request to the Hoard API failed ({ex.Message}).";
        }
    }

    private static async Task<string> HandleResponse(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
            return string.IsNullOrWhiteSpace(body) ? $"{(int)response.StatusCode} {response.StatusCode}" : body;

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => "Not found.",
            _ => $"Error {(int)response.StatusCode}: {body}"
        };
    }

    public static string BuildQuery(params (string key, object? value)[] parameters)
    {
        var pairs = parameters
            .Where(p => p.value is not null)
            .Select(p => $"{Uri.EscapeDataString(p.key)}={Uri.EscapeDataString(p.value!.ToString()!)}");

        var query = string.Join("&", pairs);
        return string.IsNullOrEmpty(query) ? "" : "?" + query;
    }
}
