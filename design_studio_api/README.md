# UI Code Generator Backend

ASP.NET Core Web API backend for a three-section UI builder:

- Prompt section sends a natural-language UI request.
- Canvas section receives structured preview data to render.
- Code section receives generated source files/text.

The backend has no database dependency. It calls DeepSeek's OpenAI-compatible Chat Completions API and asks the model to return strict JSON containing both preview metadata and source code.

## DeepSeek API Note

DeepSeek's official API docs currently list:

- OpenAI-compatible base URL: `https://api.deepseek.com`
- Current model IDs: `deepseek-v4-flash` and `deepseek-v4-pro`
- Legacy aliases `deepseek-chat` and `deepseek-reasoner` are marked for deprecation on 2026-07-24
- JSON output is supported through `response_format: { "type": "json_object" }`

This project defaults to `deepseek-v4-flash` because it is the lowest-cost current API model. DeepSeek API access uses account balance or granted/free-trial balance if available; the public chatbot being free does not mean the API is unlimited/free.

References:

- https://api-docs.deepseek.com/
- https://api-docs.deepseek.com/api/create-chat-completion
- https://api-docs.deepseek.com/quick_start/pricing/
- https://api-docs.deepseek.com/quick_start/error_codes

## Folder Structure

```text
.
|-- UiCodeGenerator.sln
|-- Directory.Build.props
|-- NuGet.config
|-- docs/
|   |-- examples/
|   |   |-- generate-ui-request.json
|   |   |-- generate-ui-response.json
|   |   `-- postman-collection.json
`-- src/
    |-- UiCodeGenerator.Api/
    |   |-- Controllers/
    |   |-- Middleware/
    |   |-- Program.cs
    |   |-- appsettings.json
    |   `-- UiCodeGenerator.Api.http
    |-- UiCodeGenerator.Application/
    |   |-- Abstractions/
    |   |-- DTOs/
    |   |-- Exceptions/
    |   `-- Services/
    |-- UiCodeGenerator.Domain/
    |   `-- Models/
    `-- UiCodeGenerator.Infrastructure/
        |-- DeepSeek/
        `-- DependencyInjection.cs
```

## Architecture

- `UiCodeGenerator.Api`: HTTP endpoints, CORS, Swagger, middleware, and API examples.
- `UiCodeGenerator.Application`: request/response DTOs, validation, use-case service, and AI abstraction.
- `UiCodeGenerator.Domain`: provider-neutral UI preview and source-code models.
- `UiCodeGenerator.Infrastructure`: DeepSeek API client and configuration options.

## Prerequisites

- .NET 8 SDK or newer
- A DeepSeek API key from https://platform.deepseek.com/api_keys

## Configuration

Set your DeepSeek key with user secrets:

```powershell
dotnet user-secrets set "DeepSeek:ApiKey" "YOUR_DEEPSEEK_API_KEY" --project .\src\UiCodeGenerator.Api
```

Or use an environment variable:

```powershell
$env:DeepSeek__ApiKey = "YOUR_DEEPSEEK_API_KEY"
```

Main settings live in `src/UiCodeGenerator.Api/appsettings.json`:

```json
{
  "DeepSeek": {
    "BaseUrl": "https://api.deepseek.com",
    "Model": "deepseek-v4-flash",
    "MaxTokens": 6000,
    "Temperature": 0.2,
    "EnableThinking": false,
    "ReasoningEffort": "high",
    "TimeoutSeconds": 90
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "http://localhost:4200"
    ]
  }
}
```

## Run Locally

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\UiCodeGenerator.Api
```

Default local URLs:

- HTTP: `http://localhost:5211`
- HTTPS: `https://localhost:7211`
- Swagger UI: `http://localhost:5211/swagger`
- Health check: `http://localhost:5211/health`

## API Endpoints

### Generate UI

`POST /api/ui-generations`

Request:

```json
{
  "prompt": "Create a responsive SaaS login screen with email, password, forgot password link, and primary sign in action.",
  "framework": "react",
  "styling": "tailwind",
  "componentName": "LoginScreen",
  "includeResponsiveLayout": true,
  "includeAccessibility": true
}
```

Response shape:

```json
{
  "id": "2b66db8c-78b0-4a65-88a8-c35b96303f44",
  "generatedAtUtc": "2026-05-17T16:45:00Z",
  "prompt": "Create a responsive SaaS login screen...",
  "framework": "react",
  "preview": {
    "title": "SaaS Login Screen",
    "description": "Centered authentication screen with form controls and supporting actions.",
    "layout": "centered-auth-card",
    "theme": {
      "primaryColor": "#2563eb",
      "backgroundColor": "#f8fafc",
      "textColor": "#0f172a",
      "fontFamily": "Inter, sans-serif"
    },
    "components": []
  },
  "sourceCode": {
    "language": "tsx",
    "framework": "react",
    "files": [
      {
        "path": "LoginScreen.tsx",
        "content": "export default function LoginScreen() { return <main>...</main>; }"
      }
    ]
  },
  "notes": []
}
```

### Example Payload

`GET /api/ui-generations/example`

Returns a sample request and response without calling DeepSeek.

### Health

`GET /health`

Returns a small health object for local checks and deployment probes.

## Test With Swagger

1. Run the API.
2. Open `http://localhost:5211/swagger`.
3. Expand `POST /api/ui-generations`.
4. Click `Try it out`.
5. Paste the request JSON from `docs/examples/generate-ui-request.json`.
6. Execute and inspect the preview/code response.

## Test With Postman

1. Import `docs/examples/postman-collection.json`.
2. Set the collection variable `baseUrl` to `http://localhost:5211`.
3. Run `Generate UI`.

The frontend never sends the DeepSeek API key. It only calls this backend; the backend reads the key from configuration.

## Error Handling

- Invalid frontend payloads return `400 ValidationProblemDetails`.
- Missing DeepSeek API key returns a `500 ProblemDetails` response in development with a clear configuration message.
- DeepSeek failures are logged and mapped to `429`, `502`, or `503` depending on the upstream status.
- All unhandled errors return standardized `application/problem+json`.

## Frontend Integration Later

Configure your frontend origin in `Cors:AllowedOrigins`, then call:

```ts
const response = await fetch("http://localhost:5211/api/ui-generations", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    prompt,
    framework: "react",
    styling: "tailwind",
    componentName: "GeneratedScreen",
    includeResponsiveLayout: true,
    includeAccessibility: true
  })
});

const generated = await response.json();
```
