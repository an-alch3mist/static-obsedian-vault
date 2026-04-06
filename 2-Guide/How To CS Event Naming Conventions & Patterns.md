
> Unity / C# reference — static bus, instance events, delegates, Actions

---

## 1. The Vocabulary

| Term            | Meaning                                                        |
| --------------- | -------------------------------------------------------------- |
| **Event**       | A field others can subscribe to but not invoke directly        |
| **Raise**       | The act of triggering the event (`Raise` prefix on the method) |
| **Subscribe**   | Attaching a listener (`+=`)                                    |
| **Unsubscribe** | Detaching a listener (`-=`)                                    |
| **Payload**     | Data passed through the event to every listener                |
| **Firer**       | The object that calls `Raise___()`                             |
| **Listener**    | The object that subscribed with `+=`                           |

---

## 2. Naming Pattern — The Core Rule

```
event field  →  On[Subject][Verb]       e.g. OnCustomerEntered
raise method →  Raise[Subject][Verb]    e.g. RaiseCustomerEntered
handler      →  Handle[Subject][Verb]   e.g. HandleCustomerEntered
              or On[Subject][Verb]      e.g. OnCustomerEntered  (on the listener class)
```

The prefix tells you the role at a glance:

|Prefix|Role|Who uses it|
|---|---|---|
|`On`|Subscription point|Everyone subscribes to this|
|`Raise`|Trigger method|Only the firer calls this|
|`Handle`|Listener method|The receiver's private handler|

---

## 3. Action Variants

### 3a. `Action` — no payload

```csharp
// No data needed. Just "this happened."
public static event Action OnStoreOpened;
public static void RaiseStoreOpened() => OnStoreOpened?.Invoke();
```

```
StoreManager ──RaiseStoreOpened()──► bus ──► CustomerSpawner.HandleStoreOpened()
                                         └──► UIManager.HandleStoreOpened()
```

Use when: the event itself is the entire message. No "who" or "what" needed.

---

### 3b. `Action<T>` — one payload

```csharp
// One piece of data. "This happened, here is what/who."
public static event Action<CustomerAgent> OnCustomerEntered;
public static void RaiseCustomerEntered(CustomerAgent c) =>
    OnCustomerEntered?.Invoke(c);
```

```
CustomerFSM ──RaiseCustomerEntered(Customer_03)──► bus
                                                    ├──► StoreManager   → c = Customer_03
                                                    └──► DebugLogger    → c = Customer_03
```

Use when: listeners need to know _which_ object triggered the event.

---

### 3c. `Action<T1, T2>` — two payloads

```csharp
// Two pieces of data. "This happened, here is who and what."
public static event Action<CustomerAgent, ItemData> OnItemPurchased;
public static void RaiseItemPurchased(CustomerAgent c, ItemData item) =>
    OnItemPurchased?.Invoke(c, item);
```

```
CustomerFSM ──RaiseItemPurchased(Customer_03, Item_Milk)──► bus
                                                             ├──► StoreManager  → revenue += item.price
                                                             └──► ReceiptPrinter → print c.name + item.name
```

Use when: listeners need two independent pieces of context.

---

### 3d. `Action<T1, T2, T3>` — three payloads

```csharp
// Three pieces. Beyond this, use an EventArgs struct instead (see section 6).
public static event Action<CustomerAgent, ItemData, float> OnPurchaseCompleted;
public static void RaisePurchaseCompleted(CustomerAgent c, ItemData item, float price) =>
    OnPurchaseCompleted?.Invoke(c, item, price);
```

Use when: three tightly related values need to travel together. Hard limit — four or more payloads → use a struct (section 6).

---

### 3e. `Action` with no raise wrapper (raw invoke)

```csharp
// Sometimes you skip the raise method entirely.
// Acceptable for internal/private events on an instance.
public event Action OnHealthDepleted;

// caller just invokes directly — no Raise wrapper
OnHealthDepleted?.Invoke();
```

Use when: the event is private/internal to a class and no external caller needs to trigger it. The `Raise` wrapper is only needed when outside code needs to fire it through the bus.

---

## 4. Static Bus vs Instance Event

### Static bus (GameEvents pattern)

```csharp
// ── GameEvents.cs ────────────────────────────────────────────
public static class GameEvents
{
    public static event Action<CustomerAgent> OnCustomerEntered;
    public static void RaiseCustomerEntered(CustomerAgent c) =>
        OnCustomerEntered?.Invoke(c);
}

// ── Firer (CustomerFSM.cs) ───────────────────────────────────
// External agent calls Raise on the bus
GameEvents.RaiseCustomerEntered(this);

// ── Listener (StoreManager.cs) ───────────────────────────────
private void OnEnable()  => GameEvents.OnCustomerEntered += HandleCustomerEntered;
private void OnDisable() => GameEvents.OnCustomerEntered -= HandleCustomerEntered;

private void HandleCustomerEntered(CustomerAgent c)
{
    _count++;
}
```

```
[CustomerFSM]                [GameEvents]             [StoreManager]
     │                            │                         │
     │──RaiseCustomerEntered()───►│                         │
     │                            │──HandleCustomerEntered─►│
     │                            │                         │ _count++
```

**When to use:** any cross-system event where the firer and listener have no direct reference to each other.

---

### Instance event (object owns the event)

```csharp
// ── CustomerAgent.cs ─────────────────────────────────────────
public class CustomerAgent : MonoBehaviour
{
    // Instance event — each CustomerAgent has its own
    public event Action<float> OnHealthChanged;

    private float _health = 100f;

    public void TakeDamage(float amount)
    {
        _health -= amount;
        OnHealthChanged?.Invoke(_health); // raised internally by the owner
    }
}

// ── HealthBar.cs ─────────────────────────────────────────────
// Must have a reference to the specific agent to subscribe
private void Start()
{
    _agent.OnHealthChanged += HandleHealthChanged;
}

private void HandleHealthChanged(float newHealth)
{
    _slider.value = newHealth / 100f;
}
```

```
[CustomerAgent instance]           [HealthBar]
         │                              │
         │ TakeDamage(20f)              │
         │   └─ OnHealthChanged(80f) ──►│
         │                              │ slider.value = 0.8f
```

**When to use:** when the listener already holds a reference to the specific object and cares only about that one object's state.

---

## 5. Internal vs External Raise

### Raised internally by the owner

```csharp
public class Door : MonoBehaviour
{
    // Event declared and raised by the same class
    public event Action OnOpened;

    public void Open()
    {
        // ... open animation ...
        OnOpened?.Invoke(); // owner raises its own event
    }
}
```

```
[Door]──Open()──► [Door internal]──OnOpened──► [SoundManager]
                                           └──► [LightController]
```

The event owner controls when it fires. Outsiders can only subscribe, never invoke it directly.

---

### Raised externally via a bus method

```csharp
// GameEvents.cs — bus method is public so any class can call it
public static void RaiseCustomerEntered(CustomerAgent c) =>
    OnCustomerEntered?.Invoke(c);

// CustomerFSM.cs — external caller triggers the bus
GameEvents.RaiseCustomerEntered(this);
```

```
[CustomerFSM] ──calls──► [GameEvents.RaiseCustomerEntered]
                                    │
                          OnCustomerEntered.Invoke(c)
                                    │
                         ┌──────────┴───────────┐
                         ▼                       ▼
                   [StoreManager]         [AnalyticsLogger]
```

The key difference: the event field (`OnCustomerEntered`) is never directly touched by `CustomerFSM`. It only calls the `Raise` method. Listeners cannot accidentally trigger it — only firers with access to the `Raise` method can.

---

## 6. EventArgs Struct — four or more payloads

When you need more than three values, pack them into a struct:

```csharp
// ── PurchaseEventArgs.cs ─────────────────────────────────────
public struct PurchaseEventArgs
{
    public CustomerAgent Customer;
    public ItemData      Item;
    public float         Price;
    public int           QueuePosition;
    public float         WaitDuration;
}

// ── GameEvents.cs ────────────────────────────────────────────
public static event Action<PurchaseEventArgs> OnPurchaseCompleted;
public static void RaisePurchaseCompleted(PurchaseEventArgs args) =>
    OnPurchaseCompleted?.Invoke(args);

// ── Firer ────────────────────────────────────────────────────
GameEvents.RaisePurchaseCompleted(new PurchaseEventArgs
{
    Customer      = this,
    Item          = _currentItem,
    Price         = 2.50f,
    QueuePosition = 0,
    WaitDuration  = 14.3f
});

// ── Listener ─────────────────────────────────────────────────
private void HandlePurchaseCompleted(PurchaseEventArgs args)
{
    _revenue      += args.Price;
    _totalWait    += args.WaitDuration;
}
```

---

## 7. Full Naming Cheat Sheet

```
Scenario                          event field              raise method
─────────────────────────────────────────────────────────────────────────
No payload                        OnStoreOpened            RaiseStoreOpened
One object                        OnCustomerEntered        RaiseCustomerEntered
One object + data                 OnItemPurchased          RaiseItemPurchased
State change on a specific obj    OnHealthChanged          RaiseHealthChanged
Something failed                  OnPathfindingFailed      RaisePathfindingFailed
Something completed               OnQuestCompleted         RaiseQuestCompleted
Timer elapsed                     OnDayEnded               RaiseDayEnded
External trigger (player)         OnStoreClosedByPlayer    RaiseStoreClosedByPlayer
```

```
Listener handler method naming
─────────────────────────────────────────────────────────────────────────
On the listener class:   Handle[Subject][Verb]
e.g.                     HandleCustomerEntered
                         HandleItemPurchased
                         HandleDayEnded
```

---

## 8. Subscribe / Unsubscribe Pattern

Always pair `+=` with `-=`. Forgetting to unsubscribe leaks the listener into destroyed objects.

```csharp
// ── Correct pattern ──────────────────────────────────────────
private void OnEnable()
{
    GameEvents.OnCustomerEntered    += HandleCustomerEntered;
    GameEvents.OnPurchaseCompleted  += HandlePurchaseCompleted;
}

private void OnDisable()
{
    GameEvents.OnCustomerEntered    -= HandleCustomerEntered;
    GameEvents.OnPurchaseCompleted  -= HandlePurchaseCompleted;
}
```

```
OnEnable  ──+=──► subscriber registered
OnDisable ──-=──► subscriber removed   ← if you skip this,
                                          destroyed objects
                                          stay subscribed and
                                          throw MissingReferenceException
```

Use `OnEnable` / `OnDisable` for MonoBehaviours. Use constructor / `Dispose` for plain C# classes.

---

## 9. Quick Decision Tree

```
Do you need to communicate between two systems
that have no direct reference to each other?
    │
    YES ──► static bus (GameEvents)
    │           Raise from firer
    │           Subscribe in listener OnEnable
    │
    NO
    │
    └── Does the listener already hold a reference
        to the specific object?
            │
            YES ──► instance event on that object
            │           public event Action<T> OnXyz
            │           owner raises internally
            │
            NO
            │
            └── Is this internal to one class only?
                    │
                    YES ──► private event, raise internally
                    │       no Raise wrapper needed
                    │
                    NO  ──► reconsider — you probably
                            want a static bus
```