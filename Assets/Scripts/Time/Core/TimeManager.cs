using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-100)]
public class TimeManager : MonoBehaviour {
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
  public static TimeManager Instance { get; private set; }
  public RewindState State => m_state;
  public float FrameTime => m_frameTime;
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
  private float m_frameTimer; // Used to track the time since the last update.

  
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
    if (!m_zeroUpdateFired) {
      m_zeroUpdateFired = true;
      m_rewindSaveEvent.Invoke();
    }

    // Invoke updates
    if (m_state is not RewindState.REWINDING) {
      m_frameTimer += Time.unscaledDeltaTime;
      // Next update tick
      while (m_frameTimer >= 0.02f) {
        m_frameTimer -= 0.02f;
        ++m_frameNumber;
        
        if (m_storedTicks < m_bufferSizeLimit) {
          ++m_storedTicks;
        }
        
        m_rewindSaveEvent.Invoke();
      }
    }

    
    // Processing
    switch (m_state) {
    case RewindState.SLOWING_DOWN:
      m_timeSlowingProgress -= Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingProgress / m_timeSlowingLength);
      Time.fixedDeltaTime = 0.02f * Time.timeScale;
      if (m_timeSlowingProgress <= 0) {
        m_rewindStartEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.REWINDING));
        m_state = RewindState.REWINDING;
      }

      break;
    case RewindState.SPEEDING_UP:
      m_timeSlowingProgress += Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingProgress / m_timeSlowingLength);
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      if (m_timeSlowingProgress >= m_timeSlowingLength) {
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.WAITING));
        m_state = RewindState.WAITING;
      }

      break;
    case RewindState.REWINDING:
      // Invoke rollbacks
      m_frameTimer -= Time.unscaledDeltaTime;
      while (m_frameTimer < 0.0f) {
        m_frameTimer += 0.02f;
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

      break;
    case RewindState.WAITING:
      break;
    }

    // Input
    HandleInput();
  }
  
#endregion

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

  public void ForceResetRewind() {
    if (m_state != RewindState.WAITING) {
      Time.timeScale = 1;
      m_rewindStopEvent.Invoke();
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, RewindState.WAITING));
      m_state = RewindState.WAITING;
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

#region Event methods
  
  public void Register(IRewindable component) {
    RegisterStateChangedEvent(component.OnRewindStateChanged);
    RegisterRewindStartEvent(component.OnRewindStart);
    RegisterRewindStopEvent(component.OnRewindStop);
    RegisterRewindStepEvent(component.OnRewindRollback);
    m_rewindSaveEvent.AddListener(component.OnRewindSave);
  }

  public void Unregister(IRewindable component) {
    UnregisterStateChangedEvent(component.OnRewindStateChanged);
    UnregisterRewindStartEvent(component.OnRewindStart);
    UnregisterRewindStopEvent(component.OnRewindStop);
    UnregisterRewindStepEvent(component.OnRewindRollback);
    m_rewindSaveEvent.RemoveListener(component.OnRewindSave);
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

  public void RegisterRewindStepEvent(UnityAction<RollbackEventData> callback) {
    m_rewindRollbackEvent.AddListener(callback);
  }
  
  public void UnregisterRewindStepEvent(UnityAction<RollbackEventData> callback) {
    m_rewindRollbackEvent.RemoveListener(callback);
  }
  
#endregion
}

// Per-level singleton