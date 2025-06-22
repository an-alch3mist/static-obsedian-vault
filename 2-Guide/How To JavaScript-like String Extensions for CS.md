
This library provides JavaScript-style `.split()` and `.match()` methods for C# strings, making regex operations more intuitive.
## namespace using SPACE_UTIL

```csharp
using SPACE_UTIL;

// Basic usage
string text = "Hello world1, test word2, another word3";
string[] words = text.match(@"word\d+");           // ["word1", "word2", "word3"]
string[] parts = text.split(@",\s*");              // ["Hello world1", "test word2", "another word3"]
```

## Methods

### `.split(this string str, pattern, flags = "gx")`

Splits the string using a regex pattern (like JavaScript's `str.split(/pattern/flags)`).

```csharp
string data = "A -> B\nC -> D\nE -> F";

// Split on newlines
data.split(@"\n")                      // ["A -> B", "C -> D", "E -> F"]

// Split on arrows with optional spaces
data.split(@"\s*->\s*")               // ["A", "B\nC", "D\nE", "F"]

// Split on multiple spaces
"word1   word2    word3".split(@" +")  // ["word1", "word2", "word3"]
```

### `.match(this string str, pattern, flags = "gm")`

Finds all matches of a regex pattern in the string (like JavaScript's `str.match(/pattern/flags)`).

```csharp
string text = @"Error: failed
Warning: slow  
Error: timeout";

// Find all patterns (default behavior) with "gm"
text.match(@"Error: \w+")              // ["Error: failed", "Error: timeout"]

// Same as above (gm is default)
text.match(@"Error: \w+", "gm")        // ["Error: failed", "Error: timeout"]

// Find patterns at end of lines
text.match(@"\w+$", "gm")              // ["failed", "slow", "timeout"]

// Case insensitive
text.match(@"error", "gim")            // ["Error", "Error"] (finds both)
```


### `.replace(this string str, pattern, replace_with, flags = "gm")`

Finds all matches of a regex pattern in the string and replace them with custom word.

```csharp
text.replace($@"{C.esc(word)}\d+", $@"<link=\"{_ToolTip.tip}\">word</link>")
```

## Regex Flags

| Flag | Name            | Description                              | Example Use                                                    |
| ---- | --------------- | ---------------------------------------- | -------------------------------------------------------------- |
| `g`  | Global          | Find all matches (not just first)        | `text.match(@"word", "g")` finds all "word"                    |
| `m`  | Multiline       | `^` and `$` work per-line                | `text.match(@"^\w+", "gm")` finds first word of each line      |
| `i`  | IgnoreCase      | Case insensitive matching                | `text.match(@"error", "gi")` finds "Error", "ERROR", etc.      |
| `x`  | ExplicitCapture | Only named groups `(?<name>...)` capture | Prevents unwanted captures in    "a -> b".split(" -> ", "gx"). |

## Example Common Patterns

### log
```csharp
string log = @"2024-01-15 ERROR: Database failed
2024-01-15 INFO: Server started  
2024-01-16 ERROR: Connection lost";

// Extract all error messages
log.match(@"ERROR: .+")                    // ["ERROR: Database failed", "ERROR: Connection lost"]

// Extract dates at start of lines
log.match(@"^\d{4}-\d{2}-\d{2}", "gm")    // ["2024-01-15", "2024-01-15", "2024-01-16"]

// Split into log entries
log.split(@"\n")                          // ["2024-01-15 ERROR: ...", "2024-01-15 INFO: ...", ...]

// Extract just the messages (everything after the date and level)
log.match(@"(?:ERROR|INFO|WARN): (.+)", "gm")  // Gets the full matches
```
### Parse CSV-like data

```csharp
string csv = "name,age,city\nJohn,25,NYC\nJane,30,LA";

string[] rows = csv.split(@"\n");
foreach(string row in rows)
{
    string[] columns = row.split(@",");
    // Process columns...
}
```
### Extract URLs from text

```csharp
string content = "Visit https://example.com or http://test.org for more info";
string[] urls = content.match(@"https?://[^\s]+");  // ["https://example.com", "http://test.org"]
```

### Find function calls in code

```csharp
string code = @"Debug.Log(""hello"");
Console.WriteLine(""world"");
Debug.Log(""test"");";

string[] debugCalls = code.match(@"Debug\.Log\([^)]+\)", "gm");  
// ["Debug.Log(\"hello\")", "Debug.Log(\"test\")"]
```

## Tips

1. **Use `.clean()` for multiline strings** - removes `\r` characters that can interfere with line matching
2. **Default flags ("gx" for split, "gm" for match) work for 90% of cases** - you rarely need to specify flags manually
3. **Test complex patterns first** - use [***online regex 101***](https://regex101.com/) testers for complicated patterns

## Essential Extension Methods

These utility methods support the main functionality and provide additional string operations.

#### `clean(this string str)`

Removes `\r` characters, leaving only `\n` for consistent line handling across platforms.

```csharp
string windowsText = "line1\r\nline2\r\n";
string cleanText = windowsText.clean();    // "line1\nline2\n"

// Used internally by split() and match()
text.split(@"\n")  // Automatically calls clean() first
```

#### `flat(this string str, string name)`

Displays string in a single line with visible escape characters - perfect for debugging multiline strings.

```csharp
string multiline = "Hello\r\nWorld\tTest";
Debug.Log(multiline.flat("check: "));      // "check: Hello\r\nWorld\tTest"
```

#### `join(this IEnumerable<string> strings, string separator)`

Joins string arrays/collections with a separator (like JavaScript's `array.join()`).

```csharp
string[] words = {"apple", "banana", "cherry"};
string result = words.join(", ");          // "apple, banana, cherry"
string list = words.join(" | ");           // "apple | banana | cherry"

// Works with any IEnumerable<string>
var matches = text.match(@"word\d+");
string output = matches.join(" -> ");      // "word1 -> word2 -> word3"
```

#### `repeat(char character, int count)`

Repeats a character multiple times.

```csharp
char.repeat('-', 20);                      // "--------------------"
char.repeat('=', 5);                       // "====="

// Useful for formatting
Debug.Log('='.repeat(50));
Debug.Log("TITLE");
Debug.Log('='.repeat(50));
```

#### `str_to_flags(string flags)`

Converts flag string to `RegexOptions` enum (used internally by split/match).

```csharp
RegexOptions opts = str_to_flags("gim");   // Global + IgnoreCase + Multiline
// Rarely used, 90% of cases direct usage - split() and match() handle this automatically
```
