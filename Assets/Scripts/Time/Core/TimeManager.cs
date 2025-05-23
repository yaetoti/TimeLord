using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

// Per-level singleton
public class TimeManager : MonoBehaviour {
  public enum State {
    WAITING,
    SLOWING_DOWN,
    REWINDING,
    SPEEDING_UP
  }
  
  public sealed class StateChangedEventData {
    public State PreviousState { get; private set; }
    public State CurrentState { get; private set; }

    public StateChangedEventData(State previousState, State currentState) {
      PreviousState = previousState;
      CurrentState = currentState;
    }
  }

  public sealed class StepEventData {
    public StepEventData() {
    }
  }

  public static TimeManager Instance { get; private set; }

  public State CurrentState => m_state;
  public ulong FrameNumber => m_frameNumber;
  public int BufferSizeMax => m_bufferSizeMax;

  [SerializeField] [Min(0)] private int m_bufferSizeMax = 500;
  [SerializeField] [Min(0)] private float m_timeSlowingLength = 0.5f;
  [SerializeField] [Min(0)] private int m_manaMax = 2000;

  // Events
  private UnityEvent m_rewindUpdateEvent;
  private UnityEvent m_rewindStartEvent; // TODO remove
  private UnityEvent m_rewindStopEvent; // TODO remove
  private UnityEvent<StepEventData> m_rewindStepEvent;
  private UnityEvent<StateChangedEventData> m_stateChangedEvent;
  
  // State
  private State m_state = State.WAITING;
  private float m_timeSlowingProgress;
  private float m_rewindTime;
  private int m_bufferTicks;
  private int m_mana;

  private bool m_zeroUpdateFired;
  private ulong m_frameNumber;
  private float m_frameTimer;

  private void Awake() {
    Initialize();
    m_mana = m_manaMax;
    
    // Events
    m_rewindUpdateEvent = new UnityEvent();
    m_stateChangedEvent = new UnityEvent<StateChangedEventData>();
    m_rewindStartEvent = new UnityEvent();
    m_rewindStopEvent = new UnityEvent();
    m_rewindStepEvent = new UnityEvent<StepEventData>();
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
    if (m_state != State.WAITING) {
      Time.timeScale = 1;
      m_rewindStopEvent.Invoke();
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.WAITING));
      m_state = State.WAITING;
    }
  }

  private void Update() {
    if (!m_zeroUpdateFired) {
      m_zeroUpdateFired = true;
      m_rewindUpdateEvent.Invoke();
    }

    // Invoke updates
    if (m_state is not State.REWINDING) {
      m_frameTimer += Time.unscaledDeltaTime;
      // Next update tick
      while (m_frameTimer >= 0.02f) {
        m_frameTimer -= 0.02f;
        ++m_frameNumber;
        
        if (m_bufferTicks < m_bufferSizeMax) {
          ++m_bufferTicks;
        }
        
        m_rewindUpdateEvent.Invoke();
      }
    }

    
    // Processing
    switch (m_state) {
    case State.SLOWING_DOWN:
      m_timeSlowingProgress -= Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingProgress / m_timeSlowingLength);
      Time.fixedDeltaTime = 0.02f * Time.timeScale;
      if (m_timeSlowingProgress <= 0) {
        m_rewindStartEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.REWINDING));
        m_state = State.REWINDING;
      }

      break;
    case State.SPEEDING_UP:
      m_timeSlowingProgress += Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingProgress / m_timeSlowingLength);
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      if (m_timeSlowingProgress >= m_timeSlowingLength) {
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.WAITING));
        m_state = State.WAITING;
      }

      break;
    case State.REWINDING:
      // Invoke rollbacks
      m_frameTimer -= Time.unscaledDeltaTime;
      while (m_frameTimer < 0.0f) {
        m_frameTimer += 0.02f;
        --m_frameNumber;
        
        --m_bufferTicks;
        --m_mana;
        
        m_rewindStepEvent.Invoke(new StepEventData());
      }
      
      m_rewindTime += Time.unscaledDeltaTime;
      if (m_rewindTime > 0.02f) {
        // TODO: fixedStep
        m_rewindTime -= 0.02f;
        // --m_bufferTicks;
        // --m_mana;

        
        //m_rewindStepEvent.Invoke(new StepEventData());
      }

      if (m_bufferTicks <= 0 || m_mana <= 0) {
        m_rewindStopEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.SPEEDING_UP));
        m_state = State.SPEEDING_UP;
      }

      break;
    case State.WAITING:
      break;
    }

    // Input
    HandleInput();
  }

  private void HandleInput() {
    if (InputHolder.Instance.MasterInput.Player.TimeRewind.WasPressedThisFrame()) {
      switch (m_state) {
      case State.WAITING:
        if (m_mana > 0) {
          m_timeSlowingProgress = m_timeSlowingLength;
          m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.SLOWING_DOWN));
          m_state = State.SLOWING_DOWN;
        }

        break;
      case State.SLOWING_DOWN:
        m_state = State.SPEEDING_UP;
        break;
      case State.REWINDING:
        m_rewindStopEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.SPEEDING_UP));
        m_state = State.SPEEDING_UP;
        break;
      case State.SPEEDING_UP:
        break;
      }
    }
  }

  public void Register(IRewindableComponent component) {
    RegisterStateChangedEvent(component.OnRewindStateChanged);
    RegisterRewindStartEvent(component.OnRewindStart);
    RegisterRewindStopEvent(component.OnRewindStop);
    RegisterRewindStepEvent(component.OnRewindStep);
    m_rewindUpdateEvent.AddListener(component.OnRewindUpdate);
  }

  public void Unregister(IRewindableComponent component) {
    UnregisterStateChangedEvent(component.OnRewindStateChanged);
    UnregisterRewindStartEvent(component.OnRewindStart);
    UnregisterRewindStopEvent(component.OnRewindStop);
    UnregisterRewindStepEvent(component.OnRewindStep);
    m_rewindUpdateEvent.RemoveListener(component.OnRewindUpdate);
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

  public void RegisterRewindStepEvent(UnityAction<StepEventData> callback) {
    m_rewindStepEvent.AddListener(callback);
  }
  
  public void UnregisterRewindStepEvent(UnityAction<StepEventData> callback) {
    m_rewindStepEvent.RemoveListener(callback);
  }
}