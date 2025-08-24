Below is a short, focused `README.md` you can drop into your prompt feed files. It summarizes what `StockfishBridge.cs` does, its public API, expected inputs/outputs, important config/limitations, and a tiny usage example. (This summary is based on your `StockfishBridge.cs` source.)

````markdown
# StockfishBridge.cs

**Summary**  
`StockfishBridge` is a Unity `MonoBehaviour` Class that wraps a Stockfish (UCI) engine as a non-blocking, coroutine-first bridge. It runs the engine in a background process/thread, parses UCI output, exposes a coroutine API for analysis requests, and returns a structured `ChessAnalysisResult` containing best move, evaluation (as win probabilities), depth, and debug output.

**Key features**
- Starts/stops Stockfish process and manages reader thread.
- Coroutine-based analysis (`AnalyzePositionCoroutine`) that yields until engine returns a `bestmove`.
- Parses `info` lines to produce:
  - `bestMove` (e.g. `"e2e4"`, or `"check-mate"`, `"stale-mate"`, or error messages)
  - `evaluation` (0..1 = probability WHITE wins)
  - `stmEvaluation` (probability for side-to-move)
  - `isGameEnd`, depths, raw engine output, skill level / approx Elo
- Crash detection and automatic restart helper (`RestartEngineCoroutine`).
- Options to limit engine strength (Elo, Skill Level) and separate depths for move-search vs evaluation.
- Debug logging and `OnEngineLine` UnityEvent for live engine lines.

**Public API (most useful members)**
- `void StartEngine()`
- `void StopEngine()`
- `IEnumerator InitializeEngineCoroutine()`
- `IEnumerator RestartEngineCoroutine()`
- `IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = 2000, int searchDepth = 1, int evaluationDepth = 5, int elo = 400, int skillLevel = 0)`
- `void SendCommand(string command)`
- `UnityEvent<string> OnEngineLine` — subscribe to raw engine lines.
- Properties: `LastAnalysisResult`, `LastRawOutput`, `IsEngineRunning`, `IsReady`

**Result structure**
`ChessAnalysisResult` (serializable)
- `bestMove` (string)
- `Side` (`'w'`/`'b'`)
- `evaluation` (float 0..1 for White)
- `stmEvaluation` (float 0..1 for side-to-move)
- `isGameEnd` (bool)
- `rawEngineOutput` (string)
- `searchDepth`, `evaluationDepth`, `skillLevel`, `approximateElo`, `errorMessage`

**Usage example**
```csharp
// Start engine (e.g. in Awake)
stockfishBridge.StartEngine();
StartCoroutine(stockfishBridge.InitializeEngineCoroutine());

// Run analysis (example)
StartCoroutine(RunAnalysis());

private IEnumerator RunAnalysis() {
  yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
    "r1bqkbnr/pppppppp/n7/8/8/N7/PPPPPPPP/R1BQKBNR w KQkq - 0 1",
    movetimeMs: 1000,
    searchDepth: 10,
    evaluationDepth: 12,
    elo: 1500,
    skillLevel: 0
  ));
  var result = stockfishBridge.LastAnalysisResult;
  Debug.Log(result.ToString());
}
````

## Important notes & limitations

- Requires Stockfish binary at `StreamingAssets/sf-engine.exe` (or platform-specific handling).
- Heavy parsing logic — outputs probabilities mapped from centipawns or mate distance; enable/disable evaluation with `enableEvaluation`.
    
- Designed to be robust to crashes/timeouts but still subject to OS process behavior and path/platform differences.
    
- Large file — prefer keeping this README + a few function signatures in prompt feed to help an LLM quickly understand the API instead of pasting entire source.
    

## License / attribution

- Keep the original source file attribution in your repo. This README is a short guide — consult the full `StockfishBridge.cs` for detailed behavior and tuning options.

```
### Is this the right approach for feeding a prompt to Claude 4?
Yes — good idea. Claude (and other LLMs) handle large codebases better when you give them a short, explicit summary of: purpose, public API (signatures), return types, important invariants/assumptions, and a small usage example. That allows the model to reason about the code without being forced to read thousands of lines. A few suggestions:
- Include the README above as the top of your prompt feed.
- Below it, include only the *key* code snippets (public method signatures, the `ChessAnalysisResult` class, any config constants, and the parsing logic you want the model to understand/modify).
- If you need the model to change behavior, include a small focused diff or a clear instruction referencing the named methods/fields.
- For very targeted changes, give a single file function or the relevant ~50–200 line snippet rather than the whole file.

If you want, I can produce a minimal prompt template that packages this README + the exact code snippets Claude 4 should be given (and a short instruction template) — I can do that now. Want that?
```