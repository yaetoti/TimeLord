using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour {
  public enum State {
    WAITING,
    SLOWING_TIME,
    REWINDING,
    SPEEDING_UP_TIME
  }
  
  public class StateChangedEventData {
    public State PreviousState { get; private set; }
    public State CurrentState { get; private set; }

    public StateChangedEventData(State previousState, State currentState) {
      PreviousState = previousState;
      CurrentState = currentState;
    }
  }

  public static TimeManager Instance => m_instance;

  private static TimeManager m_instance;

  public int BufferTicks => m_bufferTicks;
  public float TimeSlowingLength => m_timeSlowingLength;


  [SerializeField] [Min(0)] private int m_bufferTicks = 500;
  [SerializeField] [Min(0)] private float m_timeSlowingLength = 2;
  [SerializeField] [Min(0)] private int m_mana = 750;
  [SerializeField] private ScriptableRendererFeature m_rendererData;

  // Events
  private UnityEvent m_rewindStartEvent;
  private UnityEvent m_rewindStopEvent;
  private UnityEvent m_rewindStepEvent;
  private UnityEvent<StateChangedEventData> m_stateChangedEvent;
  
  private State m_state;
  private float m_timeSlowingCurrent;
  private float m_rewindTime;
  private int m_bufferTicksCurrent;
  private int m_manaCurrent;

  private void Awake() {
    Initialize();
    m_manaCurrent = m_mana;
    m_state = State.WAITING;
    
    // Events
    m_stateChangedEvent = new UnityEvent<StateChangedEventData>();
    m_rewindStartEvent = new UnityEvent();
    m_rewindStopEvent = new UnityEvent();
    m_rewindStepEvent = new UnityEvent();
  }

  private void OnEnable() {
#if UNITY_EDITOR
    if (m_instance != this) {
      Initialize();
    }
#endif
  }

  private void Initialize() {
    if (m_instance is not null) {
      Destroy(this);
      return;
    }

    m_instance = this;
  }

  public void ForceResetRewind() {
    if (m_state != State.WAITING) {
      Time.timeScale = 1;
      //m_rendererData.SetActive(false);
      m_rewindStopEvent.Invoke();
      m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.WAITING));
      m_state = State.WAITING;
    }
  }

  private void OnDestroy() {
    if (m_instance != this) {
      return;
    }
    
    m_instance = null;
    ForceResetRewind();
  }

  void Update() {
    // Processing
    switch (m_state) {
    case State.SLOWING_TIME:
      m_timeSlowingCurrent -= Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingCurrent / m_timeSlowingLength);
      Time.fixedDeltaTime = 0.02f * Time.timeScale;
      if (m_timeSlowingCurrent <= 0) {
        m_rewindStartEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.REWINDING));
        m_state = State.REWINDING;
      }

      break;
    case State.SPEEDING_UP_TIME:
      m_timeSlowingCurrent += Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingCurrent / m_timeSlowingLength);
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      if (m_timeSlowingCurrent >= m_timeSlowingLength) {
        //m_rendererData.SetActive(false);
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.WAITING));
        m_state = State.WAITING;
      }

      break;
    case State.REWINDING:
      m_rewindTime += Time.unscaledDeltaTime;
      if (m_rewindTime > 0.02f) {
        // TODO: fixedStep
        m_rewindTime -= 0.02f;
        --m_bufferTicksCurrent;
        --m_manaCurrent;
        m_rewindStepEvent.Invoke();
      }

      if (m_bufferTicksCurrent <= 0 || m_manaCurrent <= 0) {
        m_rewindStopEvent.Invoke();
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.SPEEDING_UP_TIME));
        m_state = State.SPEEDING_UP_TIME;
      }

      break;
    case State.WAITING:
      break;
    }

    // Input
    if (InputHolder.Instance.MasterInput.Player.TimeRewind.WasPressedThisFrame()) {
      switch (m_state) {
      case State.WAITING:
        if (m_manaCurrent > 0) {
          //m_rendererData.SetActive(true);
          m_timeSlowingCurrent = m_timeSlowingLength;
          m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.SLOWING_TIME));
          m_state = State.SLOWING_TIME;
        }

        break;
      case State.SLOWING_TIME:
        m_state = State.SPEEDING_UP_TIME;
        break;
      case State.REWINDING:
        m_rewindStopEvent.Invoke(); // Invoke callbacks before changing state to make it possible to get previous state from methods
        m_stateChangedEvent.Invoke(new StateChangedEventData(m_state, State.SPEEDING_UP_TIME));
        m_state = State.SPEEDING_UP_TIME;
        break;
      case State.SPEEDING_UP_TIME:
        break;
      }
    }
  }

  private void FixedUpdate() {
    if (m_state != State.REWINDING && m_bufferTicksCurrent < m_bufferTicks) {
      ++m_bufferTicksCurrent;
    }
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

  public void RegisterRewindStepEvent(UnityAction callback) {
    m_rewindStepEvent.AddListener(callback);
  }
  
  public void UnregisterRewindStepEvent(UnityAction callback) {
    m_rewindStepEvent.RemoveListener(callback);
  }
}