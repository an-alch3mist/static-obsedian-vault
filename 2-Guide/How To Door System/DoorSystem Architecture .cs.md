```cs
using System;
using UnityEngine;
// impoved via: https://claude.ai/share/8f49bd28-aa38-4ce4-93b0-56aee148448a

// ============================================================================
// ENUMS - Stable API that won't break existing code when adding new states
// ============================================================================

public enum DoorState
{
	Closed,
	Opening,
	Opened,
	Closing,
	Swaying // Supernatural entity control
}

public enum DoorLockState
{
	Locked,
	Unlocked,
	UnlockedJammed // Cannot be locked by player - always unlocked
}

public enum LockSide
{
	Inside,
	Outside,
	Both // For common locks (keypad, gate handle)
}

public enum DoorActionResult
{
	Success,
	Blocked,                // Door is supernaturally blocked
	Locked,                 // Attempted open but door is locked
	AlreadyInState,         // Already in target state (e.g. already open)
	AnimationInProgress,    // Another action is animating - prevents spam
	WrongLockType,          // Tried LockSide.Both on separate locks or vice versa
	ObstructionDetected     // Object blocking door closure
}

// named exact same inside animatorCOntroller too.
public enum DoorAnimParamType
{
	// trigger
	doorOpen,
	doorClose,
	doorSwayStart,
	doorSwayStop,
	doorLockedJiggle,
	doorBlockedJiggle,

	lockInside,
	lockOutside,
	lockCommon,
	unlockInside,
	unlockOutside,
	unlockCommon,

	// boolean
	isDoorOpen,
	isInsideLocked,
	isOutsideLocked,
	isDoorSwaying,
}

public enum AnimationEventType
{
	// Door movement events
	DoorOpeningStarted,
	DoorOpeningComplete,
	DoorClosingStarted,
	DoorClosingComplete,

	// Inside lock events
	InsideLockingStarted,
	InsideLockingComplete,
	InsideUnlockingStarted,
	InsideUnlockingComplete,

	// Outside lock events
	OutsideLockingStarted,
	OutsideLockingComplete,
	OutsideUnlockingStarted,
	OutsideUnlockingComplete,

	// Common lock events (for keypad/gate locks)
	CommonLockingStarted,
	CommonLockingComplete,
	CommonUnlockingStarted,
	CommonUnlockingComplete,

	// Supernatural events
	DoorSwayStarted,
	DoorSwayStopped
}

// ============================================================================
// INTERFACE - Public contract for all door types
// ============================================================================

public interface IDoor
{
	// ========================================================================
	// READ-ONLY PROPERTIES - State can only change through Try* methods
	// ========================================================================

	bool IsBlocked { get; }           // Supernatural block - cannot open/close
	bool UsesCommonLock { get; }      // true = single lock (keypad/gate), false = separate inside/outside
	bool CanBeLocked { get; }         // false = door can never be locked
	bool IsAnimating { get; }         // CRITICAL: Prevents action spam and state corruption

	DoorState CurrentState { get; }
	DoorLockState InsideLockState { get; }
	DoorLockState OutsideLockState { get; }

	// ========================================================================
	// EVENTS - Enables loosely-coupled systems (audio, AI, quests)
	// ========================================================================

	event Action<DoorState> OnDoorStateChanged;
	event Action<DoorLockState, LockSide> OnLockStateChanged; // LockSide indicates which lock changed

	// ========================================================================
	// INITIALIZATION - Syncs animator to match inspector-set initial state
	// ========================================================================

	/// <summary>
	/// Synchronizes animator controller state to match inspector-serialized values.
	/// MUST be called from MonoBehaviour.Awake() to ensure door visuals match initial state.
	/// Example: If inspector shows door Opened + InsideLocked, animator will immediately 
	/// show door in open position with inside handle in locked position (no animation played).
	/// </summary>
	void Init();

	// ========================================================================
	// PUBLIC API - All interactions go through these methods
	// ========================================================================

	/// <summary>
	/// Attempt to open the door. Will auto-unlock if needed and both locks are unlocked.
	/// Returns AnimationInProgress if animation is running - prevents spam-clicking.
	/// </summary>
	DoorActionResult TryOpen();

	/// <summary>
	/// Attempt to close the door. Retries up to MaxCloseRetries if obstructed.
	/// After max retries, forces door closed (slam shut).
	/// </summary>
	DoorActionResult TryClose();

	/// <summary>
	/// Unified lock method. Use LockSide.Both for common locks, Inside/Outside for separate locks.
	/// Returns WrongLockType if side doesn't match door's lock configuration.
	/// </summary>
	DoorActionResult TryLock(LockSide side);

	/// <summary>
	/// Unified unlock method. Use LockSide.Both for common locks, Inside/Outside for separate locks.
	/// Returns WrongLockType if side doesn't match door's lock configuration.
	/// </summary>
	DoorActionResult TryUnlock(LockSide side);

	/// <summary>
	/// Supernatural ability - blocks door from being opened/closed.
	/// Lock state remains unchanged.
	/// </summary>
	DoorActionResult TryBlock();

	/// <summary>
	/// Supernatural ability - removes block on door.
	/// </summary>
	DoorActionResult TryUnblock();

	/// <summary>
	/// Supernatural ability - makes door sway back and forth.
	/// Forces both locks to unlock state and plays looping sway animation.
	/// Block state remains unchanged (can sway while blocked).
	/// </summary>
	DoorActionResult TryStartSwaying();

	/// <summary>
	/// Stops door swaying and transitions to targetState (Opened or Closed).
	/// Typically called when player looks away or after timer.
	/// </summary>
	DoorActionResult TryStopSwaying(DoorState targetState = DoorState.Opened);

	// ========================================================================
	// ANIMATION CALLBACK - Called by DoorAnimationEventForwarder
	// ========================================================================

	/// <summary>
	/// Type-safe callback from Animation Events. Sets IsAnimating = false at appropriate times.
	/// Handles state transitions and chained actions (e.g. unlock → open).
	/// </summary>
	void OnAnimationComplete(AnimationEventType eventType);

	// ========================================================================
	// CONFIGURATION
	// ========================================================================

	int MaxCloseRetries { get; set; } // Default = 5 - how many times to retry closing before forcing
}
```


## DoorHinged.cs
```cs
using System;
using UnityEngine;
using SPACE_UTIL;

/// <summary>
/// Hinged door implementation with animator-driven animations.
/// Supports separate inside/outside locks OR single common lock (keypad/gate).
/// All state changes flow through Try* methods → Animator → OnAnimationComplete callbacks.
/// </summary>
public class DoorHinged : MonoBehaviour, IDoor
{
	// ========================================================================
	// INSPECTOR CONFIGURATION - Serialized initial state
	// ========================================================================
	
	[Header("Door Configuration")]
	[Tooltip("Set initial door state - animator will sync to match at game start")]
	[SerializeField] private DoorState initialDoorState = DoorState.Opened;

	[Tooltip("Set initial inside lock state")]
	[SerializeField] private DoorLockState initialInsideLockState = DoorLockState.Locked;

	[Tooltip("Set initial outside lock state")]
	[SerializeField] private DoorLockState initialOutsideLockState = DoorLockState.Unlocked;

	[Space(10)]
	[Tooltip("True = single lock (keypad/gate), False = separate inside/outside locks")]
	[SerializeField] private bool usesCommonLock = false;

	[Tooltip("False = door can never be locked (always unlocked)")]
	[SerializeField] private bool canBeLocked = true;

	[Tooltip("Number of retry attempts before forcing door closed when obstructed")]
	[SerializeField] private int maxCloseRetries = 5;

	[Header("References")]
	[SerializeField] private Animator animator;

	[Tooltip("Layer mask for objects that block door closure (e.g. 'Box' layer)")]
	[SerializeField] private LayerMask obstructionLayer;

	// ========================================================================
	// RUNTIME STATE - Current door state (private backing fields)
	// ========================================================================

	private DoorState _currentState;
	private DoorLockState _insideLockState;
	private DoorLockState _outsideLockState;
	private bool _isBlocked = false;
	private bool _isAnimating = false;

	// Internal chaining state - when unlock animation completes, auto-open if this is true
	private bool _pendingOpen = false;

	// Close retry tracking
	private int _closeRetryCount = 0;

	// Sway stop target
	private DoorState _targetStateAfterSway = DoorState.Opened;

	// ========================================================================
	// INTERFACE PROPERTIES - Public read-only access to state
	// ========================================================================

	public bool IsBlocked => _isBlocked;
	public bool UsesCommonLock => usesCommonLock;
	public bool CanBeLocked => canBeLocked;
	public bool IsAnimating => _isAnimating;

	public DoorState CurrentState => _currentState;
	public DoorLockState InsideLockState => _insideLockState;
	public DoorLockState OutsideLockState => _outsideLockState;

	public int MaxCloseRetries
	{
		get => maxCloseRetries;
		set => maxCloseRetries = Mathf.Max(1, value); // Ensure at least 1 retry
	}

	// ========================================================================
	// EVENTS - Decoupled notification system
	// ========================================================================

	public event Action<DoorState> OnDoorStateChanged;
	public event Action<DoorLockState, LockSide> OnLockStateChanged;

	// ========================================================================
	// UNITY LIFECYCLE
	// ========================================================================

	private void Awake()
	{
		// Validate references
		if (animator == null)
		{
			
			Debug.LogError($"[DoorHinged] Animator not assigned on {gameObject.name}!", this);
			enabled = false;
			return;
		}

		// CRITICAL: Initialize state from inspector values
		Init();
	}

	private void OnTriggerStay(Collider other)
	{
		// Only check during closing animation
		if (_currentState != DoorState.Closing) return;
		// if (!obstructionLayer.Contains(other.gameObject.layer)) return;
		if (!obstructionLayer.contains(gameObject)) return;

		// Obstruction detected during close
		_closeRetryCount++;

		if (_closeRetryCount < maxCloseRetries)
		{
			// Reverse back to opening - door bounces back
			Debug.Log($"[DoorHinged] Obstruction detected, retry {_closeRetryCount}/{maxCloseRetries}");
			animator.SetTrigger("doorOpen");
			_currentState = DoorState.Opening;
		}
		else
		{
			// Force close after max retries (door slams shut regardless of obstruction)
			Debug.Log($"[DoorHinged] Max retries reached, forcing door closed");
			_closeRetryCount = 0;
			// Let closing animation continue without interruption
		}
	}

	// ========================================================================
	// INITIALIZATION - Syncs animator to inspector state WITHOUT playing animations
	// ========================================================================

	public void Init()
	{
		// Copy inspector values to runtime state
		_currentState = initialDoorState;
		_insideLockState = initialInsideLockState;
		_outsideLockState = initialOutsideLockState;

		// Sync animator bools to match initial state - this makes animator jump to correct idle states
		// WITHOUT playing transition animations (bools set initial layer states)
		animator.SetBool("isDoorOpen", _currentState == DoorState.Opened);
		animator.SetBool("isInsideLocked", _insideLockState == DoorLockState.Locked);
		animator.SetBool("isOutsideLocked", _outsideLockState == DoorLockState.Locked);
		animator.SetBool("isDoorSwaying", false);

		// Force animator to evaluate immediately so door appears in correct state on first frame
		animator.Update(0f);

		Debug.Log($"[DoorHinged] Initialized - State: {_currentState}, InsideLock: {_insideLockState}, OutsideLock: {_outsideLockState}");
	}

	// ========================================================================
	// PUBLIC API - Door Open/Close
	// ========================================================================

	public DoorActionResult TryOpen()
	{
		// Validation checks
		if (_isAnimating) return DoorActionResult.AnimationInProgress;
		if (_isBlocked) return DoorActionResult.Blocked;
		if (_currentState == DoorState.Opened) return DoorActionResult.AlreadyInState;
		if (_currentState == DoorState.Swaying) return DoorActionResult.Blocked; // Can't open while swaying

		// Check if door is locked - need to unlock first
		bool insideLocked = _insideLockState == DoorLockState.Locked;
		bool outsideLocked = _outsideLockState == DoorLockState.Locked;

		if (insideLocked || outsideLocked)
		{
			// Can't open locked door - caller should unlock first
			return DoorActionResult.Locked;
		}

		// All checks passed - trigger opening animation
		_isAnimating = true;
		_currentState = DoorState.Opening;
		animator.SetBool("isDoorOpen", true);
		animator.SetTrigger("doorOpen");

		OnDoorStateChanged?.Invoke(DoorState.Opening);
		return DoorActionResult.Success;
	}

	public DoorActionResult TryClose()
	{
		// Validation checks
		if (_isAnimating) return DoorActionResult.AnimationInProgress;
		if (_isBlocked) return DoorActionResult.Blocked;
		if (_currentState == DoorState.Closed) return DoorActionResult.AlreadyInState;
		if (_currentState == DoorState.Swaying) return DoorActionResult.Blocked; // Can't close while swaying

		// Reset retry counter for new close attempt
		_closeRetryCount = 0;

		// Trigger closing animation
		_isAnimating = true;
		_currentState = DoorState.Closing;
		animator.SetBool("isDoorOpen", false);
		animator.SetTrigger("doorClose");

		OnDoorStateChanged?.Invoke(DoorState.Closing);
		return DoorActionResult.Success;
	}

	// ========================================================================
	// PUBLIC API - Lock/Unlock (Unified Interface)
	// ========================================================================

	public DoorActionResult TryLock(LockSide side)
	{
		if (_isAnimating) return DoorActionResult.AnimationInProgress;
		if (!canBeLocked) return DoorActionResult.Blocked; // Door cannot be locked

		// Common lock path (keypad/gate with single lock mechanism)
		if (usesCommonLock)
		{
			if (side != LockSide.Both) return DoorActionResult.WrongLockType; // Must use LockSide.Both
			if (_insideLockState == DoorLockState.Locked) return DoorActionResult.AlreadyInState;

			_isAnimating = true;
			animator.SetTrigger("lockCommon");
			return DoorActionResult.Success;
		}

		// Separate locks path (traditional door with inside/outside handles)
		if (side == LockSide.Both) return DoorActionResult.WrongLockType; // Can't lock both separately

		if (side == LockSide.Inside)
		{
			if (_insideLockState == DoorLockState.Locked) return DoorActionResult.AlreadyInState;
			_isAnimating = true;
			animator.SetBool("isInsideLocked", true);
			animator.SetTrigger("lockInside");
		}
		else // LockSide.Outside
		{
			if (_outsideLockState == DoorLockState.Locked) return DoorActionResult.AlreadyInState;
			_isAnimating = true;
			animator.SetBool("isOutsideLocked", true);
			animator.SetTrigger("lockOutside");
		}

		return DoorActionResult.Success;
	}

	public DoorActionResult TryUnlock(LockSide side)
	{
		if (_isAnimating) return DoorActionResult.AnimationInProgress;

		// Common lock path
		if (usesCommonLock)
		{
			if (side != LockSide.Both) return DoorActionResult.WrongLockType;
			if (_insideLockState == DoorLockState.Unlocked) return DoorActionResult.AlreadyInState;

			_isAnimating = true;
			animator.SetTrigger("unlockCommon");
			return DoorActionResult.Success;
		}

		// Separate locks path
		if (side == LockSide.Both) return DoorActionResult.WrongLockType;

		if (side == LockSide.Inside)
		{
			if (_insideLockState == DoorLockState.Unlocked) return DoorActionResult.AlreadyInState;
			_isAnimating = true;
			animator.SetBool("isInsideLocked", false);
			animator.SetTrigger("unlockInside");
		}
		else // LockSide.Outside
		{
			if (_outsideLockState == DoorLockState.Unlocked) return DoorActionResult.AlreadyInState;
			_isAnimating = true;
			animator.SetBool("isOutsideLocked", false);
			animator.SetTrigger("unlockOutside");
		}

		return DoorActionResult.Success;
	}

	// ========================================================================
	// PUBLIC API - Supernatural Abilities
	// ========================================================================

	public DoorActionResult TryBlock()
	{
		if (_isBlocked) return DoorActionResult.AlreadyInState;
		_isBlocked = true;
		return DoorActionResult.Success;
	}

	public DoorActionResult TryUnblock()
	{
		if (!_isBlocked) return DoorActionResult.AlreadyInState;
		_isBlocked = false;
		return DoorActionResult.Success;
	}

	public DoorActionResult TryStartSwaying()
	{
		if (_isAnimating) return DoorActionResult.AnimationInProgress;
		if (_currentState == DoorState.Swaying) return DoorActionResult.AlreadyInState;

		// Supernatural override - force unlock both sides (no animation played, instant)
		_insideLockState = DoorLockState.Unlocked;
		_outsideLockState = DoorLockState.Unlocked;
		animator.SetBool("isInsideLocked", false);
		animator.SetBool("isOutsideLocked", false);

		// Start swaying
		_isAnimating = true;
		_currentState = DoorState.Swaying;
		animator.SetBool("isDoorSwaying", true);
		animator.SetTrigger("doorSwayStart");

		OnDoorStateChanged?.Invoke(DoorState.Swaying);
		return DoorActionResult.Success;
	}

	public DoorActionResult TryStopSwaying(DoorState targetState = DoorState.Opened)
	{
		if (_currentState != DoorState.Swaying) return DoorActionResult.AlreadyInState;

		// Store target state for OnAnimationComplete callback
		_targetStateAfterSway = targetState;

		animator.SetBool("isDoorSwaying", false);
		animator.SetTrigger("doorSwayStop");

		// Animation will handle transition to target state
		return DoorActionResult.Success;
	}

	// ========================================================================
	// ANIMATION CALLBACK - Called by DoorAnimationEventForwarder component
	// ========================================================================

	public void OnAnimationComplete(AnimationEventType eventType)
	{
		switch (eventType)
		{
			// ----------------------------------------------------------------
			// Door Movement Events
			// ----------------------------------------------------------------
			case AnimationEventType.DoorOpeningStarted:
				// Opening animation started (optional callback)
				break;
			case AnimationEventType.DoorOpeningComplete:
				_isAnimating = false;
				_currentState = DoorState.Opened;
				animator.SetBool("isDoorOpen", true);  // ← ADD THIS
				OnDoorStateChanged?.Invoke(DoorState.Opened);
				break;

			case AnimationEventType.DoorClosingComplete:
				_isAnimating = false;
				_currentState = DoorState.Closed;
				animator.SetBool("isDoorOpen", false);  // ← ADD THIS
				OnDoorStateChanged?.Invoke(DoorState.Closed);
				break;

			case AnimationEventType.DoorClosingStarted:
				// Closing animation started (optional callback)
				break;

			// ----------------------------------------------------------------
			// Inside Lock Events
			// ----------------------------------------------------------------
			case AnimationEventType.InsideLockingStarted:
				// Locking animation started (optional callback)
				break;

			case AnimationEventType.InsideLockingComplete:
				_isAnimating = false;
				_insideLockState = DoorLockState.Locked;
				OnLockStateChanged?.Invoke(DoorLockState.Locked, LockSide.Inside);
				break;

			case AnimationEventType.InsideUnlockingStarted:
				// Unlocking animation started (optional callback)
				break;

			case AnimationEventType.InsideUnlockingComplete:
				_isAnimating = false;
				_insideLockState = DoorLockState.Unlocked;
				OnLockStateChanged?.Invoke(DoorLockState.Unlocked, LockSide.Inside);

				// CHAINING: If open was attempted while locked, auto-open now
				if (_pendingOpen)
				{
					_pendingOpen = false;
					TryOpen();
				}
				break;

			// ----------------------------------------------------------------
			// Outside Lock Events
			// ----------------------------------------------------------------
			case AnimationEventType.OutsideLockingStarted:
				// Locking animation started (optional callback)
				break;

			case AnimationEventType.OutsideLockingComplete:
				_isAnimating = false;
				_outsideLockState = DoorLockState.Locked;
				OnLockStateChanged?.Invoke(DoorLockState.Locked, LockSide.Outside);
				break;

			case AnimationEventType.OutsideUnlockingStarted:
				// Unlocking animation started (optional callback)
				break;

			case AnimationEventType.OutsideUnlockingComplete:
				_isAnimating = false;
				_outsideLockState = DoorLockState.Unlocked;
				OnLockStateChanged?.Invoke(DoorLockState.Unlocked, LockSide.Outside);

				// CHAINING: If open was attempted while locked, auto-open now
				if (_pendingOpen)
				{
					_pendingOpen = false;
					TryOpen();
				}
				break;

			// ----------------------------------------------------------------
			// Common Lock Events (keypad/gate)
			// ----------------------------------------------------------------
			case AnimationEventType.CommonLockingStarted:
				// Locking animation started (optional callback)
				break;

			case AnimationEventType.CommonLockingComplete:
				_isAnimating = false;
				_insideLockState = DoorLockState.Locked;
				_outsideLockState = DoorLockState.Locked;
				OnLockStateChanged?.Invoke(DoorLockState.Locked, LockSide.Both);
				break;

			case AnimationEventType.CommonUnlockingStarted:
				// Unlocking animation started (optional callback)
				break;

			case AnimationEventType.CommonUnlockingComplete:
				_isAnimating = false;
				_insideLockState = DoorLockState.Unlocked;
				_outsideLockState = DoorLockState.Unlocked;
				OnLockStateChanged?.Invoke(DoorLockState.Unlocked, LockSide.Both);

				// CHAINING: If open was attempted while locked, auto-open now
				if (_pendingOpen)
				{
					_pendingOpen = false;
					TryOpen();
				}
				break;

			// ----------------------------------------------------------------
			// Supernatural Events
			// ----------------------------------------------------------------
			case AnimationEventType.DoorSwayStarted:
				// Sway loop started (optional callback)
				break;

			case AnimationEventType.DoorSwayStopped:
				_isAnimating = false;
				_currentState = _targetStateAfterSway;
				// animator.SetBool("isDoorOpen", _targetStateAfterSway == DoorState.Opened);
				OnDoorStateChanged?.Invoke(_targetStateAfterSway);
				break;

			default:
				Debug.LogWarning($"[DoorHinged] Unhandled animation event: {eventType}");
				break;
		}
	}
}
```

## DoorInteraction.cs

```cs
using UnityEngine;
using SPACE_UTIL;

/// <summary>
/// Simple keyboard-based door interaction system.
/// Attach to a GameObject in the scene (e.g. GameManager or Player).
/// Handles open/close/lock/unlock based on player position relative to door triggers.
/// </summary>
public class DoorInteraction : MonoBehaviour
{
	[Header("Door Reference")]
	[Tooltip("Drag the GameObject with DoorHinged component here")]
	[SerializeField] private DoorHinged door;

	[Header("Trigger References")]
	[Tooltip("Drag the 'door inside trigger' GameObject here")]
	[SerializeField] private Collider insideTrigger;

	[Tooltip("Drag the 'door outside trigger' GameObject here")]
	[SerializeField] private Collider outsideTrigger;

	[Header("Player Reference")]
	[Tooltip("Drag the player GameObject here (works with CharacterController, Rigidbody, or any GameObject)")]
	[SerializeField] private GameObject player;

	// Runtime state tracking
	private bool _isPlayerInside = false;
	private bool _isPlayerOutside = false;

	// ========================================================================
	// UNITY LIFECYCLE
	// ========================================================================

	private void Awake()
	{
		// Validate all required references
		if (door == null)
		{
			Debug.LogError("[DoorInteraction] Door reference not assigned!", this);
			enabled = false;
			return;
		}

		if (insideTrigger == null || outsideTrigger == null)
		{
			Debug.LogError("[DoorInteraction] Trigger references not assigned!", this);
			enabled = false;
			return;
		}

		if (player == null)
		{
			Debug.LogError("[DoorInteraction] Player reference not assigned!", this);
			enabled = false;
			return;
		}

		// Ensure triggers are marked as triggers
		if (!insideTrigger.isTrigger)
		{
			Debug.LogWarning("[DoorInteraction] Inside trigger is not marked as trigger! Fixing...", insideTrigger);
			insideTrigger.isTrigger = true;
		}

		if (!outsideTrigger.isTrigger)
		{
			Debug.LogWarning("[DoorInteraction] Outside trigger is not marked as trigger! Fixing...", outsideTrigger);
			outsideTrigger.isTrigger = true;
		}

		// Subscribe to door state events for logging
		door.OnDoorStateChanged += OnDoorStateChanged;
		door.OnLockStateChanged += OnLockStateChanged;
	}

	private void OnDestroy()
	{
		// Unsubscribe from events to prevent memory leaks
		if (door != null)
		{
			door.OnDoorStateChanged -= OnDoorStateChanged;
			door.OnLockStateChanged -= OnLockStateChanged;
		}
	}

	private void Update()
	{
		// Update player position relative to triggers
		UpdatePlayerPosition();

		// Handle keyboard input
		HandleInput();
	}

	// ========================================================================
	// PLAYER POSITION TRACKING
	// ========================================================================

	private void UpdatePlayerPosition()
	{
		if (player == null) return;

		// Check if player is inside either trigger
		_isPlayerInside = insideTrigger.bounds.Contains(player.transform.position);
		_isPlayerOutside = outsideTrigger.bounds.Contains(player.transform.position);
	}

	// ========================================================================
	// INPUT HANDLING
	// ========================================================================

	private void HandleInput()
	{
		// KeyCode.O - Open door
		if (Input.GetKeyDown(KeyCode.O))
		{
			HandleOpenDoor();
		}

		// KeyCode.C - Close door
		if (Input.GetKeyDown(KeyCode.C))
		{
			HandleCloseDoor();
		}

		// KeyCode.L - Lock door (side-specific)
		if (Input.GetKeyDown(KeyCode.L))
		{
			HandleLockDoor();
		}

		// KeyCode.U - Unlock door (side-specific)
		if (Input.GetKeyDown(KeyCode.U))
		{
			HandleUnlockDoor();
		}
	}

	// ========================================================================
	// DOOR ACTIONS
	// ========================================================================

	private void HandleOpenDoor()
	{
		Debug.Log(C.method(this, "yellow", adMssg: "Attempting to open door"));

		DoorActionResult result = door.TryOpen();

		switch (result)
		{
			case DoorActionResult.Success:
				Debug.Log(C.method(this, "green", adMssg: "Door opening..."));
				break;

			case DoorActionResult.Locked:
				Debug.Log(C.method(this, "red", adMssg: "Door is locked! Unlock it first with KeyCode.U"));
				break;

			case DoorActionResult.AlreadyInState:
				Debug.Log(C.method(this, "orange", adMssg: "Door is already open"));
				break;

			case DoorActionResult.AnimationInProgress:
				Debug.Log(C.method(this, "orange", adMssg: "Door is currently animating, please wait"));
				break;

			case DoorActionResult.Blocked:
				Debug.Log(C.method(this, "red", adMssg: "Door is supernaturally blocked!"));
				break;

			default:
				Debug.Log(C.method(this, "red", adMssg: $"Failed to open door: {result}"));
				break;
		}
	}

	private void HandleCloseDoor()
	{
		Debug.Log(C.method(this, "yellow", adMssg: "Attempting to close door"));

		DoorActionResult result = door.TryClose();

		switch (result)
		{
			case DoorActionResult.Success:
				Debug.Log(C.method(this, "green", adMssg: "Door closing..."));
				break;

			case DoorActionResult.AlreadyInState:
				Debug.Log(C.method(this, "orange", adMssg: "Door is already closed"));
				break;

			case DoorActionResult.AnimationInProgress:
				Debug.Log(C.method(this, "orange", adMssg: "Door is currently animating, please wait"));
				break;

			case DoorActionResult.Blocked:
				Debug.Log(C.method(this, "red", adMssg: "Door is supernaturally blocked!"));
				break;

			default:
				Debug.Log(C.method(this, "red", adMssg: $"Failed to close door: {result}"));
				break;
		}
	}

	private void HandleLockDoor()
	{
		// Determine which side to lock based on player position
		LockSide side = GetPlayerLockSide();

		if (side == LockSide.Both)
		{
			Debug.Log(C.method(this, "red", adMssg: "You must be inside or outside a trigger to lock the door!"));
			return;
		}

		Debug.Log(C.method(this, "yellow", adMssg: $"Attempting to lock door from {side} side"));

		DoorActionResult result = door.TryLock(side);

		switch (result)
		{
			case DoorActionResult.Success:
				Debug.Log(C.method(this, "green", adMssg: $"Locking door from {side} side..."));
				break;

			case DoorActionResult.AlreadyInState:
				Debug.Log(C.method(this, "orange", adMssg: $"{side} lock is already locked"));
				break;

			case DoorActionResult.AnimationInProgress:
				Debug.Log(C.method(this, "orange", adMssg: "Door is currently animating, please wait"));
				break;

			case DoorActionResult.Blocked:
				Debug.Log(C.method(this, "red", adMssg: "Door cannot be locked (canBeLocked = false)"));
				break;

			case DoorActionResult.WrongLockType:
				Debug.Log(C.method(this, "red", adMssg: "Wrong lock type! Check if door uses common lock"));
				break;

			default:
				Debug.Log(C.method(this, "red", adMssg: $"Failed to lock door: {result}"));
				break;
		}
	}

	private void HandleUnlockDoor()
	{
		// Determine which side to unlock based on player position
		LockSide side = GetPlayerLockSide();

		if (side == LockSide.Both)
		{
			Debug.Log(C.method(this, "red", adMssg: "You must be inside or outside a trigger to unlock the door!"));
			return;
		}

		Debug.Log(C.method(this, "yellow", adMssg: $"Attempting to unlock door from {side} side"));

		DoorActionResult result = door.TryUnlock(side);

		switch (result)
		{
			case DoorActionResult.Success:
				Debug.Log(C.method(this, "green", adMssg: $"Unlocking door from {side} side..."));
				break;

			case DoorActionResult.AlreadyInState:
				Debug.Log(C.method(this, "orange", adMssg: $"{side} lock is already unlocked"));
				break;

			case DoorActionResult.AnimationInProgress:
				Debug.Log(C.method(this, "orange", adMssg: "Door is currently animating, please wait"));
				break;

			case DoorActionResult.WrongLockType:
				Debug.Log(C.method(this, "red", adMssg: "Wrong lock type! Check if door uses common lock"));
				break;

			default:
				Debug.Log(C.method(this, "red", adMssg: $"Failed to unlock door: {result}"));
				break;
		}
	}

	// ========================================================================
	// HELPER METHODS
	// ========================================================================

	private LockSide GetPlayerLockSide()
	{
		if (door.UsesCommonLock)
		{
			// Common lock doors require LockSide.Both
			return (_isPlayerInside || _isPlayerOutside) ? LockSide.Both : LockSide.Both;
		}

		// Separate locks - determine based on trigger position
		if (_isPlayerInside && !_isPlayerOutside)
			return LockSide.Inside;
		else if (_isPlayerOutside && !_isPlayerInside)
			return LockSide.Outside;
		else
			return LockSide.Both; // Invalid state - not in either trigger
	}

	// ========================================================================
	// EVENT CALLBACKS - For logging state changes
	// ========================================================================

	private void OnDoorStateChanged(DoorState newState)
	{
		Debug.Log(C.method(this, "cyan", adMssg: $"Door state changed to: {newState}"));
	}

	private void OnLockStateChanged(DoorLockState newLockState, LockSide side)
	{
		Debug.Log(C.method(this, "cyan", adMssg: $"Lock state changed: {side} is now {newLockState}"));
	}

	// ========================================================================
	// DEBUG VISUALIZATION
	// ========================================================================

	private void OnDrawGizmos()
	{
		if (insideTrigger != null)
		{
			Gizmos.color = new Color(0, 1, 0, 0.3f); // Green
			Gizmos.DrawCube(insideTrigger.bounds.center, insideTrigger.bounds.size);
		}

		if (outsideTrigger != null)
		{
			Gizmos.color = new Color(1, 0, 0, 0.3f); // Red
			Gizmos.DrawCube(outsideTrigger.bounds.center, outsideTrigger.bounds.size);
		}

		if (player != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(player.transform.position, 0.5f);
		}
	}
}
```