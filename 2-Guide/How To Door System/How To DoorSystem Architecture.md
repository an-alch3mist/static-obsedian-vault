### High-Level Architecture
```
┌─────────────────────────────────────────────────────────┐
│  IDoor (interface)                                      │
│  - DoorState State { get; }                             │
│  - bool TryOpen() / TryClose() / TryUnlock()            │
│  - event Action<DoorState> OnStateChanged               │
└─────────────────────────────────────────────────────────┘
                        ▲
                        │ inherits
┌─────────────────────────────────────────────────────────┐
│  DoorBase : MonoBehaviour (abstract)                    │
│  - Animator animator                                    │
│  - DoorConfigSO config                                  │
│  - Coroutine autoCloseRoutine                           │
│  - IEnumerator OpenRoutine() / CloseRoutine()           │
│  - bool IsObstructed()                                  │
└─────────────────────────────────────────────────────────┘
           ▲                    ▲                   ▲
           │                    │                   │
  ┌────────┴────────┐  ┌────────┴────────┐  ┌───────┴────────┐
  │ DoorHinged      │  │ DoorSliding     │  │ DoorDouble     │
  │ (rotate pivot)  │  │ (translate pos) │  │ (sync 2 doors) │
  └─────────────────┘  └─────────────────┘  └────────────────┘
```

### the hierarchy should look somthng like
the thing is there is 2 ways lock system, 

in a nut shell here's animations/state

```cs
public enum DoorState
{
    opening,
    opened,
    closing,
    closed,
    swaying, // x -> done by super natural entity
}
public enum DoorLockState
{
    locked,
    unLocked,
    unLockedJammed, // -> door cannot be locked by user its alawys unlocked
}
public interface IDoor // -> (critique)
{
    bool blocked; // x -> (cannot open if its blocked, shall be done by supernatural entity)
    bool usesCommonLock; // if this is enabled meaning the door uses a lock which can be opened from inside or outside(example keyPadLock with password, or a gate), in those case you can just play doorAnimationInside or outside any things is fine since there is only one common handle
    DoorState doorState;
    DoorLockState doorLockStateInside;
    DoorLockState doorLockStateOutside;

    void init();
        // based on above fields: usesCommonLock; doorState; doorLockStateInside; doorLockStateOutside;
        // the inital door state(when door is viewed for the first time), lockstate in visual shall be set:
        // say: if use common lock is true: 
        //      doorState: .closed
        //      doorLockStateInside: .unLocked
        //      doorLockStateInside: .locked
        // the animation triggers shall be set to that by default(at start of game) it shall look as door is closed with doorLockStateInside, doorLockStateInside as you see here
        // so make sure later you design animator controller in such a way so that the visual shall be satisfied as above.

    bool TryLockInside(); 
        // can be done be regardless of this.blocked
        // can be done only if bool usesCommonLock == false and canBeLocked == true
        // -> can be done only if doorLockStateInside != .locked
        // -> than animate lockingInsideAnim to lockedInsideAnim
        // -> no interuption until animation complete
        // -> and than set state to .locked

    bool TryLockOutside();
        // can be done be regardless of this.blocked
        // can be done only if bool usesCommonLock == false and canBeLocked == true
        // -> can be done only if doorLockStateOutside != .locked
        // -> than animate lockingOutsideAnim to lockedOutsideAnim
        // -> no interuption until animation complete
        // -> and than set state to .locked

    bool TryLockCommon();
        // can be done be regardless of this.blocked 
        // can be done only if bool usesCommonLock == true and canBeLocked == true
        // can be done if either of locks are == .unLocked // usually when using common
        // than animate commonLockingAnim to commonLockedIdleAnime
        // both lock shall be set to == .Locked if 

    bool TryUnLockInside(); 
        // can be done be regardless of this.blocked
        // can be done only if bool usesCommonLock == false
        // -> can be done only if doorLockStateInside != .unLocked
        // -> than animate unLockingOutsideAnim to unLockedOutsideAnim
        // -> no interuption until animation complete
        // -> and than set state to .unlocked
    
    bool TryUnLockOutside();
        // can be done be regardless of this.blocked
        // can be done only if bool usesCommonLock == false
        // -> can be done only if doorLockStateOutside != .unLocked
        // -> than animate unLockingInsideAnim to unLockedInsideAnim
        // -> no interuption until animation complete
        // -> and than set state to .unlocked 

    bool TryUnLockCommon();
        // can be done be regardless of this.blocked
        // can be done only if bool usesCommonLock == true
        // can be done if either of locks are == .locked
        // than animate commonUnLockingAnim to commonUnLockedIdleAnime
        // both lock shall be set to == .unLocked if 

    bool TryOpen(); 
        // cannot be done be this.blocked == true
        // -> can be done from inside/ouside(it depends or it is independedent if this.CommonLock == true) only if doorState == .closed && doorLockStateOutside == .unLocked  && doorLockStateInside == .unLocked
        // -> than animate unlock from doorInside/Outside(depends on where user is)(if locked) and play unlocking animation(inside or outside it depends) by TryUnLock(Inside/Outside)and than set the doorLock(Inside/Outside) state to .unlocked  and play opening animation, set state to .opening
        // -> no interuption until animation complete
        // -> and than set state to .opened 

    int maxTriesBeforeForceClose = 5;
    bool TryClose();
        // cannot be done be this.blocked == true
        // -> can be done from (inside/outside independent regardless where user is) only if doorState == .opened
        // -> than animate play (unlocking animation for both inside, outside lock if they locked), and than play closing animation, set state to .closing
        // -> no interuption until animation complete, execpt it could could be if it encounter a rigit body while closing, (say a boxCollider with rigid body(layerMask box))
                // ./Box/(scale:0.3 | MeshFilter | MeshRenderer, Rigidbody, BoxCollider) -> LayerMask(box)
                // this make it go back to opening animation -> opened state
                // btw there is number of tries being set say 5 tries after that it shall force close
        // -> and than set state to .closed

    bool TryBlock(); // ->(shall be done by supernatual entity),
    bool TryUnBlock(); // -> unblock the door if blocked

    bool TryDoorSwaying(); // ->(shall be done by supernatual entity),
        // blocked stays intact if it was blocked before
        // -> reagrdless of blocked or locked the door shall get (uncloked both inside and outside along with thier animation played)
        // -> shall sway the door doorSwayLoopAnim(); 
        // -> interuption made through TryDoorStopSwaying()

    bool TryDoorStopSwaying(bool DoorState doorStateAfterSopSwaying == DoorState.opened); // ->(shall be done by supernatual entity),  this maybe when user gets up or 2s after look at facing door, 
        // blocked stays intact if it was blocked before
        // and shall lead to door open from there, and door state shall be .opened or closed based on parameter of this function
}
/*
./Anim/
├ door/
│ ├ doorBlockedClosedJiggle.anim (anim | 1.00s | 60fps)
│ ├ doorBlockedOpenedJiggle.anim (anim | 0.08s | 60fps) // x (since blocked by super natural entity) -> when door is blocked when doorState == .opened, and user try to close the door this is played)
│ ├ doorClosedIdleAnim.anim (anim | 0.00s | 60fps)
│ ├ doorClosingAnim.anim (anim | 0.50s | 60fps)
│ ├ doorLockedClosedJiggle.anim (anim | 0.17s | 60fps) // maybe cause door is blocked(when closed) or due to door is locked from outside (or inside/outside if common locked)
│ ├ doorOpenedIdleAnim.anim (anim | 0.00s | 60fps)
│ ├ doorOpeningAnim.anim (anim | 0.67s | 60fps)
│ └ doorSwayLoopAnim.anim (anim | 0.38s | 60fps)
└ doorHandle/
  ├ commonLockedIdleAnim.anim (anim | 0.00s | 60fps)
  ├ commonLockingAnim.anim (anim | 0.67s | 60fps)
  ├ commonUnlockedIdle.anim (anim | 0.00s | 60fps)
  ├ commonUnlockingAnim.anim (anim | 0.52s | 60fps)
  ├ insideLockedIdle.anim (anim | 0.00s | 60fps)
  ├ insideLockingAnim.anim (anim | 0.67s | 60fps)
  ├ insideUnlockedIdle.anim (anim | 0.00s | 60fps)
  ├ insideUnlockingAnim.anim (anim | 0.58s | 60fps)
  ├ outsideLockedIdle.anim (anim | 0.00s | 60fps)
  ├ outsideLockingAnim.anim (anim | 0.83s | 60fps)
  ├ outsideUnlockedIdle.anim (anim | 0.00s | 60fps)
  └ outsideUnlockingAnim.anim (anim | 0.50s | 60fps)

anim parameters ->.... feel free  crtique/rewrite add more only if you think required ?
  doorOpen (trigger) = false
  doorClose (trigger) = false
  doorBlockedJiggle (trigger) = false
  doorLockedJiggle (trigger) = false
  isDoorOpen (bool) = false
  lockInside (trigger) = false
  unlockInside (trigger) = false
  lockOutside (trigger) = false
  unlockOutside (trigger) = false

*/

/* 
animator controller flow your suggestion ? along with parameters weather trigger/bool
by i think when its common lock, the tryUnLockInside or tryUnLock trigger can still be done in animatorController since they both linked to same animationAnim (doorLockingCommonAnim, doorUnLockingCommonAnim)
*/
// if you ever required(highly require to check weather certain animation is complete) use animationEvent function approach
```


### Also, Here's reformed hierarchy: -> fixed hierarchy architecture (no modification)
```scene-hierarchy
=== Component Abbreviations ===
dmc = MeshFilter | MeshRenderer
bc = BoxCollider
anim = Animator
================================

./doorHinged/(scale:1.0 | anim)
├ trigger (scale:1.0 | no components)
│ ├ door outside trigger (scale:1.0 | bc)
│ └ door inside trigger  (scale:1.0 | bc)
├ door (scale:1.0 | no components) -> to be animated for doorOpen, Close, Sway, etc
│ ├ frame(visual + collider) (scale:(1.0,2.0,0.1) | dmc, bc)
│ ├ handleOutsideLock(visual + collider) (scale:(0.1,0.2,0.1) | dmc, bc)
│ └ handleInsideLock(visual + collider) (scale:(0.1,0.2,0.1) | dmc, bc)
└ doorFrame (scale:1.0 | no components)
  ├ visual frame left(as viewed from outside) (scale:(0.1,2.0,0.1) | dmc, bc)
  └ visual frame right(as viewed from outside) (scale:(0.1,2.0,0.1) | dmc, bc)
```


```animatorController-hierarchy --> feel free to critique, reqrute
=== Animator Controller: doorOpenCloseAnimController_stateMachineApproach ===

Parameters:
  doorOpen (trigger) = false
  doorClose (trigger) = false
  doorBlockedJiggle (trigger) = false
  doorLockedJiggle (trigger) = false
  isDoorOpen (bool) = false
  lockInside (trigger) = false
  unlockInside (trigger) = false
  isInsideLocked (bool) = false
  lockOutside (trigger) = false
  unlockOutside (trigger) = false
  isOutsideLocked (bool) = false

Animation Layers (3):
├ -> Layer 0: Layer 0: Door Movemenet(Base Layer)
│   Weight: 0.00 | Blending: Override | IK: False | Sync: None

│   Entry:
│     └ (default transition) → doorClosedIdleAnim(The Default State)
│   States Info (9):
│   ├ New State | Motion: (no motion) | Speed: 1.00x
│   ├ doorBlockedClosedJiggle | Motion: doorBlockedClosedJiggle | Speed: 1.00x
│   │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → doorClosedIdleAnim
│   ├ doorBlockedOpenedJiggle | Motion: doorBlockedOpenedJiggle | Speed: 1.00x
│   │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → doorOpenedIdleAnim
│   ├ doorClosedIdleAnim | Motion: doorClosedIdleAnim | Speed: 1.00x [DEFAULT]
│   │ ├ [doorOpen = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → doorOpeningAnim
│   │ ├ [doorLockedJiggle = true] (hasExitTime:☑ | exitTime:0.75 | transition duration:0.25s) → doorLockedClosedJiggle
│   │ └ [doorBlockedJiggle = true] (hasExitTime:☑ | exitTime:0.75 | transition duration:0.25s) → doorBlockedClosedJiggle
│   ├ doorClosingAnim | Motion: doorClosingAnim | Speed: 1.00x
│   │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → doorClosedIdleAnim
│   ├ doorLockedClosedJiggle | Motion: doorLockedClosedJiggle | Speed: 1.00x
│   │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → doorClosedIdleAnim
│   ├ doorOpenedIdleAnim | Motion: doorOpenedIdleAnim | Speed: 1.00x
│   │ ├ [doorClose = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → doorClosingAnim
│   │ └ [doorBlockedJiggle = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → doorBlockedOpenedJiggle
│   ├ doorOpeningAnim | Motion: doorOpeningAnim | Speed: 1.00x
│   │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → doorOpenedIdleAnim
│   └ doorSwayLoopAnim | Motion: doorSwayLoopAnim | Speed: 1.00x

├ -> Layer 1: LAYER 1: Inside Lock (Weight: 1.0, Additive Blending)
│   Weight: 1.00 | Blending: Additive | IK: False | Sync: None

│   Entry:
│     └ (default transition) → insideUnlockedIdle(The Default State)
│   States Info (4):
│   ├ insideLockedIdle | Motion: insideLockedIdle | Speed: 1.00x
│   │ └ [unlockInside = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → insideUnlockingAnim
│   ├ insideLockingAnim | Motion: insideLockingAnim | Speed: 1.00x
│   │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → insideLockedIdle
│   ├ insideUnlockedIdle | Motion: insideUnlockedIdle | Speed: 1.00x [DEFAULT]
│   │ └ [lockInside = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → insideLockingAnim
│   └ insideUnlockingAnim | Motion: insideUnlockingAnim | Speed: 1.00x
│     └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → insideUnlockedIdle

└ -> Layer 2: LAYER 2: Outside Lock (Weight: 1.0, Additive Blending)
    Weight: 1.00 | Blending: Additive | IK: False | Sync: None

    Entry:
      └ (default transition) → outsideUnlockedIdle(The Default State)
    States Info (4):
    ├ outsideLockedIdle | Motion: outsideLockedIdle | Speed: 1.00x
    │ └ [unlockOutside = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → outsideUnlockingAnim
    ├ outsideLockingAnim | Motion: outsideLockingAnim | Speed: 1.00x
    │ └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → outsideLockedIdle
    ├ outsideUnlockedIdle | Motion: outsideUnlockedIdle | Speed: 1.00x [DEFAULT]
    │ └ [lockOutside = true] (hasExitTime:☐ | exitTime:0.75 | transition duration:0.01s) → outsideLockingAnim
    └ outsideUnlockingAnim | Motion: outsideUnlockingAnim | Speed: 1.00x
      └ [auto] (hasExitTime:☑ | exitTime:1.00 | transition duration:0.01s) → outsideUnlockedIdle
```

