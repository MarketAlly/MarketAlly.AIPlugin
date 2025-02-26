# File Workflow Plugin

The `FileWorkflow` plugin provides a streamlined way to apply tracked changes to a file. This plugin bridges the gap between presenting changes (via `PresentAnswer`) and actually applying those changes to the file system.

## Overview

The `FileWorkflow` plugin takes a file path and a collection of line changes (similar to what you would provide to `PresentAnswer`), and applies those changes to create a modified version of the file. The plugin can either return the modified content or update the file directly.

## Use Cases

This plugin is particularly useful for:

1. Applying suggested code changes to files
2. Creating a proper diff-based workflow for code modifications
3. Automating file updates with proper change tracking
4. Recording what changes were made to files

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| filePath | string | Yes | Full path to the file to modify |
| lineChanges | Dictionary<int, LineChange> | Yes | Dictionary of line changes with line numbers as keys |
| createBackup | bool | No | Whether to create a backup of the original file (default: true) |
| updateFile | bool | No | Whether to update the file in place (true) or just return the modified content (false) (default: false) |
| headerComment | string | No | Comment to add at the top of the file (optional) |

## Example Function Calls

### Apply changes but don't modify the file yet:

```json
{
  "name": "FileWorkflow",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Services/UserService.cs",
    "lineChanges": {
      "10": {
        "changeType": "Context",
        "content": "public class UserService"
      },
      "11": {
        "changeType": "Context",
        "content": "{"
      },
      "15": {
        "changeType": "Modified",
        "content": "    public async Task<User> GetUserByIdAsync(int userId)",
        "originalContent": "    public User GetUserById(int userId)"
      },
      "16": {
        "changeType": "Added",
        "content": "    {"
      },
      "17": {
        "changeType": "Added",
        "content": "        if (userId <= 0) throw new ArgumentException(\"User ID must be positive\", nameof(userId));"
      },
      "18": {
        "changeType": "Added",
        "content": ""
      },
      "19": {
        "changeType": "Added",
        "content": "        try"
      },
      "20": {
        "changeType": "Added",
        "content": "        {"
      },
      "21": {
        "changeType": "Modified",
        "content": "            return await _repository.GetUserByIdAsync(userId);",
        "originalContent": "        return _repository.GetUserById(userId);"
      },
      "22": {
        "changeType": "Added",
        "content": "        }"
      },
      "23": {
        "changeType": "Added",
        "content": "        catch (Exception ex)"
      },
      "24": {
        "changeType": "Added",
        "content": "        {"
      },
      "25": {
        "changeType": "Added",
        "content": "            _logger.LogError(ex, $\"Error retrieving user {userId}\");"
      },
      "26": {
        "changeType": "Added",
        "content": "            throw;"
      },
      "27": {
        "changeType": "Added",
        "content": "        }"
      },
      "28": {
        "changeType": "Added",
        "content": "    }"
      }
    },
    "updateFile": false
  }
}
```

### Apply changes and update the file:

```json
{
  "name": "FileWorkflow",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Services/UserService.cs",
    "lineChanges": {
      "15": {
        "changeType": "Modified",
        "content": "    public async Task<User> GetUserByIdAsync(int userId)",
        "originalContent": "    public User GetUserById(int userId)"
      },
      "16": {
        "changeType": "Added",
        "content": "    {"
      },
      "17": {
        "changeType": "Added",
        "content": "        if (userId <= 0) throw new ArgumentException(\"User ID must be positive\", nameof(userId));"
      }
      // Additional changes...
    },
    "createBackup": true,
    "updateFile": true,
    "headerComment": "// Modified by AI Assistant on 2025-02-26 to add proper async/await and error handling"
  }
}
```

## Response Format

The plugin returns a result with the following structure:

```json
{
  "FilePath": "C:/Projects/MyApp/Services/UserService.cs",
  "ModifiedContent": "// Modified by AI Assistant on 2025-02-26 to add proper async/await and error handling\npublic class UserService\n{\n    public async Task<User> GetUserByIdAsync(int userId)\n    {\n        // ... rest of modified file content\n    }\n}",
  "ChangeStats": {
    "OriginalLineCount": 42,
    "ModifiedLineCount": 53,
    "LinesAdded": 13,
    "LinesModified": 2,
    "LinesDeleted": 0,
    "TotalChanges": 15
  },
  "FileUpdated": true,
  "Success": true
}
```

## Complete Workflow with All Plugins

Here's a complete workflow that uses all the plugins together:

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

2. **Present the suggested changes to the user**:

```json
{
  "name": "PresentAnswer",
  "arguments": {
    "message": "I've improved the UserService class by adding proper async/await patterns, parameter validation, and error handling.",
    "probability": 0.95,
    "fileType": "csharp",
    "partialContent": {
      "15": {
        "changeType": "Modified",
        "content": "    public async Task<User> GetUserByIdAsync(int userId)",
        "originalContent": "    public User GetUserById(int userId)"
      },
      "16": {
        "changeType": "Added",
        "content": "    {"
      }
      // Additional changes...
    },
    "summary": "Added async/await, parameter validation, and error handling to UserService methods."
  }
}
```

3. **Apply the changes to the file**:

```json
{
  "name": "FileWorkflow",
  "arguments": {
    "filePath": "C:/Projects/MyApp/Services/UserService.cs",
    "lineChanges": {
      // Same line changes as used in PresentAnswer
      "15": {
        "changeType": "Modified",
        "content": "    public async Task<User> GetUserByIdAsync(int userId)",
        "originalContent": "    public User GetUserById(int userId)"
      },
      "16": {
        "changeType": "Added",
        "content": "    {"
      }
      // Additional changes...
    },
    "updateFile": true,
    "headerComment": "// Modified by AI Assistant on 2025-02-26"
  }
}
```

4. **Store the result for future reference**:

```json
{
  "name": "PresentAnswer",
  "arguments": {
    "message": "I've successfully updated the UserService.cs file with the changes we discussed.",
    "probability": 1.0,
    "summary": "Updated UserService.cs with async/await patterns, parameter validation, and error handling. Made a total of 15 changes (13 additions, 2 modifications, 0 deletions)."
  }
}
```

## Best Practices

1. **Always use `ReadFile` first** to get the current state of the file.

2. **Present changes with `PresentAnswer` before applying them** to allow the user to review the changes.

3. **Set `updateFile: false` initially** to preview changes before committing them.

4. **Keep `createBackup: true`** when actually updating files to ensure data safety.

5. **Use the same line changes in both `PresentAnswer` and `FileWorkflow`** to maintain consistency.

6. **Include a header comment** to indicate when and why the file was modified.

7. **Use the change statistics** in your final answer to inform the user about the scope of changes.