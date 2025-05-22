using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour {
  public static TimeManager Instance {
    get {
      if (m_instance == null) {
        throw new NullReferenceException("TimeManager is null!");
      }
      return m_instance;
    }
  }

  public int BufferTicks {
    get {
      return m_bufferTicks;
    }
  }

  public float TimeSlowingLength {
    get {
      return m_timeSlowingLength;
    }
  }

  private static TimeManager m_instance;

  [SerializeField]
  [Min(0)]
  private int m_bufferTicks = 500;
  [SerializeField]
  [Min(0)]
  private float m_timeSlowingLength = 2;
  [SerializeField]
  [Min(0)]
  private int m_mana = 750;
  [SerializeField]
  private ScriptableRendererFeature m_rendererData;

  private UnityEvent m_rewindStartEvent;
  private UnityEvent m_rewindStopEvent;
  private UnityEvent m_rewindStepEvent;
  private VolumeController m_volumeController; // TODO nahui from this class
  private ControllerState state;
  private float m_timeSlowingCurrent;
  private float m_rewindTime;
  private int m_bufferTicksCurrent;
  private int m_manaCurrent;

  private void Awake() {
    Initialize();
    m_manaCurrent = m_mana;
    state = ControllerState.WAITING;
    m_rewindStartEvent = new UnityEvent();
    m_rewindStopEvent = new UnityEvent();
    m_rewindStepEvent = new UnityEvent();
    m_volumeController = FindObjectOfType<VolumeController>(); // TODO nahui from this class
  }

  private void OnEnable() {
#if UNITY_EDITOR
    if (m_instance != this) {
      Initialize();
    }
#endif
  }

  private void Initialize() {
    if (m_instance != null) {
      Destroy(this);
    }

    m_instance = this;
  }

  public void ForceResetRewind() {
    if (state != ControllerState.WAITING) {
      Time.timeScale = 1;
      m_volumeController.SwitchActive();
      //m_rendererData.SetActive(false);
      m_rewindStopEvent.Invoke();
      state = ControllerState.WAITING;
    }
  }

  private void OnDestroy() {
    m_instance = null;
    ForceResetRewind();
  }

  void Update() {
    // Processing
    switch (state) {
    case ControllerState.SLOWING_TIME:
      m_timeSlowingCurrent -= Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingCurrent / m_timeSlowingLength);
      Time.fixedDeltaTime = 0.02f * Time.timeScale;
      if (m_timeSlowingCurrent <= 0) {
        m_rewindStartEvent.Invoke();
        state = ControllerState.REWINDING;
      }
      break;
    case ControllerState.SPEEDING_UP_TIME:
      m_timeSlowingCurrent += Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingCurrent / m_timeSlowingLength);
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      if (m_timeSlowingCurrent >= m_timeSlowingLength) {
        m_volumeController.SwitchTimeRevese(); // TODO nahui from this class
        //m_rendererData.SetActive(false);
        state = ControllerState.WAITING;
      }
      break;
    case ControllerState.REWINDING:
      m_rewindTime += Time.unscaledDeltaTime;
      if (m_rewindTime > 0.02f) { // TODO: fixedStep
        m_rewindTime -= 0.02f;
        --m_bufferTicksCurrent;
        --m_manaCurrent;
        m_rewindStepEvent.Invoke();
      }

      if (m_bufferTicksCurrent <= 0 || m_manaCurrent <= 0) {
        m_rewindStopEvent.Invoke();
        state = ControllerState.SPEEDING_UP_TIME;
      }
      break;
    case ControllerState.WAITING:
      break;
    }

    // Input
    if (InputHolder.Instance.MasterInput.Player.TimeRewind.WasPressedThisFrame()) {
      switch (state) {
      case ControllerState.WAITING:
        if (m_manaCurrent > 0) {
          m_volumeController.SwitchTimeRevese(); // TODO nahui from this class
          //m_rendererData.SetActive(true);
          state = ControllerState.SLOWING_TIME;
          m_timeSlowingCurrent = m_timeSlowingLength;
        }
        break;
      case ControllerState.SLOWING_TIME:
        state = ControllerState.SPEEDING_UP_TIME;
        break;
      case ControllerState.REWINDING:
        m_rewindStopEvent.Invoke(); // Invoke callbacks before changing state to make it possible to get previous state from methods
        state = ControllerState.SPEEDING_UP_TIME;
        break;
      case ControllerState.SPEEDING_UP_TIME:
        break;
      }
    }
  }

  private void FixedUpdate() {
    if (state != ControllerState.REWINDING && m_bufferTicksCurrent < m_bufferTicks) {
      ++m_bufferTicksCurrent;
    }
  }

  public void RegisterRewindStartEvent(UnityAction callback) {
    m_rewindStartEvent.AddListener(callback);
  }

  public void RegisterRewindStopEvent(UnityAction callback) {
    m_rewindStopEvent.AddListener(callback);
  }

  public void RegisterRewindStepEvent(UnityAction callback) {
    m_rewindStepEvent.AddListener(callback);
  }
}
