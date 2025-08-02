
## Prompt for TextEditor Syntax Highlight Behaviour in Unity3D

You are a professional Unity C# engineer and UI/UX specialist. I need a robust, production-grade TMPro input field component with:

1. **Syntax highlighting**
    - **Keywords** (`if`, `while`, `def`, etc.) in one color (e.g. blue).
    - **Strings** (`"…"`, `'…'`) in another (e.g. green).
    - **Numbers** in a distinct color (e.g. orange).
    - **Comments** (`# …`) in gray.
    - **All other text** white.
2. **Auto-indentation on Enter**
    - When the user presses **Enter**, the new line auto-inherits the indentation level (number of leading spaces) of the previous line.
3. **Tab handling**
    - Pressing **Tab** inserts four spaces (no actual `\t` character).
4. **Ctrl + Backspace**
    - Deletes the entire previous word (up to the preceding space or start of line).

**provide:**
- A clear architectural overview: which Unity events/hooks to use, how to parse the text, and how to apply colors in TMP. and colors alter in real time for every keypress based on type of token.
- Sample C# scripts (MonoBehaviour or custom `InputField` subclass) showing all behaviors.
- Explanations of any Unity or TMPro APIs you leverage (e.g. `onValueChanged`, `TextMeshProUGUI.textInfo`, `TMP_TextParsingUtilities`).
- Tips on performance (e.g. incremental repainting, avoiding full re-parses on every keystroke).

Answer as a professional dev: describe your reasoning, key trade-offs, and include ready-to-drop-into-Unity code.