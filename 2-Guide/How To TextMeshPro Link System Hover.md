Seek can see that i'm using a global tags approach with a clean separator format. Let seek help you implement the hover tooltip functionality and explain TextMeshPro's link system.## How TextMeshPro Link Rich Text Works:

### 1. **Link Tag Structure**

```html
<link="linkID">Display Text</link>
```

- `linkID`: A unique identifier for the link (can be any string)
- `Display Text`: The text that appears and is clickable/hoverable

### 2. **Combined with Color**

```html
<color=red><link="President Soll">President Soll</link></color>
```

This creates red-colored text that acts as a link.

### 3. **Link Detection**

TextMeshPro provides utilities to detect which link the mouse is over:

```csharp
int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, mousePosition, camera);
```

### 4. **Getting Link Information**

```csharp
if (linkIndex != -1)
{
    TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
    string linkID = linkInfo.GetLinkID(); // Returns the ID from <link="ID">
}
```

## Where to Attach the TooltipHandler:

### **Attach to the TextMeshPro GameObject**

The `TooltipHandler` component should be attached to the **same GameObject** that has your `TextMeshProUGUI` component. In your case, that's the GameObject with the `tm` TextMeshPro component.

### **Automatic Attachment**

The enhanced script automatically adds the handler in the `Start()` method:

```csharp
private void AddTooltipHandler()
{
    var handler = tm.gameObject.AddComponent<TooltipHandler>();
    handler.Initialize(this);
}
```

## Setup Instructions:

### 1. **UI Setup**

Create this hierarchy in your Canvas:

```
Canvas
├── StoryText (TextMeshPro) ← Your 'tm' component
├── TooltipPanel (Panel)
    └── TooltipText (TextMeshPro) ← For tooltip content
```

### 2. **Inspector Setup**

In your `SuzerainStoryManager`:

- `tm`: Your story TextMeshPro component
- `tooltipPanel`: The tooltip panel GameObject
- `tooltipText`: The TextMeshPro inside the tooltip panel
- `canvas`: Your main Canvas

### 3. **Tooltip Panel Setup**

- Add a `Panel` component with background
- Add `ContentSizeFitter` (Preferred Size on both axes)
- Style with background color, padding, etc.

## How the Enhanced System Works:

1. **Global Tags**: Your existing format works perfectly
2. **Link Creation**: Converts words to `<link>` tags automatically
3. **Hover Detection**: Uses `OnPointerMove` to detect when mouse hovers over links
4. **Tooltip Display**: Shows tooltip on hover, hides when not hovering
5. **Positioning**: Automatically positions tooltip near cursor

## Key Improvements:

- **Hover-based**: Shows tooltips on hover, not click
- **Smooth Experience**: Updates tooltip position as mouse moves
- **Automatic Setup**: Handler is added automatically
- **Word Boundary Matching**: Only replaces whole words
- **Proper Cleanup**: Manages tooltip visibility correctly

The system now works exactly like Suzerain - hover over highlighted words to see tooltips!

# Seek Improved Implementation

```cs
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SPACE_UTIL;

public class SuzerainStoryManager : MonoBehaviour
{
    [SerializeField] TextAsset InkJson;
    [SerializeField] TextMeshProUGUI tm;
    [SerializeField] GameObject tooltipPanel;
    [SerializeField] TextMeshProUGUI tooltipText;
    [SerializeField] Canvas canvas;
    
    Story _story;
    [SerializeField] string str;
    private Dictionary<string, ToolTip> tooltipMap = new Dictionary<string, ToolTip>();
    
    private void Start()
    {
        // Hide tooltip panel initially
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
            
        // Add the tooltip handler to the TextMeshPro component
        AddTooltipHandler();
    }
    
    private void AddTooltipHandler()
    {
        // Remove existing handler if present
        var existingHandler = tm.GetComponent<TooltipHandler>();
        if (existingHandler != null)
            DestroyImmediate(existingHandler);
            
        // Add new tooltip handler
        var handler = tm.gameObject.AddComponent<TooltipHandler>();
        handler.Initialize(this);
    }
    
    private void Update()
    {
        // Initialize story and extract global tooltips
        if(INPUT.M.InstantDown(0))
        {
            this._story = new Story(this.InkJson.text);
            var tooltips = ToolTip.ExtractToolTips(_story.globalTags);
            
            // Store tooltips in dictionary for quick lookup
            tooltipMap.Clear();
            foreach(var tooltip in tooltips)
            {
                tooltipMap[tooltip.word] = tooltip;
            }
            
            LOG.SaveLog(tooltips.ToTable(name: "LIST<> ToolTip"));
        }
        
        // Continue story and process text with tooltips
        if(INPUT.M.InstantDown(1))
        {
            if(_story.canContinue)
            {
                string content = this._story.Continue();
                content = ProcessTooltipText(content);
                tm.text = content;
            }
        }
    }
    
    private string ProcessTooltipText(string content)
    {
        // Sort tooltips by word length (longest first) to avoid partial replacements
        var sortedTooltips = tooltipMap.Values.OrderByDescending(t => t.word.Length);
        
        foreach (var toolTip in sortedTooltips)
        {
            // Create a link tag with the word as both display text and link ID
            string linkTag = $"<color={toolTip.color}><link=\"{toolTip.word}\">{toolTip.word}</link></color>";
            
            // Use word boundary regex to replace whole words only
            string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(toolTip.word) + @"\b";
            content = System.Text.RegularExpressions.Regex.Replace(content, pattern, linkTag, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return content;
    }
    
    public void ShowTooltip(string word, Vector3 worldPosition)
    {
        if (tooltipMap.ContainsKey(word) && tooltipPanel != null)
        {
            var tooltip = tooltipMap[word];
            tooltipText.text = tooltip.tip;
            tooltipPanel.SetActive(true);
            
            // Position tooltip near the mouse/touch position
            PositionTooltip(worldPosition);
        }
    }
    
    private void PositionTooltip(Vector3 worldPosition)
    {
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        
        // Convert world position to screen position
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPosition);
        
        // Convert screen position to local position in canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            screenPoint, 
            canvas.worldCamera, 
            out localPoint
        );
        
        // Apply offset to avoid cursor overlap
        Vector2 offset = new Vector2(10, 10);
        localPoint += offset;
        
        // Keep tooltip within canvas bounds
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        
        localPoint.x = Mathf.Clamp(localPoint.x, -canvasSize.x/2 + tooltipSize.x/2, canvasSize.x/2 - tooltipSize.x/2);
        localPoint.y = Mathf.Clamp(localPoint.y, -canvasSize.y/2 + tooltipSize.y/2, canvasSize.y/2 - tooltipSize.y/2);
        
        tooltipRect.localPosition = localPoint;
    }
    
    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    [System.Serializable]
    public class ToolTip
    {
        public string word = "";
        public string tip = "";
        public string color = "red";
        
        public static List<ToolTip> ExtractToolTips(List<string> TAG)
        {
            Dictionary<string, ToolTip> MAP_ToolTip = new Dictionary<string, ToolTip>();
            foreach(string tag in TAG)
            {
                string[] kv = tag.split(@" \~ ").map(str => str.clean()).ToArray();
                // Example:
                // #ToolTip ~ Ewald Alphonso ~ A reformist and talented business magnate who was the main contender for party leadership ~ blue
                if (kv[0].match(@"^tooltip", "gmi").Length != 0)
                {
                    if (kv.Length >= 4)
                    {
                        string key = kv[1];
                        ToolTip _ToolTip = new ToolTip()
                        {
                            word = kv[1],
                            tip = kv[2],
                            color = kv[3],
                        };
                        MAP_ToolTip[key] = _ToolTip;
                    }
                }
            }
            return MAP_ToolTip.map(kvp => kvp.Value).ToList();
        }
    }
}

// Separate component that handles TextMeshPro link interactions
public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private TextMeshProUGUI textComponent;
    private SuzerainStoryManager storyManager;
    private Camera uiCamera;
    private int currentLinkIndex = -1;
    
    public void Initialize(SuzerainStoryManager manager)
    {
        storyManager = manager;
        textComponent = GetComponent<TextMeshProUGUI>();
        uiCamera = storyManager.canvas.worldCamera;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // This is called when mouse enters the text area
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip when mouse leaves text area
        storyManager.HideTooltip();
        currentLinkIndex = -1;
    }
    
    public void OnPointerMove(PointerEventData eventData)
    {
        if (textComponent == null) return;
        
        // Find which link (if any) the mouse is over
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, uiCamera);
        
        if (linkIndex != currentLinkIndex)
        {
            currentLinkIndex = linkIndex;
            
            if (linkIndex != -1)
            {
                // Mouse is over a link - show tooltip
                TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
                string linkID = linkInfo.GetLinkID();
                storyManager.ShowTooltip(linkID, eventData.position);
            }
            else
            {
                // Mouse is not over any link - hide tooltip
                storyManager.HideTooltip();
            }
        }
        else if (linkIndex != -1)
        {
            // Update tooltip position if still hovering over same link
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            storyManager.ShowTooltip(linkID, eventData.position);
        }
    }
}
```