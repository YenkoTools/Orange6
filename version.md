## Version Script

The `version.ps1` PowerShell script automatically generates `version.json` files for both the Client and Service applications. It captures build metadata including git information, build timestamps, and environment details.

### Usage

```powershell
.\version.ps1
```

### What It Does

The script performs the following operations:

1. **Fetches Git Tags**: Retrieves the latest git tags from the repository
2. **Collects Build Information**: Gathers version control and environment metadata
3. **Generates Build Number**: Creates a unique build number based on current date and time
4. **Creates version.json**: Writes version files to both Client and Service directories

### Output Locations

- **Client**: `Client/public/version.json`
- **Service**: `Service/src/Api/wwwroot/version.json`

### Generated version.json Structure

```json
{
  "BuildNumber": "20231217.845",
  "BuildDate": "2023-12-17T14:05:23+00:00",
  "BuildHost": "COMPUTER-NAME",
  "CommitHash": "a1b2c3d",
  "CurrentUser": "username",
  "GitBranch": "main",
  "GitHead": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0",
  "GitTag": "v1.0.0"
}
```

### Build Number Format

The build number follows the format `YYYYMMDD.Tick` where:
- `YYYYMMDD`: Current date (e.g., 20231217)
- `Tick`: Minutes elapsed since midnight (e.g., 845 = 2:05 PM)

### Example

```powershell
# Run from the Aem directory
cd Aem
.\version.ps1

# Output:
# Build Number: 20231217.845
# ✅ version.json written to C:\...\Client\public\version.json
# ✅ version.json written to C:\...\Service\src\Api\wwwroot\version.json
```

### Requirements

- **PowerShell 7+**: The script requires PowerShell version 7 or higher
- **Git**: Git must be installed and available in the system's PATH
- **Git Repository**: Must be run from within a Git repository with commit history

### Notes

- The script will create output directories if they don't exist
- If no tags are found, it defaults to `v0.0.0`
- The script attempts to fetch all tags including unshallow history
- In CI/CD environments, it can use the `GITHUB_REF` environment variable for tag detection
- Both Client and Service receive identical version information

### Use Cases

- **Local Development**: Track build versions during development
- **CI/CD Pipelines**: Automatically version builds in automated workflows
- **Production Deployments**: Verify deployed versions through the `/version.json` endpoint
- **Debugging**: Identify exact commit and build time for deployed applications


Hello Jim