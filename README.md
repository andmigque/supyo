# SUPYO
> A .NET 9 console app that likes to talk

Features
-	CLI for web hook posts
-	Supports plain text or custom JSON payloads.
-	Timeout configuration.
Prerequisites
-	.NET 9 SDK
-	Git
-	Visual Studio 202
Setup

```
git clone https://github.com/andmigque/supyo.git
cd NotifyTeams
```
Install packages
```
dotnet restore
```
Build the project
```
dotnet build -c Release
```
Build a single x64 exe
```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### Usage

#### Basic Example

Send a simple text message to a webhook endpoint:
```bash
NotifyTeams.exe --url "https://webhook.site/your-id" --text "Hello from NotifyTeams!"
```

#### Send a Custom JSON Payload

Create a file named `payload.json`:
```json
{
  "text": "Custom message"
}
```
Then run:
```bash
NotifyTeams.exe --url "https://webhook.site/your-id" --json "payload.json"
```

#### Pipe Input from Standard Input
```bash
echo "Hello from stdin" | NotifyTeams.exe --url "https://webhook.site/your-id"
```

### Timeout Configuration

Set a custom timeout (in seconds):
```bash
NotifyTeams.exe --url "https://webhook.site/your-id" --text "Test" --timeout 5
```

### Environment Variables

*   `TEAMS_WEBHOOK_URL`: If `--url` is not provided, this environment variable will be used.
*   `TEAMS_TIMEOUT_SECS`: If `--timeout` is not provided, this environment variable will be used.

### Notes

*   Build artifacts and IDE files are excluded via `.gitignore`.
*   For testing, you can use Webhook.site or any HTTP endpoint that accepts POST requests.
*   Incoming Webhooks for Microsoft Teams require a work/school tenant (not available in personal/communities accounts).

### Troubleshooting

*   If you encounter authentication issues with Git, ensure you use a GitHub personal access token for HTTPS remotes.
*   If the build produces only a `.dll`, check your `.csproj` for `<OutputType>Exe</OutputType>` and build for Windows.

### License

MIT