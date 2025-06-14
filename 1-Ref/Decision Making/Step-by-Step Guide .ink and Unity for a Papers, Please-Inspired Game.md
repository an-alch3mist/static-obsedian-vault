


Certainly! Let's explore how to use **.ink** with **Unity** for interactive storytelling, using the game *Papers, Please* as a scenario. *Papers, Please* is a narrative-driven game where you play as an immigration officer, making decisions about who to let into the country based on their documents. These choices affect the story, making it a great example for learning .ink and Unity integration. This guide will walk you through the process step-by-step.

---

## What is .ink?
**.ink** is a scripting language created by Inkle Studios for writing interactive narratives. It’s perfect for games with dialogue and player choices, like *Papers, Please*. With .ink, you can:
- Write dialogue and present choices to the player.
- Create branching story paths based on decisions.
- Use variables to track story state (e.g., how many people you’ve let in).

## Why Use .ink with Unity?
**Unity** is a versatile game engine that handles graphics, sound, and gameplay mechanics. By combining .ink with Unity:
- .ink manages the story and dialogue.
- Unity handles the visuals, user interface (UI), and other game elements.
- Together, they create a seamless narrative experience.

---

## Step-by-Step Guide: .ink and Unity for a *Papers, Please*-Inspired Game

### Step 1: Write Your Story in .ink
In *Papers, Please*, you review documents and decide whether to admit or deny entry to immigrants. Let’s create a simple .ink script to simulate this.

#### Example .ink Script
Save this as `story.ink`:
```
VAR people_let_in = 0

You see a person approaching the checkpoint with their documents.

* [Let them in]
    ~ people_let_in = people_let_in + 1
    You stamp their passport and let them into the country.
    -> continue
* [Deny entry]
    You reject their entry and send them away.
    -> continue

=== continue ===
{ people_let_in > 2:
    The border patrol notices you've let in many people today.
- else:
    The day proceeds quietly.
}
-> END
```

#### Explanation
- **`VAR people_let_in = 0`**: Declares a variable to track how many people the player lets in.
- **Choices (`* [Let them in]` and `* [Deny entry]`)**: The player decides what to do.
  - `~ people_let_in = people_let_in + 1`: Increments the variable if the player chooses "Let them in."
- **Conditional Logic (`{ people_let_in > 2: ... }`)**: Changes the story outcome based on the variable.
- **`->`**: Directs the story to another section (e.g., `continue`).

This is a basic example, but you can expand it with more characters, days, or consequences, as in *Papers, Please*.

---

### Step 2: Set Up Unity with Ink Integration
To use .ink in Unity, you’ll need the **Ink Unity Integration** plugin.

#### Instructions
1. **Download the Plugin**:
   - Get it from the [Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/ink-unity-integration-69710) or [GitHub](https://github.com/inkle/ink-unity-integration).
2. **Import into Unity**:
   - Open Unity, go to `Assets > Import Package > Custom Package`, and select the downloaded file.
3. **Add Your .ink File**:
   - Drag `story.ink` into your Unity project’s `Assets` folder. The plugin will compile it into a JSON file automatically.

---

### Step 3: Control the Story in Unity
You’ll need a C# script to load and manage the .ink story in Unity.

#### Example Script: `StoryController.cs`
Create a new script in Unity and paste this code:
```csharp
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class StoryController : MonoBehaviour
{
    public TextAsset inkJSON; // Assign your compiled .ink JSON file here
    public Text dialogueText; // UI Text for displaying dialogue
    public Button[] choiceButtons; // Array of UI Buttons for choices

    private Story story;

    void Start()
    {
        story = new Story(inkJSON.text);
        DisplayNextLine();
    }

    void DisplayNextLine()
    {
        // Clear previous choices
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
        }

        if (story.canContinue)
        {
            // Show the next line of dialogue
            dialogueText.text = story.Continue();
        }
        else if (story.currentChoices.Count > 0)
        {
            // Display available choices
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<Text>().text = story.currentChoices[i].text;
                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => MakeChoice(choiceIndex));
            }
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        story.ChooseChoiceIndex(choiceIndex);
        DisplayNextLine();
    }
}
```

#### Explanation
- **`TextAsset inkJSON`**: Holds the compiled .ink file (assign it in the Unity Inspector).
- **`Story story`**: The runtime object that interprets the .ink script.
- **`story.Continue()`**: Advances the story and returns the next line of dialogue.
- **`story.currentChoices`**: Lists the available player choices.
- **`MakeChoice(int choiceIndex)`**: Processes the player’s selection and continues the story.

---

### Step 4: Create a User Interface in Unity
For *Papers, Please*, you need to display dialogue and choices to the player.

#### Setup
1. **Create a Canvas**:
   - In Unity, right-click in the Hierarchy: `UI > Canvas`.
2. **Add a Dialogue Text**:
   - Right-click the Canvas: `UI > Text`. Name it `DialogueText` and adjust its size/position.
3. **Add Choice Buttons**:
   - Right-click the Canvas: `UI > Button`. Duplicate it for each choice (e.g., 2 buttons for "Let them in" and "Deny entry").
   - Name them `ChoiceButton1`, `ChoiceButton2`, etc.

#### Link to Script
- Attach `StoryController.cs` to a GameObject (e.g., the Canvas).
- In the Inspector:
  - Drag the compiled `story.ink.json` (found in Assets) to the `Ink JSON` field.
  - Drag `DialogueText` to the `Dialogue Text` field.
  - Set the `Choice Buttons` array size to 2 (or more), then drag your buttons into the slots.

---

### Step 5: Handle Variables and Game State
In *Papers, Please*, decisions have consequences (e.g., letting too many people in might upset the authorities). .ink’s variables track this.

#### Accessing Variables
Add this to `StoryController.cs` to check the number of people let in:
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Space)) // Press Space to test
    {
        int peopleLetIn = (int)story.variablesState["people_let_in"];
        Debug.Log("People let in: " + peopleLetIn);
    }
}
```

#### Modifying Variables
You can also set variables from Unity:
```csharp
story.variablesState["people_let_in"] = 5; // Example: Set to 5
```

This lets you integrate .ink’s state with Unity mechanics, like triggering events or updating a UI counter.

---

### Step 6: Expand for *Papers, Please*
To make it more like *Papers, Please*:
- **Multiple Days**: Use .ink’s **knots** to organize the script by day:
  ```
  === day_one ===
  First day on the job.
  -> character_one

  === character_one ===
  A person approaches...
  ```
- **Documents**: Add choices to inspect documents or ask questions:
  ```
  * [Inspect passport]
      The passport looks valid.
      -> decision
  * [Ask questions]
      "Where are you from?"
      -> decision
  ```
- **Consequences**: Use variables to track mistakes or bribes, affecting later dialogue.

---

## Why This Works for *Papers, Please*?
- **Branching Choices**: .ink easily handles decisions like admitting or denying entry.
- **State Tracking**: Variables monitor your performance, mimicking *Papers, Please*’s consequences.
- **Modularity**: Unity’s UI and .ink’s script work together, keeping story and visuals separate.

---

## Conclusion
Using .ink with Unity lets you create a *Papers, Please*-style game where players make meaningful choices. Start with a simple .ink script, integrate it into Unity with the Ink Unity Integration plugin, and build a UI to bring the story to life. Experiment with more characters, days, and variables to deepen the experience. With practice, you’ll master interactive storytelling in Unity!