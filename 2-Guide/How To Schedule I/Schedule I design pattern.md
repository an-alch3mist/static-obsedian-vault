# Schedule-1 — Codebase Architecture Documentation

> **Purpose:** In-depth technical reference for the systems, patterns, and design decisions found in the Schedule-1 Unity/C# codebase. Written for learning and professional reference.
> 
> **Source:** Decompiled + stripped script source (1752 files, `Assembly-CSharp.dll` via ILSpy + FishNet codegen).  
> **Engine:** Unity + FishNet (networking), A* Pathfinding Project, Curvy Splines.

---

## Table of Contents

1. [Project Overview & Namespace Map](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#1-project-overview--namespace-map)
2. [Item Framework](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#2-item-framework)
    - [ItemDefinition (Static Data)](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#21-itemdefinition-static-data)
    - [ItemInstance (Runtime State)](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#22-iteminstance-runtime-state)
    - [Definition Hierarchy](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#23-definition-hierarchy)
    - [Instance Hierarchy](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#24-instance-hierarchy)
    - [Registry — Global Item Lookup](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#25-registry--global-item-lookup)
3. [Inventory & Storage System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#3-inventory--storage-system)
    - [ItemSlot](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#31-itemslot)
    - [IItemSlotOwner Interface](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#32-iitemslotowner-interface)
    - [ItemFilter System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#33-itemfilter-system)
    - [StorageEntity](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#34-storageentity)
    - [WorldStorageEntity & SurfaceStorageEntity](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#35-worldstorageentity--surfaceentity)
4. [Station Framework](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#4-station-framework)
    - [StationItem & ItemModule](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#41-stationitem--itemmodule)
    - [StationRecipe](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#42-stationrecipe)
    - [Concrete Stations](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#43-concrete-stations)
5. [Product & Effects System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#5-product--effects-system)
    - [ProductDefinition](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#51-productdefinition)
    - [ProductItemInstance Hierarchy](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#52-productiteminstance-hierarchy)
    - [Effect System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#53-effect-system)
6. [NPC System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#6-npc-system)
    - [NPC Base](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#61-npc-base)
    - [Behaviour System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#62-behaviour-system)
    - [NPCAction & NPCSignal (Scheduler)](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#63-npcaction--npcsignal-scheduler)
    - [NPCBehaviour Stack](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#64-npcbehaviour-stack)
7. [Management System (IConfigurable)](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#7-management-system-iconfigurable)
    - [IConfigurable Interface](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#71-iconfigurable-interface)
    - [EntityConfiguration & ConfigField](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#72-entityconfiguration--configfield)
    - [ConfigPanel & UI Binding](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#73-configpanel--ui-binding)
8. [Law & Crime System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#8-law--crime-system)
9. [Singleton Infrastructure](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#9-singleton-infrastructure)
10. [Save & Persistence System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#10-save--persistence-system)
11. [Phone & App System](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#11-phone--app-system)
12. [Networking Layer (FishNet)](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#12-networking-layer-fishnet)
13. [Design Patterns — Full Reference](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#13-design-patterns--full-reference)
14. [Key Takeaways for Indie Devs](https://claude.ai/chat/93da44c7-4db3-4b7b-94e3-e5755849cfcb#14-key-takeaways-for-indie-devs)

---

## 1. Project Overview & Namespace Map

The codebase is organized into clean domain namespaces. Each namespace is a self-contained module with minimal coupling to others.

```
ScheduleOne
├── ItemFramework          → ItemDefinition, ItemInstance, ItemSlot, ItemFilter
├── StationFramework       → StationItem, StationRecipe, ItemModule
├── Storage                → StorageEntity, WorldStorageEntity, SurfaceStorageEntity
├── Product                → ProductDefinition, ProductItemInstance, drug-specific types
├── Effects                → Effect (abstract SO), all concrete drug effects
├── NPCs                   → NPC base, NPCMovement, NPCAwareness, NPCHealth
├── NPCs.Behaviour         → Behaviour base + all concrete NPC behaviours
├── NPCs.Schedules         → NPCAction, NPCSignal + concrete scheduled actions
├── Management             → IConfigurable, EntityConfiguration, ConfigField system
├── Law                    → Crime base, PlayerCrimeData, LawActivitySettings
├── DevUtilities           → Singleton<T>, NetworkSingleton<T>, PlayerSingleton<T>
├── Persistence            → ISaveable, IBaseSaveable, IGenericSaveable, Loaders
├── UI                     → App<T>, all phone apps, shop UIs, station canvas UIs
├── Money                  → MoneyManager, Transaction, Cash types
├── Map                    → Map, MapRegion, POI, HeatmapManager
└── AvatarFramework        → Avatar, customization, equippables, IK
```

**Core philosophy:** Data lives in ScriptableObjects. Runtime state lives in C# classes. World presence lives in MonoBehaviour/NetworkBehaviour. These three layers never collapse into one.

---

## 2. Item Framework

The item framework is the single most important system — everything interactable in the game goes through it. It is built on the **Definition/Instance Split** pattern.

### 2.1 ItemDefinition (Static Data)

```
ItemDefinition  :  ScriptableObject
```

`ItemDefinition` is a Unity **ScriptableObject** — it lives in the project as an asset. It holds data that never changes at runtime: name, description, icon, category, legal status, stack limit, the equippable prefab reference, and custom UI references.

The key virtual method is:

```csharp
public virtual ItemInstance GetDefaultInstance(int quantity = 1)
```

This is a **Factory Method**. Each subclass overrides it to return the correct runtime instance type. The definition is the factory; you never call `new WeedInstance()` from outside.

**What it does NOT hold:** quantity, quality, applied packaging, runtime state. That belongs to `ItemInstance`.

**Designer workflow:** A designer creates a new `WeedDefinition` asset in the Unity editor, fills in name/icon/price, and the game code can refer to it by ID through the `Registry`. No code change needed for a new item type.

---

### 2.2 ItemInstance (Runtime State)

```
ItemInstance  :  abstract class  (Serializable)
```

`ItemInstance` is a plain C# class (not a MonoBehaviour) — it has no Unity GameObject. It holds the mutable state for one "stack" of items:

|Property|Role|
|---|---|
|`Definition`|Back-reference to its `ItemDefinition`|
|`Quantity`|How many are in this stack|
|`ID`|Runtime identifier for save/load|
|`onDataChanged`|C# Action delegate — fires when anything changes|
|`requestClearSlot`|Action delegate — tells the owning slot to remove this|

Key virtual members:

```csharp
public virtual bool CanStackWith(ItemInstance other, bool checkQuantities = true)
public virtual ItemInstance GetCopy(int overrideQuantity = -1)
public virtual ItemData GetItemData()           // for save serialization
public virtual float GetMonetaryValue()
```

The `[CodegenExclude]` attribute on complex properties tells FishNet's code generator to skip them during network serialization — only primitives and simple types get auto-serialized over the network.

**Why not MonoBehaviour?** Item instances are data, not world objects. They live in inventory slots, get serialized, get copied. Making them MonoBehaviours would require GameObject instantiation just to hold an int and a string — a classic Unity anti-pattern this codebase correctly avoids.

---

### 2.3 Definition Hierarchy

```
ItemDefinition  (ScriptableObject)
│
├── StorableItemDefinition
│   │   + BasePurchasePrice, ResellMultiplier, RequiredRank
│   │   + StoredItem (world prefab ref), StationItem (optional)
│   │   + CombatUtility, PickpocketDifficultyMultiplier
│   │
│   ├── QualityItemDefinition
│   │       + DefaultQuality (EQuality enum)
│   │
│   ├── AdditiveDefinition
│   │       + mixing-specific data
│   │
│   ├── SoilDefinition / SeedDefinition / SporeSyringeDefinition
│   │
│   └── PropertyItemDefinition
│           + property ownership data
│           │
│           └── ProductDefinition
│                   + BasePrice, MarketValue, DrugTypes
│                   + Effects list, EffectsDuration
│                   + ValidPackaging[], Recipes list
│                   + implements ISaveable (products persist mix recipes)
│                   │
│                   ├── WeedDefinition
│                   ├── CocaineDefinition
│                   ├── LiquidMethDefinition
│                   └── ShroomDefinition (inferred)
│
├── BuildableItemDefinition
│       + construction/placement data
│
├── CashDefinition
│       + denomination data
│
└── WaterContainerDefinition
        + capacity, fill logic
```

**Pattern in use:** Open/Closed Principle. You add a new drug type by creating a new `ProductDefinition` subclass and asset — no existing code changes.

---

### 2.4 Instance Hierarchy

```
ItemInstance  (abstract)
│
├── IntegerItemInstance
│       + plain integer-quantity items (no quality)
│
├── StorableItemInstance
│       + StoredItem ref, physical world presence
│       │
│       └── QualityItemInstance
│               + Quality (EQuality enum)
│               │
│               └── ProductItemInstance
│                       + PackagingID, AppliedPackaging
│                       + ApplyEffectsToNPC/Player(), ClearEffects...()
│                       + GetSimilarity(), GetAddictiveness()
│                       │
│                       ├── WeedInstance
│                       ├── CocaineInstance
│                       ├── ShroomInstance
│                       │       + psychedelic coroutine effects
│                       └── LiquidMethInstance (inferred)
│
└── CashInstance
        + Amount, denomination
```

Each leaf class only overrides what's different. `ShroomInstance`, for example, overrides `ApplyEffectsToPlayer` to start a psychedelic coroutine, while inheriting all standard packaging, quality, and stacking behavior from `ProductItemInstance`.

---

### 2.5 Registry — Global Item Lookup

```csharp
public class Registry : PersistentSingleton<Registry>
```

`Registry` is the global catalog of all `ItemDefinition` assets. It stores:

- `List<ItemRegister>` — serialized in the Inspector, maps ID string → `ItemDefinition`
- `Dictionary<int, ItemRegister>` — runtime hash-map for O(1) lookup
- `itemIDAliases` — backward-compatible renaming support

Key API:

```csharp
// Typed generic lookup — no casting required at call site
Registry.GetItem<WeedDefinition>("weed_og_kush");

// Untyped lookup
Registry.GetItem("cash");

// Runtime registration (used for procedurally created products)
Registry.Instance.AddToRegistry(definition);
```

Using a hash of the ID string as the dictionary key avoids per-frame string comparison. The `PersistentSingleton` base ensures it survives scene loads.

---

## 3. Inventory & Storage System

### 3.1 ItemSlot

```csharp
[Serializable]
public class ItemSlot
```

`ItemSlot` is the fundamental unit of inventory. It is a plain C# class (not a MonoBehaviour) that wraps exactly one `ItemInstance` and enforces all the rules around it.

**State it manages:**

|Member|Role|
|---|---|
|`ItemInstance`|The stored item (or null)|
|`SlotOwner`|Reference to the `IItemSlotOwner` that owns this slot|
|`ActiveLock`|A `ItemSlotLock` — who locked it and why|
|`IsRemovalLocked`|Prevents items being taken out|
|`IsAddLocked`|Prevents items being added|
|`HardFilters`|`List<ItemFilter>` — set by code, immovable|
|`PlayerFilter`|`SlotFilter` — player-configurable filter|
|`SiblingSet`|For sibling-slot logic (e.g. paired slots on a station)|

**Events (C# Action delegates):**

```csharp
public Action onItemDataChanged;
public Action onItemInstanceChanged;
public Action onLocked;
public Action onUnlocked;
public Action onFilterChange;
```

UI panels subscribe to these delegates rather than polling. When item data changes, the slot fires `onItemDataChanged` and any listening UI redraws itself.

**Core methods:**

```csharp
// High-level insert — respects filters, locks, stacking
public virtual void InsertItem(ItemInstance item)

// Low-level set — used internally or by network sync
public virtual void SetStoredItem(ItemInstance instance, bool _internal = false)

// Adds more of the same item
public virtual void AddItem(ItemInstance item, bool _internal = false)

// Empties the slot
public virtual void ClearStoredInstance(bool _internal = false)

// Filter queries
public virtual bool DoesItemMatchHardFilters(ItemInstance item)
public virtual bool DoesItemMatchPlayerFilters(ItemInstance item)
public virtual int GetCapacityForItem(ItemInstance item, bool checkPlayerFilters = false)

// Static utility — fills the first available slot in a list
public static bool TryInsertItemIntoSet(List<ItemSlot> ItemSlots, ItemInstance item)
```

The `_internal = false` parameter is a pattern used throughout: internal calls from network RPCs pass `true` to skip re-broadcasting, preventing feedback loops.

---

### 3.2 IItemSlotOwner Interface

```csharp
public interface IItemSlotOwner
{
    // Exposes the slot list
    List<ItemSlot> ItemSlots { get; set; }
}
```

Any class that holds `ItemSlot` instances implements `IItemSlotOwner`. This decouples `ItemSlot` from the concrete container type — a slot can refer back to its owner without knowing if it's a player inventory, a storage chest, a station input, or a crafting table.

Implementors include: `StorageEntity`, `ChemistryStation`, `MixingStation`, `Player` inventory.

---

### 3.3 ItemFilter System

Filters control what items can go into a slot. The system uses a **Composite Filter** approach: a slot holds a list of filters, and an item must pass all of them.

```
ItemFilter  (base class)
│
├── ItemFilter_Category       → only items of EItemCategory X
├── ItemFilter_ID             → only the specific item ID
├── ItemFilter_LegalStatus    → only items with ELegalStatus X
├── ItemFilter_PackagedProduct → only packaged products
├── ItemFilter_UnpackagedProduct
├── ItemFilter_MixingIngredient
├── ItemFilter_Dryable        → only items that can be dried
└── ItemFilter_ClothingSlot   → only clothing for a specific body slot
```

**Usage:**

```csharp
// Hardcoded on a station input slot — only accepts additives
inputSlot.AddFilter(new ItemFilter_Category(EItemCategory.Additive));

// Player-configurable filter on a storage chest slot
slot.SetFilterable(true);
slot.SetPlayerFilter(new SlotFilter(...));
```

Hard filters are set by code and cannot be overridden by the player. Player filters are additional restrictions the player sets in the management UI. An item must pass both layers.

The `FilterConfigPanel` UI lets players configure their storage chest slots, driving a `SlotFilter` onto specific `ItemSlot` instances via network RPC.

---

### 3.4 StorageEntity

```csharp
public class StorageEntity : NetworkBehaviour, IItemSlotOwner
```

`StorageEntity` is the networked base for any world container: chests, station input/output trays, counters, etc.

**Core fields:**

|Field|Role|
|---|---|
|`SlotCount`|1–20 slots, Inspector-set|
|`DisplayRowCount`|UI layout hint (1–5 rows)|
|`AccessSettings`|Closed / SinglePlayerOnly / Full|
|`MaxAccessDistance`|Auto-closes UI if player moves away|
|`EmptyOnSleep`|Clears contents when player sleeps|
|`SlotsAreFilterable`|Whether player can set slot filters|

**Events:**

```csharp
public Action onOpened;
public Action onClosed;
public Action onContentsChanged;
```

**Key methods:**

```csharp
public bool CanItemFit(ItemInstance item, int quantity = 1)

// Called by server — syncs initial state to new client
public override void OnSpawnServer(NetworkConnection connection)

// Coroutine: keeps access-distance check alive while open
private IEnumerator UpdateWhileOpen()

// Useful for economy/value calculations
public Dictionary<StorableItemInstance, int> GetContentsDictionary()
```

The `UpdateWhileOpen` coroutine only runs while the storage UI is open, doing distance checks to auto-close it. This is a good example of FishNet + coroutine economy: don't run logic every frame; only run it when needed.

**Serialization:** `StorageEntity` does not implement `ISaveable` itself — it is the `WorldStorageEntity` subclass that adds save/load.

---

### 3.5 WorldStorageEntity & SurfaceEntity

```
StorageEntity  :  NetworkBehaviour, IItemSlotOwner
│
├── WorldStorageEntity
│       + Implements save/load via WorldStorageEntityData
│       + Persists slot contents and filters across sessions
│
└── SurfaceStorageEntity
        + For surface/shelf storage (items rest on a surface)
        + Handles physics-based item display
```

`WorldStorageEntity` is how most in-world containers (storage chests, counters, shelves) persist their state. It adds the `ISaveable` / `IGenericSaveable` integration and serializes each slot's item instance + filter configuration.

---

## 4. Station Framework

Stations are the production machinery (cauldron, mixing station, chemistry station, lab oven, brick press, drying rack). They use a **Module Composition** pattern instead of deep inheritance.

### 4.1 StationItem & ItemModule

```csharp
public class StationItem : MonoBehaviour
{
    public List<ItemModule> Modules;
    public List<ItemModule> ActiveModules { get; protected set; }

    public virtual void Initialize(StorableItemDefinition itemDefinition) { }

    public void ActivateModule<T>() where T : ItemModule { }
    public bool HasModule<T>() where T : ItemModule { }
    public T GetModule<T>() where T : ItemModule { }
}
```

`StationItem` is the physical representation of an item placed into a station (e.g., a beaker on the chemistry station). It is not the item's data — it's the 3D world object that responds to station operations.

`ItemModule` is the abstract base for components that extend a `StationItem`:

```csharp
public abstract class ItemModule : MonoBehaviour
{
    public StationItem Item { get; protected set; }
    public bool IsModuleActive { get; protected set; }

    public virtual void ActivateModule(StationItem item) { }
}
```

Concrete modules are activated selectively by the station: a drying rack might activate the `DryableModule`, a mixing station activates the `MixableModule`. This is the **Component/Module** pattern — behavior is composed, not inherited, so a single `StationItem` prefab can participate in multiple station types by enabling different modules.

---

### 4.2 StationRecipe

`StationRecipe` defines what a station can produce:

- Input item requirements (with quantities and filters)
- Output product definition + quantity
- Time required
- Required station type

`ProductDefinition` maintains `List<StationRecipe> Recipes` — all recipes that produce this drug. Recipes are added at runtime via `AddRecipe()` when products are initialized with their effect lists. This means the recipe list is **data-driven**: changing a product's effects changes which recipes are valid without any code changes.

The `ChemistryCookOperation` class tracks a recipe in progress:

```csharp
public class ChemistryCookOperation
{
    public string RecipeID;
    public EQuality ProductQuality;
    public Color StartLiquidColor;
    public float LiquidLevel;
    public int CurrentTime;

    public void Progress(int mins) { }
}
```

Minutes are the time unit — when the game clock advances, `Progress()` is called with elapsed minutes rather than relying on `Time.deltaTime`. This makes time-based progress framerate-independent and pausable.

---

### 4.3 Concrete Stations

Each station is a `NetworkBehaviour` (or `NetworkBehaviour` subclass) that:

1. Owns one or more `StorageEntity` input/output slots
2. Holds a reference to active `StationRecipe` or operation data
3. Has a Canvas UI (e.g., `ChemistryStationCanvas`, `MixingStationCanvas`)
4. Implements `IConfigurable` so employees can be assigned to it via the management system

Station hierarchy example:

```
NetworkBehaviour
└── ChemistryStation
        + InputSlots : StorageEntity (additive inputs)
        + OutputSlot : StorageEntity
        + ActiveOperation : ChemistryCookOperation
        + BunsenBurner : BunsenBurner (sub-component, dial-controlled heat)
        + implements IConfigurable
```

The canvas/UI is a separate MonoBehaviour that references the station. UI is always decoupled from the station logic itself — the station never references its canvas directly. The canvas subscribes to the station's events.

---

## 5. Product & Effects System

### 5.1 ProductDefinition

```csharp
public class ProductDefinition : PropertyItemDefinition, ISaveable
```

`ProductDefinition` extends the definition hierarchy all the way to represent a fully configured drug product. Key additions over `ItemDefinition`:

|Field|Role|
|---|---|
|`DrugTypes`|List of `DrugTypeContainer` — what drug category it belongs to|
|`BasePrice` / `MarketValue`|Economy values|
|`EffectsDuration`|How long effects last (in game minutes)|
|`BaseAddictiveness`|Base addiction probability|
|`ValidPackaging[]`|Ordered array of valid packaging (smallest to largest)|
|`Recipes`|Runtime-populated list of `StationRecipe`|
|`LawIntensityChange`|How much selling this moves the law heat|

It implements `ISaveable` because product definitions themselves carry state: the mix recipe that produces them is discovered/created by the player and persisted.

`GenerateAppearanceSettings()` is a virtual method overridden by subclasses (`WeedDefinition`, `CocaineDefinition`) to compute visual appearance (material colors) from the effect list. Effects literally change how the drug looks.

---

### 5.2 ProductItemInstance Hierarchy

```
ItemInstance
└── StorableItemInstance
    └── QualityItemInstance
        └── ProductItemInstance
                + PackagingID, AppliedPackaging
                + GetSimilarity(ProductDefinition, EQuality)
                + ApplyEffectsToNPC(NPC) / ClearEffectsFromNPC(NPC)
                + ApplyEffectsToPlayer(Player) / ClearEffectsFromPlayer(Player)
                │
                ├── WeedInstance
                ├── CocaineInstance
                ├── ShroomInstance
                │       Overrides ApplyEffectsToPlayer to start
                │       a psychedelic post-process coroutine
                └── LiquidMethInstance
```

`GetSimilarity(ProductDefinition other, EQuality quality)` computes how close this product is to what a customer wants — this drives the deal acceptance and happiness system without the customer needing exact matches.

---

### 5.3 Effect System

```csharp
public abstract class Effect : ScriptableObject
{
    public string Name, Description, ID;
    public int Tier;                     // 1–5
    public float Addictiveness;          // 0–1
    public Color ProductColor;           // tints the product appearance
    public Color LabelColor;
    public int ValueChange;              // flat price modifier
    public float ValueMultiplier;        // price multiplier
    public Vector2 MixDirection;         // used in mixing color math
    public float MixMagnitude;

    public abstract void ApplyToNPC(NPC npc);
    public abstract void ClearFromNPC(NPC npc);
    public abstract void ApplyToPlayer(Player player);
    public abstract void ClearFromPlayer(Player player);
}
```

`Effect` is an abstract ScriptableObject. Every unique drug effect is a separate asset in the project:

```
Shrinking.cs     → scales the character model down
Zombifying.cs    → switches NPC VO database to zombie sounds
Paranoia.cs      → triggers paranoia logic on player/NPC
Sneaky.cs        → reduces footstep volume, modifies visibility attribute
Refreshing.cs    → restores some stamina/energy stat
```

This is the **Strategy pattern via ScriptableObjects**: each effect encapsulates exactly what it does to a player and NPC, without the product class knowing the details. Adding a new effect is adding a new `.cs` + `.asset` file with zero changes to existing code.

The `MixDirection` / `MixMagnitude` fields drive the visual color mixing system: when effects are combined, their mix vectors are summed/blended to produce the product's final color.

---

## 6. NPC System

### 6.1 NPC Base

```csharp
public class NPC : NetworkBehaviour
```

The `NPC` class is the root of all characters (customers, police, cartel members, employees). Named NPC types like `Carl` simply extend `NPC` with character-specific overrides:

```csharp
public class Carl : NPC { }
```

Key subsystems owned by `NPC`:

- `NPCBehaviour` — the behaviour stack manager (separate component)
- `NPCScheduleManager` — runs the daily schedule of `NPCAction` instances
- `NPCMovement` — pathfinding wrapper (A*)
- `NPCAwareness` — vision/detection system
- `NPCHealth` — health, knockout, death states
- `NPCAnimation` — animator controller wrapper

These are all **separate components on the same GameObject**. NPC logic is composed from multiple focused MonoBehaviours rather than one giant god-class. Each can be tested, disabled, or swapped independently.

---

### 6.2 Behaviour System

```csharp
public class Behaviour : NetworkBehaviour
{
    public string Name;
    public int Priority;
    public bool EnabledOnAwake;

    public UnityEvent onEnable, onDisable, onBegin, onEnd;

    // Full lifecycle:
    public virtual void Enable() / Disable()
    public virtual void Activate() / Deactivate()
    public virtual void Pause() / Resume()
    public virtual void BehaviourUpdate()
    public virtual void BehaviourLateUpdate()
    public virtual void OnActiveTick()
}
```

`Behaviour` is the base class for all NPC runtime behaviors. Each concrete behavior is a separate component:

|Behaviour|Triggers|
|---|---|
|`CoweringBehaviour`|NPC is frightened by player weapon|
|`RagdollBehaviour`|NPC is hit/knocked down|
|`CallPoliceBehaviour`|NPC witnesses a crime|
|`CombatBehaviour`|NPC is in active combat|
|`FleeBehaviour`|NPC runs away from threat|
|`StationaryBehaviour`|NPC stands at a fixed point|
|`RequestProductBehaviour`|Customer wants to buy|
|`ConsumeProductBehaviour`|Customer consuming purchased product|
|`DeadBehaviour`|NPC is dead|
|`UnconsciousBehaviour`|NPC is knocked out|
|`WaterPotBehaviour`|Employee waters a pot|
|`MistMushroomBedBehaviour`|Employee tends mushroom beds|

**Lifecycle detail:**

```
Enabled ─(Enable)─► Active
                      │
                   (Activate) → BehaviourUpdate() runs each frame
                      │
                   (Pause) → BehaviourUpdate() pauses
                      │
                   (Resume) → resumes
                      │
                   (Deactivate) → stops updating
                      │
Enabled ─(Disable)─► Inactive
```

Each state change has a server-authority variant (`Enable_Server()`) and a networked broadcast variant (`Enable_Networked()`), keeping all clients in sync.

---

### 6.3 NPCAction & NPCSignal (Scheduler)

```csharp
[Serializable]
public abstract class NPCAction : NetworkBehaviour
{
    public int StartTime;        // in-game minutes from midnight
    public int Priority;

    // Template Method lifecycle — subclasses fill these in:
    public abstract string GetName();
    public abstract string GetTimeDescription();
    public abstract int GetEndTime();

    public virtual void Started() { }
    public virtual void LateStarted() { }
    public virtual void ActiveUpdate() { }
    public virtual void MinPassed() { }
    public virtual void Interrupt() { }
    public virtual bool ShouldStart() { return false; }
}
```

`NPCAction` represents a single scheduled task in an NPC's day. The `NPCScheduleManager` maintains a list of actions sorted by `StartTime` and activates them as the game clock advances.

`NPCSignal` extends `NPCAction` as a discrete one-shot command (rather than a time-range action):

```csharp
public class NPCSignal : NPCAction
{
    public int MaxDuration;
    public bool StartedThisCycle { get; protected set; }
}
```

Concrete signals:

```
NPCSignal_WalkToLocation    → pathfind to a target Transform
NPCSignal_DriveToCarPark    → drive a vehicle to a parking spot
NPCSignal_UseATM            → walk to ATM, perform interaction
NPCSignal_UseVendingMachine → walk to machine, buy item
NPCSignal_WaitForDelivery   → stand and wait at a location
```

**Pattern:** This is the **Command pattern** applied to NPC scheduling. Each signal encapsulates exactly one NPC task, with its own start condition (`ShouldStart()`), active update logic, and end condition (`GetEndTime()`). The scheduler does not need to know what any specific action does — it just calls the lifecycle hooks.

---

### 6.4 NPCBehaviour Stack

```csharp
public class NPCBehaviour : NetworkBehaviour
{
    public Behaviour activeBehaviour { get; set; }
    [SerializeField] protected List<Behaviour> behaviourStack;
    [SerializeField] private List<Behaviour> enabledBehaviours;

    public T GetBehaviour<T>() where T : Behaviour
    public void SortBehaviourStack()
}
```

The `behaviourStack` is ordered by `Priority` (higher = takes priority). Each frame, `NPCBehaviour` evaluates the enabled behaviours and activates the highest-priority one. When that behaviour ends, the next one takes over.

**Priority examples (higher number wins):**

```
DeadBehaviour           → 1000  (always wins)
RagdollBehaviour        → 900
CombatBehaviour         → 800
FleeBehaviour           → 700
CallPoliceBehaviour     → 600
CoweringBehaviour       → 500
RequestProductBehaviour → 200
StationaryBehaviour     → 100
WaterPotBehaviour       → 50
```

This is a **Priority-based State Machine**. There's no hardcoded state transition table. You add a new behaviour, give it a priority, enable it when relevant — the stack handles the rest.

The `SummonBehaviour` is special: it can be triggered externally (e.g. player uses management clipboard) via the `Summon()` server RPC, which routes through `NPCBehaviour` and pushes the summon onto the stack.

---

## 7. Management System (IConfigurable)

The management system is what lets the player assign employees to stations, set routes, configure storage filters, and name objects. It is built around the `IConfigurable` interface.

### 7.1 IConfigurable Interface

```csharp
public interface IConfigurable
{
    EntityConfiguration Configuration { get; }
    ConfigurationReplicator ConfigReplicator { get; }
    EConfigurableType ConfigurableType { get; }
    WorldspaceUIElement WorldspaceUI { get; set; }
    NetworkObject CurrentPlayerConfigurer { get; set; }
    Sprite TypeIcon { get; }
    Transform Transform { get; }
    Transform UIPoint { get; }
    bool CanBeSelected { get; }
    ScheduleOne.Property.Property ParentProperty { get; }

    WorldspaceUIElement CreateWorldspaceUI();
    void DestroyWorldspaceUI();
    void ShowOutline(Color color);
    void HideOutline();
    void Selected();
    void Deselected();
    void SetConfigurer(NetworkObject player);
    void SendConfigurationToClient(NetworkConnection conn);
}
```

Anything selectable in the management overlay implements this interface: stations, storage entities, employees, packaging stations. The `ManagementWorldspaceCanvas` only knows `IConfigurable` — it never references a specific station type.

When the player enters management mode and hovers over a configurable, `ManagementWorldspaceCanvas` calls `ShowOutline()` and `CreateWorldspaceUI()`. On selection, `Selected()` is called and the appropriate `ConfigPanel` is shown. Zero coupling to specific object types.

---

### 7.2 EntityConfiguration & ConfigField

```csharp
public class EntityConfiguration
{
    public List<ConfigField> Fields;
    public UnityEvent onChanged;
    public StringField Name { get; private set; }

    public T GetField<T>() where T : ConfigField
    public void ReplicateField(ConfigField field, NetworkConnection conn = null)
    public void ReplicateAllFields(NetworkConnection conn = null, bool replicateDefaults = true)
}
```

`EntityConfiguration` is the data container for a configurable's current settings. It holds a list of typed `ConfigField` instances:

```
ConfigField  (base)
│
├── StringField      → e.g. custom name for a storage chest
├── NumberField      → e.g. price setting on a deal
├── NPCField         → which employee is assigned to this station
├── ItemField        → which item should a station produce
├── RouteField       → delivery route assignment
└── ObjectListField  → list of objects (e.g. which bins a cleaner uses)
```

Each `ConfigField` holds a value and fires `UnityEvent` when changed. The `EntityConfiguration` calls `ReplicateField()` to sync changes to all clients via the `ConfigurationReplicator`.

**Generic field access:**

```csharp
// Typed field access on a chemist station config:
var npcField = config.GetField<NPCField>();
var itemField = config.GetField<ItemField>();
```

No casting, no magic strings.

---

### 7.3 ConfigPanel & UI Binding

```csharp
public class ConfigPanel : MonoBehaviour
{
    public virtual void Bind(List<EntityConfiguration> configs) { }
}
```

`ConfigPanel` is the UI panel that appears when the player selects a configurable in management mode. Subclasses like `CleanerConfigPanel` override `Bind()` to populate their specific fields:

```csharp
public class CleanerConfigPanel : ConfigPanel
{
    public ObjectFieldUI BedUI;
    public ObjectListFieldUI BinsUI;

    public override void Bind(List<EntityConfiguration> configs)
    {
        // Binds the NPC field, bed assignment, bin list to UI elements
    }
}
```

This is the **Template Method** pattern on the UI layer. The management system calls `Bind()` with the selected configurations; each panel knows how to display its own data type.

---

## 8. Law & Crime System

```csharp
[Serializable]
public class Crime
{
    public virtual string CrimeName { get; protected set; }
}
```

`Crime` is a minimal base class. Concrete crimes are simple subclasses:

```
Crime
├── DrugTrafficking
├── PossessingControlledSubstances
├── VehicularAssault
├── Assault
├── Murder
└── Trespassing
```

`PlayerCrimeData` tracks the player's active and historical crimes, wanted level, and offence notices. `LawActivitySettings` defines the law enforcement response for a given heat level:

```csharp
public class LawActivitySettings
{
    public PatrolInstance[] Patrols;
    public CheckpointInstance[] Checkpoints;
    public CurfewInstance[] Curfews;
    public VehiclePatrolInstance[] VehiclePatrols;
    public SentryInstance[] Sentries;

    public void Evaluate()   // activates/deactivates instances based on current heat
    public void End()
    public void OnLoaded()
}
```

`ProductDefinition.LawIntensityChange` feeds into the law heat system: selling more/better product increases law intensity, which escalates `LawActivitySettings` to higher tiers. The data flow is:

```
Player sells product
    → LawIntensityChange added to law heat
    → LawActivitySettings.Evaluate() picks the right patrol/checkpoint tier
    → Police NPCs respond with escalated behaviours
```

---

## 9. Singleton Infrastructure

The codebase has three levels of singleton, all using the self-referential generic constraint:

### Plain Singleton

```csharp
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; }
}
```

For non-networked game-local managers: `Map`, `SFXManager`, `CallManager`, `HeatmapManager`.

### Persistent Singleton

```csharp
public class PersistentSingleton<T> : Singleton<T>
```

Calls `DontDestroyOnLoad`. Used for `Registry` — must survive scene transitions.

### NetworkSingleton

```csharp
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    public static T Instance { get; protected set; }
    public static bool InstanceExists { get; }
}
```

For server-authoritative managers: `ShopManager`, `StorageManager`, `VehicleManager`. These are FishNet `NetworkBehaviour`s because they need to sync state across clients.

### PlayerSingleton

```csharp
public abstract class PlayerSingleton<T> : MonoBehaviour where T : PlayerSingleton<T>
```

For per-player UI systems. Used as the base for `App<T>`. Each player has their own phone app instances — they are not shared globally.

**Pattern value:** All three types have identical static `Instance` access patterns. Code that uses `Map.Instance` looks identical to code that uses `ShopManager.Instance`. The generic constraint means the returned type is always correct without casting.

---

## 10. Save & Persistence System

The save system uses a layered interface approach:

```csharp
// The broadest contract — any saveable object
public interface ISaveable
{
    string SaveFolderName { get; }
    string SaveFileName { get; }
    bool ShouldSaveUnderFolder { get; }
    List<string> LocalExtraFiles { get; set; }
    List<string> LocalExtraFolders { get; set; }
    bool HasChanged { get; set; }
    int LoadOrder { get; }

    void InitializeSaveable();
    string GetSaveString();
}

// For objects that already have a Loader paired with them
public interface IBaseSaveable : ISaveable
{
    Loader Loader { get; }
}

// For generic save without a typed loader
public interface IGenericSaveable
{
    string GetSaveString();
}
```

Key implementors:

- `ShopManager` → `IBaseSaveable, ISaveable`
- `ProductDefinition` → `ISaveable` (recipe discovery persists)
- `WorldStorageEntity` → save/load via `WorldStorageEntityData`
- `VendingMachine` → `IGUIDRegisterable, IGenericSaveable`

`IGUIDRegisterable` marks world objects that need a stable GUID across saves. The GUID is stored on spawn and used to match saved data back to the correct world object on load:

```csharp
public interface IGUIDRegisterable
{
    // Object registers itself with a GUID manager
    // GUID survives save/load cycles
}
```

Implementors: `VendingMachine`, `ATM`, `ParkingLot`, `NPCEnterableBuilding`.

**Data objects** (`WorldStorageEntityData`, `ProductData`, etc.) are simple serializable C# classes — plain data holders with no game logic. They act as the serialization DTO (Data Transfer Object) layer: game objects serialize to these → JSON → disk → JSON → data object → game object restores state.

---

## 11. Phone & App System

```csharp
public abstract class App<T> : PlayerSingleton<T> where T : PlayerSingleton<T>
{
    public enum EOrientation { Horizontal, Vertical }
    // Open/Close, orientation management, notification badges
}
```

All phone apps inherit `App<T>`. The generic + singleton combo means:

```csharp
// From anywhere in the game, open the map:
MapApp.Instance.Open();

// From anywhere, get the messages app:
MessagesApp.Instance.AddNotification(...);
```

Concrete apps:

```
App<T>
├── MapApp
├── MessagesApp
├── ContactsApp
├── DeliveryApp
├── DealerManagementApp
└── ProductManagerApp
```

Each app is a self-contained UI system. The `Phone` MonoBehaviour manages which app is open and handles transitions between them. Apps communicate via events and direct calls through their static `Instance` references.

---

## 12. Networking Layer (FishNet)

FishNet generates `RpcWriter`, `RpcLogic`, and `RpcReader` triplets for every `[ServerRpc]` and `[ObserversRpc]` method. These appear in decompiled output as:

```csharp
// The user-written method (you write this):
[ServerRpc(RequireOwnership = false)]
public void SendStock(string shopCode, string itemID, int stock) { }

// FishNet code-gen produces these automatically:
private void RpcWriter___Server_SendStock_15643032(string shopCode, string itemID, int stock) { }
public void RpcLogic___SendStock_15643032(string shopCode, string itemID, int stock) { }
private void RpcReader___Server_SendStock_15643032(PooledReader reader, Channel channel, NetworkConnection conn) { }
```

**This is not hand-written code** — it's generated by FishNet at compile time. Every `[ServerRpc]`, `[ObserversRpc]`, and `[TargetRpc]` gets this triplet.

**Common networking patterns used:**

|Pattern|Usage|
|---|---|
|`[ServerRpc(RequireOwnership = false)]`|Client asks server to do something (insert item, buy from shop)|
|`[ObserversRpc(RunLocally = true)]`|Server tells all clients (including itself) to update|
|`[TargetRpc]`|Server tells one specific client (initial state sync on join)|
|`OnSpawnServer(NetworkConnection conn)`|Server sends current state to newly connected client|

The `_internal = false` parameter pattern on `ItemSlot` methods prevents the local machine from re-broadcasting when it receives a network update — avoiding infinite loops.

---

## 13. Design Patterns — Full Reference

|Pattern|Where Used|How|
|---|---|---|
|**Template Method**|`NPCAction`, `Behaviour`, `App<T>`, `ConfigPanel`|Abstract base defines lifecycle skeleton; subclasses fill in hooks|
|**Factory Method**|`ItemDefinition.GetDefaultInstance()`|Each definition subclass returns the correct instance type|
|**Strategy (via SO)**|`Effect` ScriptableObjects|Each effect encapsulates apply/clear behavior; product holds a list|
|**Priority Stack / State Machine**|`NPCBehaviour.behaviourStack`|Sorted list of behaviours; highest-priority enabled one wins|
|**Command**|`NPCSignal` and its subclasses|Each signal encapsulates one NPC task; scheduler calls lifecycle hooks|
|**Composite Filters**|`ItemSlot.HardFilters + PlayerFilter`|Item must pass all filter layers; filters are composable objects|
|**Observer (Action delegates)**|`ItemSlot`, `StorageEntity`, `ItemInstance`|C# Action events; UI subscribes without coupling to storage logic|
|**Definition / Instance Split**|All item types|ScriptableObject = static data, C# class = runtime state|
|**Module / Composition**|`StationItem` + `ItemModule`|Station items compose behavior from typed modules rather than deep inheritance|
|**Generic Self-Referential Singleton**|`Singleton<T>`, `NetworkSingleton<T>`, `PlayerSingleton<T>`|Type-safe instance access without casting|
|**Generic Repository**|`Registry.GetItem<T>(string ID)`|Type-safe item lookup; no casting at call site|
|**Data Transfer Object**|`WorldStorageEntityData`, `ProductData`, `ItemData`|Clean serialization boundary between game objects and save files|
|**Interface Segregation**|`ISaveable`, `IBaseSaveable`, `IGenericSaveable`|Each interface is a minimal contract; implementors take only what they need|

---

## 14. Key Takeaways for Indie Devs

### 1. Always split Definition from Instance

Never put runtime state (quantity, quality, condition) in a ScriptableObject. ScriptableObjects are shared assets — mutating them at runtime causes bugs. Create a separate C# class for the runtime state and keep the SO as the immutable config.

### 2. Use abstract base classes for lifecycle contracts

When 10+ classes share the same lifecycle (enable, activate, start, update, end), put that lifecycle in an abstract base class. Subclasses implement what's different. The caller only knows the base — zero coupling to concrete types.

### 3. Use interfaces for cross-cutting capabilities

When unrelated classes need the same capability (can hold items → `IItemSlotOwner`, can be configured → `IConfigurable`, can be saved → `ISaveable`), use an interface. Don't force a common ancestor. This lets a class like `StorageEntity` be a container AND a configurable AND a saveable independently.

### 4. Priority behaviour stacks beat state machines

For NPC AI, a priority-sorted list of behaviours is simpler and more maintainable than a state machine with transition tables. Add a new behaviour, give it a priority, and the existing stack just works. No state graph to update.

### 5. Separate the data, the world presence, and the UI

Item data: C# class. World presence: MonoBehaviour/NetworkBehaviour. UI: separate MonoBehaviour subscribing to events from the data layer. These three layers should never directly reference each other in the wrong direction. UI subscribes to data events; it does not reach into world objects to read state.

### 6. Use C# Action delegates, not just UnityEvents

UnityEvents are great for designer-wired connections in the Inspector. C# Action delegates are faster, more flexible, and better for code-wired connections. `ItemSlot` uses Actions; the UI subscribes from code — clean and testable.

### 7. Make time game-clock-based, not frame-based

Production operations (`ChemistryCookOperation.Progress(int mins)`) use in-game minutes, not `Time.deltaTime`. This means operations are framerate-independent, work correctly when time is sped up, and can be saved/loaded exactly without float accumulation errors.

### 8. Generic repository lookup beats string-to-type dictionaries

`Registry.GetItem<WeedDefinition>("id")` is typed at the call site. No casts, no runtime type errors. Use the generic constraint to make your lookup systems self-documenting.

### 9. Network RPC patterns: write once, read the convention

FishNet (and Mirror, Netcode for GameObjects) all have similar patterns. Write your `[ServerRpc]` and `[ObserversRpc]` methods with clear intent. The `_internal` flag trick on methods to prevent rebroadcast loops is a widely applicable pattern in any authority-based multiplayer framework.

### 10. Namespace = module boundary

Each namespace is a logical module. Keep cross-namespace dependencies one-directional. If `UI` needs `ItemFramework`, that's fine. `ItemFramework` should not need `UI`. This discipline keeps the codebase modular and refactorable.

---

_Documentation generated from Schedule-1 decompiled source — for educational/learning purposes._