# Using the PresentAnswer Plugin with Code Changes

This guide explains how to use the `PresentAnswer` plugin to present code changes with proper line change tracking.

## Overview

The `PresentAnswer` plugin now supports a structured format for presenting code changes. Each line in the `partialContent` parameter is marked with a specific change type:

- **Added**: New lines that were added to the file
- **Modified**: Existing lines that were changed (with optional original content)
- **Deleted**: Lines that were removed from the file
- **Context**: Unchanged lines included for context

## Plugin Schema

When the plugin is registered, it generates a schema that looks like this:

```json
{
  "name": "PresentAnswer",
  "description": "Returns a formatted answer with metadata and confidence score",
  "parameters": {
    "type": "object",
    "properties": {
      "message": {
        "type": "string",
        "description": "The answer message to present to the user"
      },
      "probability": {
        "type": "number",
        "description": "The probability/confidence score (0.0-1.0) that the answer is correct"
      },
      "fileType": {
        "type": "string",
        "description": "The file type if working with a specific file (e.g., 'csharp', 'python', 'json')"
      },
      "partialContent": {
        "type": "object",
        "description": "Partial content with line changes. Keys are line numbers, values describe what changed for each line.",
        "additionalProperties": {
          "type": "object",
          "properties": {
            "changeType": {
              "type": "string",
              "enum": ["Added", "Modified", "Deleted", "Context"],
              "description": "The type of change made to this line"
            },
            "content": {
              "type": "string",
              "description": "The content of the line"
            },
            "originalContent": {
              "type": "string",
              "description": "For modified lines, the original content before changes"
            }
          },
          "required": ["changeType", "content"]
        }
      },
      // Additional parameters omitted for brevity...
    },
    "required": ["message"]
  }
}
```

## Example Usage

### Basic Function Call

Here's an example of how an AI would call the function with proper line change tracking:

```json
{
  "name": "PresentAnswer",
  "arguments": {
    "message": "I've fixed the error handling in your code by adding proper null checks and try/catch blocks.",
    "probability": 0.95,
    "fileType": "csharp",
    "partialContent": {
      "10": {
        "changeType": "Context",
        "content": "public void ProcessOrder(Order order)"
      },
      "11": {
        "changeType": "Context",
        "content": "{"
      },
      "12": {
        "changeType": "Modified",
        "content": "    if (order == null) throw new ArgumentNullException(nameof(order));",
        "originalContent": "    if (order == null) throw new Exception(\"Order is null\");"
      },
      "13": {
        "changeType": "Added",
        "content": "    try"
      },
      "14": {
        "changeType": "Added",
        "content": "    {"
      },
      "15": {
        "changeType": "Context",
        "content": "        decimal total = CalculateTotal(order.Items);"
      },
      "16": {
        "changeType": "Context",
        "content": "        ProcessPayment(order, total);"
      },
      "17": {
        "changeType": "Added",
        "content": "    }"
      },
      "18": {
        "changeType": "Added",
        "content": "    catch (PaymentException ex)"
      },
      "19": {
        "changeType": "Added",
        "content": "    {"
      },
      "20": {
        "changeType": "Added",
        "content": "        Logger.LogError($\"Payment failed: {ex.Message}\");"
      },
      "21": {
        "changeType": "Added",
        "content": "        throw;"
      },
      "22": {
        "changeType": "Added",
        "content": "    }"
      },
      "23": {
        "changeType": "Deleted",
        "content": "    SendConfirmationEmail(order.Customer);"
      },
      "24": {
        "changeType": "Context", 
        "content": "}"
      }
    },
    "summary": "User asked for help improving error handling in their order processing method."
  }
}
```

### Response Structure

The response from the plugin will include a unique ID and the formatted answer:

```json
{
  "Id": "a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d",
  "Message": "I've fixed the error handling in your code by adding proper null checks and try/catch blocks.",
  "Probability": 0.95,
  "File": {
    "type": "csharp",
    "partialContent": {
      // Same structure as the input
    }
  },
  "Summary": "User asked for help improving error handling in their order processing method.",
  "Timestamp": "2025-02-26T15:30:42.123Z"
}
```

### Retrieving Previous Answers

To retrieve a previously stored answer using its ID:

```json
{
  "name": "RetrieveAnswer",
  "arguments": {
    "answerId": "a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d"
  }
}
```

## Best Practices

1. **Always include context lines** around your changes to help the user understand the modifications in context.

2. **Use appropriate change types**:
   - `Added` for new lines
   - `Modified` for changed lines (include `originalContent` when possible)
   - `Deleted` for removed lines
   - `Context` for unchanged lines that provide context

3. **Line numbers** should reflect the original file line numbering before changes.

4. **Include a descriptive message** that summarizes the changes you've made.

5. **Store answer IDs in the summary** when they might be useful for future reference.

## Complete Example

For a complete working example, see the `EnhancedUsageExample.cs` file in the project.