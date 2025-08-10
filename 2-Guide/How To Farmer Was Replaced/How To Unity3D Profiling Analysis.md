
# How to add profiler markers inside Unity3D Script

**Recommended (modern, low-overhead): `ProfilerMarker`** — use `Unity.Profiling.ProfilerMarker` + `.Auto()` (or `Begin`/`End`) for minimal runtime cost and compatibility with jobs. Example:

```csharp
using UnityEngine;
using Unity.Profiling;

public class ScriptRunner : MonoBehaviour
{
    // create static readonly markers (one-time allocation)
    static readonly ProfilerMarker s_OnLineExecutedMarker = new ProfilerMarker("ExecutionTracker.OnLineExecuted");
    static readonly ProfilerMarker s_LineHighlightMarker  = new ProfilerMarker("ScriptRunner.LineHighlight");
    static readonly ProfilerMarker s_RunStartMarker       = new ProfilerMarker("ScriptRunner.RunStart");

    void OnLineExecuted(/*...*/)
    {
        using (s_OnLineExecutedMarker.Auto())
        {
            // existing method body
        }
    }

    void UpdateLineNumberHighlighting(/*...*/)
    {
        using (s_LineHighlightMarker.Auto())
        {
            // highlight code
        }
    }

    public void RunScript(/*...*/)
    {
        using (s_RunStartMarker.Auto())
        {
            // code executed at start of run
        }
    }
}
```

`ProfilerMarker.Auto()` is convenient because it automatically ends the sample when the scope exits. `ProfilerMarker.Begin()` / `End()` are also available if you need more manual control. ([Unity Documentation](https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerMarker.html?utm_source=chatgpt.com "Scripting API: ProfilerMarker - Unity - Manual"))

**Legacy / simple alternative: `Profiler.BeginSample` / `Profiler.EndSample`**  
If you prefer the older API:

```csharp
using UnityEngine;
using UnityEngine.Profiling;

void OnLineExecuted()
{
    Profiler.BeginSample("ExecutionTracker.OnLineExecuted");
    // ...
    Profiler.EndSample();
}
```

This works fine for quick samples but `ProfilerMarker` is preferred for performance and job compatibility. ([Unity Documentation](https://docs.unity3d.com/ScriptReference/Profiling.Profiler.html?utm_source=chatgpt.com "Profiler - Scripting API - Unity - Manual"))

# Notes & tips

- Avoid using `/` in marker names (it breaks some Profiler UI highlighting). ([Unity Documentation](https://docs.unity3d.com/6000.1/Documentation/Manual/profiler-add-markers-code.html?utm_source=chatgpt.com "Adding profiler markers to your code - Unity - Manual"))
    
- Markers show up in the Profiler’s **CPU Usage** timeline/hierarchy when profiling in the Editor or a Development Player. `BeginSample`/`EndSample` calls are conditionally compiled out in non-development (release) builds; `ProfilerMarker.Auto()` is minimal in release builds. ([Unity Documentation](https://docs.unity3d.com/6000.1/Documentation/Manual/profiler-add-markers-code.html?utm_source=chatgpt.com "Adding profiler markers to your code - Unity - Manual"))
    
- If you want richer metadata (ints/floats/strings attached to samples) or advanced counters, look at the **Unity Profiling Core** package (`com.unity.profiling.core`). ([Unity User Manual](https://docs.unity.cn/6000.0/Documentation/Manual/profiler-adding-information-code-intro.html?utm_source=chatgpt.com "Manual: Adding profiling information to your code introduction"))
    
- Markers placed in unusual threads or outside supported contexts may not appear — profiler samples are most reliable inside MonoBehaviour callbacks, jobs, or supported threads. ([Unity Discussions](https://discussions.unity.com/t/profiler-and-profilermarker-have-no-effect-outside-certain-methods/880007?utm_source=chatgpt.com "Profiler and ProfilerMarker have no effect outside certain methods"))
    

If you want, I can:

- paste these exact snippets into your existing script (show me the file), or
    
- show where to look in the Profiler window so you can spot your markers quickly. Which would help more?