# Service

## Local Development — Extra Azure Functions URLs

When running the Azure Function App locally, the OpenAPI/Swagger extension exposes the following additional endpoints alongside your regular function routes.

---

### 1. OAuth2 Redirect Page

```
GET http://localhost:7001/api/oauth2-redirect.html
```

**Purpose:** This is a helper HTML page used internally by Swagger UI to complete OAuth 2.0 authorization flows. When you click **Authorize** in the Swagger UI and authenticate via an OAuth2 provider, the provider redirects the browser back to this URL with the authorization code or token. Swagger UI then extracts the token and uses it for subsequent API calls.

**How to use:** You do not navigate to this URL directly. It is invoked automatically by Swagger UI during the OAuth2 login process. To trigger it, open the Swagger UI, click the **Authorize** button, enter your credentials, and the redirect will happen behind the scenes.

---

### 2. OpenAPI Document

```
GET http://localhost:7001/api/openapi/{version}.{extension}
```

**Purpose:** Returns the raw OpenAPI specification document that describes all your function endpoints — their routes, HTTP methods, parameters, request bodies, and response schemas. This machine-readable document can be imported into tools like Postman, Azure API Management, or code generators.

**Supported values:**

| Placeholder   | Options              |
|---------------|----------------------|
| `{version}`   | `v2` (Swagger 2.0) or `v3` (OpenAPI 3.0) |
| `{extension}` | `json` or `yaml`     |

**Examples:**

```
GET http://localhost:7001/api/openapi/v2.json
GET http://localhost:7001/api/openapi/v3.yaml
```

**How to use:**
- Open the URL in a browser or HTTP client to inspect the spec.
- Import `http://localhost:7001/api/openapi/v3.json` into Postman via **File → Import → Link** to auto-generate a collection.
- Use the doc with tools like `openapi-generator` or `nswag` to generate client SDKs.

---

### 3. Swagger UI / Swagger Document

```
GET http://localhost:7001/api/swagger.{extension}
```

**Purpose:** Provides the interactive Swagger UI — a browser-based interface that lets you explore and test your API endpoints without any external tooling. It reads the OpenAPI spec and presents a human-friendly view with forms to fill in parameters and execute live requests.

**Supported values:**

| Placeholder   | Options       |
|---------------|---------------|
| `{extension}` | `ui` (interactive HTML page), `json`, or `yaml` |

**Examples:**

```
GET http://localhost:7001/api/swagger.ui      ← Interactive Swagger UI
GET http://localhost:7001/api/swagger.json    ← Raw Swagger 2.0 spec as JSON
GET http://localhost:7001/api/swagger.yaml    ← Raw Swagger 2.0 spec as YAML
```

**How to use:**
1. Open `http://localhost:7001/api/swagger.ui` in a browser.
2. Browse the list of endpoints and expand any operation.
3. Click **Try it out**, fill in required parameters, and click **Execute** to send a live request and see the response.

---

## Summary

| URL | Use case |
|-----|----------|
| `/api/oauth2-redirect.html` | OAuth2 callback — used automatically by Swagger UI |
| `/api/openapi/v3.json` | Machine-readable spec for tooling (Postman, code gen) |
| `/api/swagger.ui` | Interactive browser-based API explorer and tester |
