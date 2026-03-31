using System.Net;
using System.Text;
using System.Text.Json;

namespace Hoard.Mcp;

public class HoardApiClient(HttpClient http)
{
    public async Task<string> GetAsync(string path, CancellationToken ct = default)
    {
        var response = await http.GetAsync(path, ct);
        return await HandleResponse(response, ct);
    }

    public async Task<string> PostAsync(string path, object body, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await http.PostAsync(path, content, ct);
        return await HandleResponse(response, ct);
    }

    public async Task<string> PutAsync(string path, object body, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await http.PutAsync(path, content, ct);
        return await HandleResponse(response, ct);
    }

    public async Task<string> DeleteAsync(string path, CancellationToken ct = default)
    {
        var response = await http.DeleteAsync(path, ct);
        return await HandleResponse(response, ct);
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
