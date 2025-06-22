
## Basic Link Syntax

TextMeshPro uses this format for clickable/interactive text:

```
<link="linkId">Clickable Text</link>
```

## How It Works

### 1. Link Tags

- `<link="myLink">` - Opens a link with ID "myLink"
- `</link>` - Closes the link
- The text between tags becomes interactive

### 2. Link Detection

```csharp
// Find which link (if any) the mouse is over
int linkIndex = TMP_TextUtilities.FindIntersectingLink(
    textComponent,           // Your TextMeshPro component
    Input.mousePosition,     // Or eventData.position
    camera                   // UI camera (can be null for overlay canvas)
);
```

### 3. Getting Link Information

```csharp
if (linkIndex != -1) // -1 means no link found
{
    TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
    string linkID = linkInfo.GetLinkID();           // Gets "myLink"
    string linkText = linkInfo.GetLinkText();       // Gets "Clickable Text"
    
    // You can now do something based on the linkID
    HandleLinkClick(linkID);
}
```

## Complete Example

### HTML-like Rich Text:

```
This is normal text. <link="character1"><color=blue>Ewald Alphonso</color></link> is a character. 
<link="location1"><color=green>Sordland</color></link> is a country.
```

### Code to Handle:

```csharp
public void OnPointerClick(PointerEventData eventData)
{
    int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, camera);
    
    if (linkIndex != -1)
    {
        TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
        string linkID = linkInfo.GetLinkID();
        
        switch(linkID)
        {
            case "character1":
                ShowCharacterInfo("Ewald Alphonso");
                break;
            case "location1":
                ShowLocationInfo("Sordland");
                break;
        }
    }
}
```

## Advanced Features

### 1. Multiple Links Per Text

```
<link="link1">First Link</link> and <link="link2">Second Link</link> in same text.
```

### 2. Nested Tags

```
<link="myLink"><color=red><b>Bold Red Link</b></color></link>
```

### 3. Link Information Structure

```csharp
public struct TMP_LinkInfo
{
    public string GetLinkID();    // Returns the link identifier
    public string GetLinkText();  // Returns the actual text content
    public int linkIdFirstCharacterIndex;  // Start position in text
    public int linkIdLength;               // Length of link ID
    public int linkTextFirstCharacterIndex; // Start of link text
    public int linkTextLength;             // Length of link text  
}
```

## Your Implementation Benefits

1. **Global Tooltips**: Define once in Ink global tags, use everywhere
2. **Automatic Processing**: Text gets processed to add link tags automatically
3. **Hover Detection**: Uses `IPointerMoveHandler` for smooth hover effects
4. **Flexible Styling**: Combine with `<color>` tags for visual feedback

## UI Setup Required

1. **TextMeshPro Component**: Must have "Raycast Target" enabled
2. **Canvas**: Needs GraphicRaycaster component
3. **EventSystem**: Must exist in scene (usually auto-created)
4. **Tooltip Panel**: UI panel for displaying tooltip content

## Performance Notes

- Link detection happens on mouse move, so keep it lightweight
- `TMP_TextUtilities.FindIntersectingLink()` is optimized by Unity
- Consider caching link information if text doesn't change frequently

## Common Issues

1. **Links Not Detecting**: Check if TextMeshPro has "Raycast Target" enabled
2. **Wrong Camera**: Ensure correct camera reference for UI canvas type
3. **Z-Order**: Make sure tooltip panel renders above text
4. **Case Sensitivity**: Link IDs are case-sensitive

This system gives you the same professional tooltip behavior as Suzerain!

# With Ink Integration

```cs
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
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
	private List<ToolTip> currentToolTips = new List<ToolTip>();
	
	private void Start()
	{
		// Hide tooltip panel initially
		if (tooltipPanel != null)
			tooltipPanel.SetActive(false);
			
		// Add tooltip handler to the text component
		AddTooltipHandler();
	}
	
	private void AddTooltipHandler()
	{
		if (tm != null)
		{
			var handler = tm.gameObject.GetComponent<TooltipHandler>();
			if (handler == null)
			{
				handler = tm.gameObject.AddComponent<TooltipHandler>();
			}
			handler.Initialize(this);
		}
	}
	
	private void Update()
	{
		// Initialize story
		if(INPUT.M.InstantDown(0))
		{
			this._story = new Story(this.InkJson.text);
			currentToolTips = ToolTip.ExtractToolTips(_story.globalTags);
			LOG.SaveLog(currentToolTips.ToTable(name: "LIST<> ToolTip"));
		}
		
		// Continue story
		if(INPUT.M.InstantDown(1))
		{
			if(_story.canContinue)
			{
				string content = this._story.Continue();
				string processedContent = ProcessContentWithTooltips(content);
				tm.text = processedContent;
			}
		}
	}
	
	private string ProcessContentWithTooltips(string content)
	{
		string processedContent = content;
		
		// Sort tooltips by word length (longest first) to avoid partial replacements
		var sortedTooltips = currentToolTips.OrderByDescending(t => t.word.Length);
		
		foreach (var toolTip in sortedTooltips)
		{
			// Create link tag with tooltip data
			// Format: <link="tooltipId">word</link>
			string linkId = $"tooltip_{currentToolTips.IndexOf(toolTip)}";
			string replacement = $"<color={toolTip.color}><link=\"{linkId}\">{toolTip.word}</link></color>";
			
			// Use word boundary regex for exact word matching
			string pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(toolTip.word)}\b";
			processedContent = System.Text.RegularExpressions.Regex.Replace(
				processedContent, 
				pattern, 
				replacement, 
				System.Text.RegularExpressions.RegexOptions.IgnoreCase
			);
		}
		
		return processedContent;
	}
	
	public void ShowTooltip(string linkId, Vector3 worldPosition)
	{
		// Extract tooltip index from linkId
		if (linkId.StartsWith("tooltip_"))
		{
			string indexStr = linkId.Substring(8);
			if (int.TryParse(indexStr, out int index) && index < currentToolTips.Count)
			{
				ToolTip tooltip = currentToolTips[index];
				
				if (tooltipPanel != null && tooltipText != null)
				{
					tooltipText.text = tooltip.tip;
					tooltipPanel.SetActive(true);
					
					// Position tooltip
					PositionTooltip(worldPosition);
				}
			}
		}
	}
	
	public void HideTooltip()
	{
		if (tooltipPanel != null)
			tooltipPanel.SetActive(false);
	}
	
	private void PositionTooltip(Vector3 worldPosition)
	{
		if (canvas == null || tooltipPanel == null) return;
		
		RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
		RectTransform canvasRect = canvas.GetComponent<RectTransform>();
		
		// Convert world position to screen position
		Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPosition);
		
		// Convert screen position to local position in canvas
		Vector2 localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvasRect, 
			screenPosition, 
			canvas.worldCamera, 
			out localPosition
		);
		
		// Add offset and clamp to screen bounds
		Vector2 offset = new Vector2(10, 10);
		localPosition += offset;
		
		// Clamp to keep tooltip on screen
		Vector2 canvasSize = canvasRect.sizeDelta;
		Vector2 tooltipSize = tooltipRect.sizeDelta;
		
		localPosition.x = Mathf.Clamp(localPosition.x, 
			-canvasSize.x/2 + tooltipSize.x/2, 
			canvasSize.x/2 - tooltipSize.x/2);
		localPosition.y = Mathf.Clamp(localPosition.y, 
			-canvasSize.y/2 + tooltipSize.y/2, 
			canvasSize.y/2 - tooltipSize.y/2);
		
		tooltipRect.localPosition = localPosition;
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
				// #ToolTip ~ Ewald Alphonso ~ A reformist and talented business magnate who was the main contender for party leadership ~ red
				if (kv[0].match(@"^tooltip", "gmi").Length != 0)
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
			return MAP_ToolTip.map(kvp => kvp.Value).ToList();
		}
	}
}

// Separate component to handle tooltip interactions
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
		CheckForTooltip(eventData);
	}
	
	public void OnPointerMove(PointerEventData eventData)
	{
		CheckForTooltip(eventData);
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		currentLinkIndex = -1;
		storyManager.HideTooltip();
	}
	
	private void CheckForTooltip(PointerEventData eventData)
	{
		if (textComponent == null || storyManager == null) return;
		
		// Find if pointer is over a link
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, uiCamera);
		
		if (linkIndex != -1 && linkIndex != currentLinkIndex)
		{
			// New link detected
			currentLinkIndex = linkIndex;
			TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
			string linkID = linkInfo.GetLinkID();
			
			storyManager.ShowTooltip(linkID, eventData.position);
		}
		else if (linkIndex == -1 && currentLinkIndex != -1)
		{
			// No longer over a link
			currentLinkIndex = -1;
			storyManager.HideTooltip();
		}
	}
}
```