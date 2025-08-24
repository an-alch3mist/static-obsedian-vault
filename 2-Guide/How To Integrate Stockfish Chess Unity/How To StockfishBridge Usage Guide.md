
## Setup

1. **Place Engine Binary**: Put `sf-engine.exe` in `Assets/StreamingAssets/sf-engine.exe`
2. **Add Component**: Attach `StockfishBridge` component to a GameObject in your scene
3. **Configure Inspector Settings**:
    - **Default Depth**: 1 (higher = stronger but slower)
    - **Default Elo**: 400 (lower = weaker engine)
    - **Default Skill Level**: 0 (0 = weakest, 20 = strongest)

## Basic Usage

### Initialize Engine (Automatic)

```csharp
// Engine starts automatically in Awake() and initializes
// No manual initialization required
```

### Analyze Position

```csharp
public class ChessGameController : MonoBehaviour
{
    [SerializeField] private StockfishBridge stockfishBridge;
    
    private IEnumerator GetEngineMove()
    {
        string currentFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        
        // Analyze using default settings
        yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(currentFEN));
        
        // Get result
        var result = stockfishBridge.LastAnalysisResult;
        
        Debug.Log($"Best move: {result.bestMove}");
        Debug.Log($"Side to move: {result.Side}");
        Debug.Log($"White win probability: {result.evaluation:F3}");
        Debug.Log($"Game over: {result.isGameEnd}");
    }
}
```

### Custom Engine Settings

```csharp
private IEnumerator GetEngineMove()
{
    string fen = "your_position_here";
    
    // Custom analysis with specific settings
    yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
        fen,
        movetimeMs: -1,     // Use depth instead of time
        depth: 5,           // Search 5 moves deep
        elo: 1200,          // 1200 Elo strength
        skillLevel: 10      // Skill level 10/20
    ));
    
    var result = stockfishBridge.LastAnalysisResult;
    // Process result...
}
```

## Result Interpretation

### ChessAnalysisResult Properties

- **`bestMove`**:
    - Normal moves: `"e2e4"`, `"Nf3"`, etc.
    - Special cases: `"check-mate"`, `"stale-mate"`
    - Errors: `"ERROR: message"`

- **`Side`**: `'w'` for white to move, `'b'` for black to move

- **`evaluation`**: Win probability for white (0.0 to 1.0)
    - `0.5` = Equal position
    - `1.0` = White wins/checkmate
    - `0.0` = Black wins/checkmate

- **`isGameEnd`**: `true` for checkmate/stalemate

### Special Position Examples

```csharp
// Checkmate position - White mates in 1
string checkmateFEN = "K6k/8/8/8/8/8/R7/6R1 b - - 0 1";
yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(checkmateFEN));

var result = stockfishBridge.LastAnalysisResult;
// result.bestMove = "check-mate"
// result.evaluation = 1.0 (100% for white)
// result.isGameEnd = true
```

## Error Handling

### Common Error Cases

```csharp
private IEnumerator HandleErrors()
{
    yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine("invalid_fen"));
    
    var result = stockfishBridge.LastAnalysisResult;
    
    if (result.bestMove.StartsWith("ERROR:"))
    {
        Debug.LogError($"Analysis failed: {result.errorMessage}");
        // Handle error appropriately
        return;
    }
    // Process valid result...
}
```

### Engine Crash Recovery

```csharp
private IEnumerator HandleEngineCrash()
{
    // Check if engine crashed
    if (stockfishBridge.DetectAndHandleCrash())
    {
        Debug.Log("Engine crashed, restarting...");
        yield return StartCoroutine(stockfishBridge.RestartEngineCoroutine());
        
        if (stockfishBridge.IsEngineRunning)
        {
            Debug.Log("Engine restarted successfully");
            // Continue with analysis...
        }
        else
        {
            Debug.LogError("Failed to restart engine");
        }
    }
}
```

## Testing

### Using the Test Script

1. **Setup**: Attach `StockfishBridgeCheck_1` component to a GameObject
2. **Reference**: Drag your `StockfishBridge` component to the inspector field
3. **Configure**: Set test options in inspector
4. **Run**: Tests start automatically or use "Run Tests" context menu

### Test Categories

- **Invalid FEN**: Tests error handling for malformed positions
- **Checkmate**: Verifies mate detection and evaluation = 1.0/0.0
- **Stalemate**: Checks stalemate detection and evaluation = 0.5
- **Normal Positions**: Tests regular gameplay positions
- **Edge Cases**: Boundary condition testing

## Performance Tips

### Optimization Settings

```csharp
// For real-time gameplay (fast but weaker)
yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
    fen, -1, 1,     // Depth 1
    400, 0          // Low Elo, minimum skill
));

// For puzzle solving (slower but stronger)
yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
    fen, -1, 8,     // Depth 8
    -1, -1          // Maximum strength
));
```

### Memory Management

- Engine automatically starts/stops with application lifecycle
- No manual cleanup required
- Engine persists through scene changes (singleton behavior)

## Advanced Usage

### Custom UCI Commands

```csharp
// Send direct UCI commands (advanced users only)
stockfishBridge.SendCommand("setoption name Threads value 2");
stockfishBridge.SendCommand("setoption name Hash value 128");
```

### Engine Status Monitoring

```csharp
private void CheckEngineStatus()
{
    Debug.Log($"Engine Running: {stockfishBridge.IsEngineRunning}");
    Debug.Log($"Engine Ready: {stockfishBridge.IsReady}");
    Debug.Log($"Last Output: {stockfishBridge.LastRawOutput}");
}
```

### Event Handling

```csharp
private void Start()
{
    // Subscribe to engine output events
    stockfishBridge.OnEngineLine.AddListener(OnEngineOutput);
}

private void OnEngineOutput(string line)
{
    Debug.Log($"Engine: {line}");
}
```

## Troubleshooting

### Common Issues

1. **"Engine executable not found"**
    - Ensure `sf-engine.exe` is in `Assets/StreamingAssets/`
    - Check file permissions and antivirus blocking
2. **"Engine failed to initialize"**
    - Try restarting Unity
    - Check console for detailed error messages
    - Verify Stockfish binary is compatible with your platform
3. **Slow performance**
    - Reduce depth setting (use 1-3 for real-time)
    - Lower Elo rating for weaker/faster play
    - Use time limits instead of depth for consistent timing
4. **Incorrect evaluations**
    - Increase depth for more accurate analysis
    - Check FEN format is valid
    - Some positions may need deeper analysis to detect mates

### Debug Output

Enable detailed logging in inspector to see full engine communication:

```
[Stockfish] > position fen rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
[Stockfish] > go depth 1
[Stockfish] < info depth 1 seldepth 1 score cp 25 nodes 20 time 1 pv e2e4
[Stockfish] < bestmove e2e4
```

## Integration Examples

### Turn-Based Chess Game

```csharp
public class ChessTurnManager : MonoBehaviour
{
    [SerializeField] private StockfishBridge stockfishBridge;
    
    public IEnumerator ProcessAITurn(string currentFEN)
    {
        // Show thinking indicator
        ShowThinkingUI();
        
        // Get AI move
        yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(currentFEN));
        var result = stockfishBridge.LastAnalysisResult;
        
        HideThinkingUI();
        
        if (result.bestMove.StartsWith("ERROR:"))
        {
            HandleError(result.errorMessage);
            yield break;
        }
        
        if (result.isGameEnd)
        {
            HandleGameEnd(result.bestMove); // "check-mate" or "stale-mate"
        }
        else
        {
            ExecuteMove(result.bestMove);
        }
        
        // Update evaluation bar
        UpdateEvaluationBar(result.evaluation);
    }
}
```

### Chess Puzzle Solver

```csharp
public class ChessPuzzleSolver : MonoBehaviour
{
    [SerializeField] private StockfishBridge stockfishBridge;
    
    public IEnumerator SolvePuzzle(string puzzleFEN)
    {
        // Use higher depth for puzzle accuracy
        yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
            puzzleFEN, -1, 6, -1, -1  // Depth 6, max strength
        ));
        
        var result = stockfishBridge.LastAnalysisResult;
        
        if (result.evaluation >= 0.95f)
        {
            Debug.Log($"Found winning move: {result.bestMove}");
        }
        else if (result.evaluation <= 0.05f)
        {
            Debug.Log($"Position is losing, best try: {result.bestMove}");
        }
        else
        {
            Debug.Log($"Unclear position: {result.bestMove} (eval: {result.evaluation:F3})");
        }
    }
}
```

## API Reference Summary

### Key Methods

- `AnalyzePositionCoroutine(string fen)` - Analyze with defaults
- `AnalyzePositionCoroutine(string fen, int time, int depth, int elo, int skill)` - Custom analysis
- `RestartEngineCoroutine()` - Restart crashed engine
- `DetectAndHandleCrash()` - Check engine status
- `SendCommand(string)` - Direct UCI command

### Key Properties

- `LastAnalysisResult` - Latest analysis result
- `IsEngineRunning` - Engine process status
- `IsReady` - Engine initialization status
- `LastRawOutput` - Raw engine response

### Inspector Settings

- `defaultDepth` - Search depth (1-20)
- `defaultElo` - Engine strength (400-3000)
- `defaultSkillLevel` - Skill level (0-20)
- `enableDebugLogging` - Console output toggle