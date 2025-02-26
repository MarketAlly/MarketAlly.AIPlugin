# File Operations Plugins Guide

This guide explains how to use the file-related plugins to read, create, update, and delete files. These plugins are designed to work together with the `PresentAnswer` plugin to provide a complete workflow for the AI.

## Available Plugins

### 1. ReadFile Plugin

The `ReadFile` plugin allows the AI to read existing files from the file system, with options to format the output in the most suitable way for analysis.

### 2. FileOperations Plugin

The `FileOperations` plugin enables the AI to create new files, update existing files, or delete files from the file system.

## Workflow for Code Modifications

Here's the recommended workflow for the AI when working with code files:

1. **Read the original file** using the `ReadFile` plugin
2. **Analyze the code** and determine necessary changes
3. **Present the changes** using the `PresentAnswer` plugin with change tracking
4. **Create or update files** using the `FileOperations` plugin when needed

## ReadFile Plugin Usage

### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| filePath | string | Yes | Full path to the file to read |
| returnWithLineNumbers | bool | No | Whether to return the content as lines with line numbers (true) or as a single string (false) |
| maxLines | int | No | Maximum number of lines to return (0 for unlimited) |
| lineRanges | string | No | Whether to include only specific line ranges, format: '1-10,15,20-25' |

### Example Function Calls

#### Reading an entire file with line numbers:

```json
{
  "name": "ReadFile",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Controllers/UserController.cs",
    "returnWithLineNumbers": true
  }
}
```

#### Reading specific parts of a large file:

```json
{
  "name": "ReadFile",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Services/DataService.cs",
    "returnWithLineNumbers": true,
    "lineRanges": "50-100,150-200"
  }
}
```

#### Reading the first 50 lines of a file as a single string:

```json
{
  "name": "ReadFile",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Program.cs",
    "returnWithLineNumbers": false,
    "maxLines": 50
  }
}
```

### Response Format

When `returnWithLineNumbers` is `true`:

```json
{
  "FileName": "UserController.cs",
  "FileType": "cs",
  "FileSizeBytes": 5431,
  "TotalLines": 210,
  "IncludedLines": 210,
  "Content": {
    "1": "using System;",
    "2": "using System.Collections.Generic;",
    "3": "using Microsoft.AspNetCore.Mvc;",
    // ... more lines
  }
}
```

When `returnWithLineNumbers` is `false`:

```json
{
  "FileName": "UserController.cs",
  "FileType": "cs",
  "FileSizeBytes": 5431,
  "Content": "using System;\nusing System.Collections.Generic;\nusing Microsoft.AspNetCore.Mvc;\n// ... rest of file"
}
```

## FileOperations Plugin Usage

### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| operation | string | Yes | The operation to perform (create, update, delete) |
| filePath | string | Yes | Full path to the file |
| content | string | For create/update | Content to write to the file |
| createDirectories | bool | No | Whether to create the directory structure if it doesn't exist (default: true) |
| overwrite | bool | No | Whether to overwrite existing files (default: false) |
| makeBackup | bool | No | Whether to make a backup before updating or deleting (default: true) |

### Example Function Calls

#### Creating a new file:

```json
{
  "name": "FileOperations",
  "arguments": {
    "operation": "create",
    "filePath": "C:/Projects/MyApp/Models/UserProfile.cs",
    "content": "using System;\n\nnamespace MyApp.Models\n{\n    public class UserProfile\n    {\n        public int Id { get; set; }\n        public string Username { get; set; }\n    }\n}"
  }
}
```

#### Updating an existing file:

```json
{
  "name": "FileOperations",
  "arguments": {
    "operation": "update",
    "filePath": "C:/Projects/MyApp/Models/UserProfile.cs",
    "content": "using System;\n\nnamespace MyApp.Models\n{\n    public class UserProfile\n    {\n        public int Id { get; set; }\n        public string Username { get; set; }\n        public string Email { get; set; }\n    }\n}",
    "makeBackup": true
  }
}
```

#### Deleting a file:

```json
{
  "name": "FileOperations",
  "arguments": {
    "operation": "delete",
    "filePath": "C:/Projects/MyApp/Models/OldModel.cs",
    "makeBackup": true
  }
}
```

### Response Format

For create/update operations:

```json
{
  "Operation": "create",
  "FilePath": "C:/Projects/MyApp/Models/UserProfile.cs",
  "FileSize": 185,
  "Success": true,
  "Message": "File created successfully: C:/Projects/MyApp/Models/UserProfile.cs"
}
```

For delete operations:

```json
{
  "Operation": "delete",
  "FilePath": "C:/Projects/MyApp/Models/OldModel.cs",
  "Success": true,
  "Message": "File deleted successfully: C:/Projects/MyApp/Models/OldModel.cs"
}
```

## Complete Workflow Example

Here's a complete workflow example for the AI to analyze and modify a file:

1. **Read the original file**:

```json
{
  "name": "ReadFile",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Services/UserService.cs",
    "returnWithLineNumbers": true
  }
}
```

2. **Present the changes**:

```json
{
  "name": "PresentAnswer",
  "arguments": {
    "message": "I've improved error handling in the UserService by adding proper parameter validation and try/catch blocks.",
    "probability": 0.95,
    "fileType": "csharp",
    "partialContent": {
      "24": { "changeType": "Context", "content": "public async Task<User> GetUserByIdAsync(int userId)" },
      "25": { "changeType": "Context", "content": "{" },
      "26": { "changeType": "Added", "content": "    if (userId <= 0) throw new ArgumentException(\"User ID must be positive\", nameof(userId));" },
      // ... more changes
    },
    "summary": "Added parameter validation and exception handling to UserService methods."
  }
}
```

3. **Update the file**:

```json
{
  "name": "FileOperations",
  "arguments": {
    "operation": "update",
    "filePath": "C:/Projects/MyApp/Services/UserService.cs",
    "content": "// Updated file content with all changes applied"
  }
}
```

## Best Practices

1. **Always read the file first** before suggesting changes.

2. **Use line numbers when reading files** to make it easier to reference specific lines.

3. **Present changes with appropriate change types** before modifying the actual file.

4. **Keep backups enabled** when updating or deleting files.

5. **Use lineRanges parameter** when working with large files to reduce the amount of data transferred.

6. **Store the file ID from ReadFile responses** to reference it in later operations.

7. **Use the summary field** in PresentAnswer to track what files were modified and why.