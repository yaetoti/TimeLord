using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-99)]
public sealed class RewindManager : MonoBehaviour {
  public enum RewindState {
    WAITING,
    SLOWING_DOWN,
    REWINDING,
    SPEEDING_UP
  }
  
  public sealed class StateChangedEventData {
    public RewindState PreviousState { get; private set; }
    public RewindState CurrentState { get; private set; }

    public StateChangedEventData(RewindState previousState, RewindState currentState) {
      PreviousState = previousState;
      CurrentState = currentState;
    }
  }

  public sealed class RollbackEventData {
    public RollbackEventData() {
    }
  }

  // Attributes
  public static RewindManager Instance { get; private set; }
  public RewindState State => m_state;
  public float FrameTime => m_frameTime;
  public float FrameProgress => m_frameProgress;
  /// <summary>
  /// Represents the current frame number in the timeline managed by the TimeManager.
  /// </summary>
  /// <remarks>
  /// The frame number is incremented on each frame, providing a unique identifier
  /// for the current state in the timeline. This property is often used for tracking
  /// events or snapshots tied to specific frames in systems that rely on time-based
  /// rewinding or replay mechanisms.
  /// </remarks>
  public ulong FrameNumber => m_frameNumber;
  public int BufferSizeLimit => m_bufferSizeLimit;
  public int Mana => m_mana;
  public int ManaLimit => m_manaLimit;

  // Serialized
  [SerializeField] [Min(0.001f)] private float m_frameTime = 0.02f;
  [SerializeField] [Min(0)] private float m_rewindSpeedModifier = 1.0f;
  [SerializeField] [Min(0)] private int m_bufferSizeLimit = 500;
  [SerializeField] [Min(0)] private float m_timeSlowingLength = 0.5f;
  [SerializeField] [Min(0)] private int m_manaLimit = 2000;

  // Events
  private UnityEvent m_rewindSaveEvent;
  private UnityEvent<RollbackEventData> m_rewindRollbackEvent;
  private UnityEvent<StateChangedEventData> m_stateChangedEvent;
  private UnityEvent m_rewindStartEvent; // TODO remove
  private UnityEvent m_rewindStopEvent; // TODO remove

  // TODO implement
  // - Normalized FrameProgress
  
  // - NormalTimeScale (general + current)
  // - RewindTimeScale (general + current)
  // - GoToFrameEvent
  // - ClearRewindHistoryEvent
  // - SLOWING_DOWN and SPEEDING_UP are absolute, but the process can be divided into two halves: slowing down (towards 0) and speeding up (towards -1).
  // - BeforeRollbackEvent and AfterRollbackEvent
  // - PauseRewind, ResumeRewind, IsRewinding, IsPaused
  
  // State
  private RewindState m_state = RewindState.WAITING;
  private float m_timeSlowingProgress;
  private int m_storedTicks; // Number of ticks stored in the buffer.
  private int m_mana; // Unused for now, but will be used for mana regeneration and other things.

  // Frame management
  private bool m_zeroUpdateFired; // Used to fire the RewindSaveEvent on the first update.
  private ulong m_frameNumber; // Unique identifier for the current frame.
  private float m_timeSinceLastFrame; // Used to track the time since the last update.
  private float m_frameProgress; // Used to track the progress of the current frame. Normalized [0; 1].
  
#region Lifecycle methods
  private void Awake() {
    Initialize();
    m_mana = m_manaLimit;
    
    // Events
    m_rewindSaveEvent = new UnityEvent();
    m_stateChangedEvent = new UnityEvent<StateChangedEventData>();
    m_rewindStartEvent = new UnityEvent();
    m_rewindStopEvent = new UnityEvent();
    m_rewindRollbackEvent = new UnityEvent<RollbackEventData>();
  }

  private void OnEnable() {
#if UNITY_EDITOR
    Initialize();
#endif
  }
  
  private void OnDestroy() {
    if (Instance != this) {
      return;
    }
    
    Instance = null;
    ForceResetRewind();
  }

  private void Update() {
    // Fire first RewindSaveEvent. TODO set execution order and remove. Be aware of Awake and OnDestroy listener leaks
    if (!m_zeroUpdateFired) {
      m_zeroUpdateFired = true;
      m_rewindSaveEvent.Invoke();
    }

    // Invoke RewindSaveEvents. Positive time flow.
    if (m_state is not RewindState.REWINDING) {
      m_timeSinceLastFrame += Time.unscaledDeltaTime;
      m_frameProgress = m_timeSinceLastFrame / FrameTime;
      
      // Next update tick
      while (m_frameProgress >= 1.0f) {
        // BeforeUpdate
        m_timeSinceLastFrame -= FrameTime;
        m_frameProgress = m_timeSinceLastFrame / FrameTime;
        if (m_frameProgress > 1.0f) {
          m_frameProgress = 1.0f;
        }
        
        ++m_frameNumber;

        if (m_storedTicks < m_bufferSizeLimit) {
          ++m_storedTicks;
        }
        
        // Update
        m_rewindSaveEvent.Invoke();
        
        // AfterUpdate
        m_frameProgress = 0.0f;
      }
    }

    
    // State handling
    switch (m_state) {
    case RewindState.SLOWING_DOWN:
      HandleSlowingDown();
      break;
    case RewindState.SPEEDING_UP:
      HandleSpeedingUp();
      break;
    case RewindState.REWINDING:
      HandleRewinding();
      break;
    case RewindState.WAITING:
      HandleWaiting();
      break;
    default:
      throw new ArgumentOutOfRangeException();
    }

    // Input handling
    HandleInput();
  }
  
#endregion

#region Public class methods

  public void ForceResetRewind() {
    if (m_state != RewindState.WAITING) {
      Time.timeScale = 1;
      m_rewindStopEvent.Invoke();
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.WAITING));
      m_state = RewindState.WAITING;
    }
  }
  
#endregion

#region Private class methods

  private void Initialize() {
    if (Instance == this) {
      return;
    }
    
    if (Instance is not null) {
      Destroy(this);
      return;
    }

    Instance = this;
  }

  // Positive time flow
  private void HandleSlowingDown() {
    m_timeSlowingProgress -= Time.unscaledDeltaTime;
    Time.timeScale = Mathf.Clamp01(m_timeSlowingProgress / m_timeSlowingLength);
    Time.fixedDeltaTime = 0.02f * Time.timeScale;
    if (m_timeSlowingProgress <= 0) {
      m_rewindStartEvent.Invoke();
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.REWINDING));
      m_state = RewindState.REWINDING;
    }
  }

  private void HandleSpeedingUp() {
    m_timeSlowingProgress += Time.unscaledDeltaTime;
    Time.timeScale = Mathf.Clamp01(m_timeSlowingProgress / m_timeSlowingLength);
    Time.fixedDeltaTime = Time.timeScale * 0.02f;
    if (m_timeSlowingProgress >= m_timeSlowingLength) {
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.WAITING));
      m_state = RewindState.WAITING;
    }
  }
  
  private void HandleWaiting() {
  }
  
  private void HandleRewinding() {
    // Invoke rollbacks
    m_timeSinceLastFrame -= Time.unscaledDeltaTime;
    while (m_timeSinceLastFrame < 0.0f) {
      m_timeSinceLastFrame += 0.02f;
      --m_frameNumber;
        
      --m_storedTicks;
      --m_mana;
        
      m_rewindRollbackEvent.Invoke(new RollbackEventData());
    }

    if (m_storedTicks <= 0 || m_mana <= 0) {
      m_rewindStopEvent.Invoke();
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.SPEEDING_UP));
      m_state = RewindState.SPEEDING_UP;
    }
  }
  
  private void HandleInput() {
    if (InputHolder.Instance.MasterInput.Player.TimeRewind.WasPressedThisFrame()) {
      switch (m_state) {
      case RewindState.WAITING:
        if (m_mana > 0) {
          m_timeSlowingProgress = m_timeSlowingLength;
          m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.SLOWING_DOWN));
          m_state = RewindState.SLOWING_DOWN;
        }

        break;
      case RewindState.SLOWING_DOWN:
        m_state = RewindState.SPEEDING_UP;
        break;
      case RewindState.REWINDING:
        m_rewindStopEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.SPEEDING_UP));
        m_state = RewindState.SPEEDING_UP;
        break;
      case RewindState.SPEEDING_UP:
        break;
      }
    }
  }

#endregion
  
#region Event methods
  
  public void Register(IRewindable component) {
    RegisterStateChangedEvent(component.OnRewindStateChanged);
    RegisterRewindStartEvent(component.OnRewindStart);
    RegisterRewindStopEvent(component.OnRewindStop);
    RegisterRewindRollbackEvent(component.OnRewindRollback);
    RegisterRewindSaveEvent(component.OnRewindSave);
  }

  public void Unregister(IRewindable component) {
    UnregisterStateChangedEvent(component.OnRewindStateChanged);
    UnregisterRewindStartEvent(component.OnRewindStart);
    UnregisterRewindStopEvent(component.OnRewindStop);
    UnregisterRewindRollbackEvent(component.OnRewindRollback);
    UnregisterRewindSaveEvent(component.OnRewindSave);
  }
  
  public void RegisterStateChangedEvent(UnityAction<StateChangedEventData> callback) {
    m_stateChangedEvent.AddListener(callback);
  }
  
  public void UnregisterStateChangedEvent(UnityAction<StateChangedEventData> callback) {
    m_stateChangedEvent.RemoveListener(callback);
  }
  
  public void RegisterRewindStartEvent(UnityAction callback) {
    m_rewindStartEvent.AddListener(callback);
  }

  public void UnregisterRewindStartEvent(UnityAction callback) {
    m_rewindStartEvent.RemoveListener(callback); 
  }

  public void RegisterRewindStopEvent(UnityAction callback) {
    m_rewindStopEvent.AddListener(callback);
  }
  
  public void UnregisterRewindStopEvent(UnityAction callback) {
    m_rewindStopEvent.RemoveListener(callback);
  }

  public void RegisterRewindSaveEvent(UnityAction callback) {
    m_rewindSaveEvent.AddListener(callback);
  }
  
  public void UnregisterRewindSaveEvent(UnityAction callback) {
    m_rewindSaveEvent.RemoveListener(callback);
  }
  
  public void RegisterRewindRollbackEvent(UnityAction<RollbackEventData> callback) {
    m_rewindRollbackEvent.AddListener(callback);
  }
  
  public void UnregisterRewindRollbackEvent(UnityAction<RollbackEventData> callback) {
    m_rewindRollbackEvent.RemoveListener(callback);
  }
  
#endregion
}

// Per-level singleton