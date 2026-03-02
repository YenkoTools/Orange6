## Git Tagging Script

The `git-tag.ps1` PowerShell script is a helper script designed to tag a Git repository using the annotated tag format. Below are the details of its functionality:

### Usage

```powershell
.\git-tag.ps1 -version <version> [-message <message>]    
```

### Parameters

- `-version`: The version number to tag the repository with.
- `-message`: The message to associate with the tag. If not provided, the script will use the version number as the message.    

### Example

```powershell
.\git-tag.ps1 -version 1.0.0 -message "Initial release"
```

### Notes

- The script will create an annotated tag with the specified version number and message.
- If a tag with the same version number already exists, the script will prompt the user to confirm overwriting the existing tag.

### Requirements

- The script requires Git to be installed and available in the system's PATH.





