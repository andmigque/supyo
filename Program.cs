// File: src/TeamsWebhookSender/Program.cs
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private const int DefaultTimeoutSeconds = 10;
    private const string contentTypeAppJason = "application/json";

    public static async Task<int> Main(string[] args)
    {
        var urlOption = new Option<string>("--url") {
            Description = "The Teams webhook URL"
        };
        var jsonOption = new Option<string>("--json") {
            Description = "Path to a JSON file for the payload"
        };
        var textOption = new Option<string>("--text") {
            Description = "Text to send"
        };
        var timeoutOption = new Option<int>("--timeout") {
            Description = "Timeout in seconds",
            DefaultValueFactory = _ => DefaultTimeoutSeconds
        };

        var rootCommand = new RootCommand
        {
            urlOption,
            jsonOption,
            textOption,
            timeoutOption
        };

        rootCommand.Description = "Teams Webhook Sender";

        rootCommand.SetAction(async context =>
        {
            
            var urlResult = context.CommandResult.GetValue(urlOption);
            var jsonResult = context.CommandResult.GetValue(jsonOption);
            var textResult = context.CommandResult.GetValue(textOption);
            var timeoutResult = context.CommandResult.GetValue(timeoutOption);

            var resolvedUrl = !string.IsNullOrWhiteSpace(urlResult)
                ? urlResult
                : Environment.GetEnvironmentVariable("TEAMS_WEBHOOK_URL") ?? "";

            var payloadJson = await BuildPayloadJsonAsync(jsonResult, textResult);

            isEmpty(resolvedUrl, "Url is null or empty");
            isEmpty(payloadJson, "Payload is null or empty");

            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutResult));
                using var httpClient = new HttpClient();
                using var jasonContent = new StringContent(payloadJson, Encoding.UTF8, contentTypeAppJason);

                var response = await httpClient.PostAsync(resolvedUrl, jasonContent, cancellationTokenSource.Token);
                var body = await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);

                var result = response.IsSuccessStatusCode
                    ? "OK"
                    : string.Format("HTTP {0} {1}{2}{3}", (int)response.StatusCode, response.ReasonPhrase, Environment.NewLine, body);

                (response.IsSuccessStatusCode ? Console.Out : Console.Error).WriteLine(result);
                Environment.Exit(response.IsSuccessStatusCode ? 0 : 1);
            }
            catch (TaskCanceledException)
            {
                Console.Error.WriteLine("Request timed out.");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        });
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static void isEmpty(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Console.Error.WriteLine(message);
            Environment.Exit(2);
        }
    }

    private static async Task<string> BuildPayloadJsonAsync(string? jsonPath, string? text)
    {
        if (!string.IsNullOrWhiteSpace(jsonPath))
        {
            return await File.ReadAllTextAsync(jsonPath);
        }

        string? resolvedText = !string.IsNullOrWhiteSpace(text)
            ? text
            : (Console.IsInputRedirected ? (await Console.In.ReadToEndAsync())?.Trim() : null);

        if (string.IsNullOrWhiteSpace(resolvedText))
            return "";

        var payload = new { text = resolvedText };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
    }
}
