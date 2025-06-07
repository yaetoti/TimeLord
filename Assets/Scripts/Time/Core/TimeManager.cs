using System;
using TimeLord;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class TimeManager : MonoBehaviour {
  public enum State {
    SLOWING_DOWN,
    SPEEDING_UP,
    NORMAL
  }
  
  // Attributes
  public static TimeManager Instance { get; private set; }
  public State CurrentState { get; private set; }
  
  // State
  private float m_unscaledFixedDeltaTime = 0.02f;
  private float m_fixedDeltaTime = 0.02f;
  
  private float m_fromTimeScale = 1.0f;
  private float m_toTimeScale = 1.0f;
  
  private Timer m_transitionTimer;
  

#region Lifecycle methods

  private void Awake() {
    Initialize();
    CurrentState = State.NORMAL;
    Debug.Log(Time.timeScale);
    Debug.Log(CurrentState);
    Debug.Log(Time.fixedUnscaledDeltaTime);
    Debug.Log("");
  }

  private void OnEnable() {
    #if UNITY_EDITOR
      Initialize();
    #endif
  }

  private void OnDestroy() {
    if (Instance == this) {
      Instance = null;
    }
  }

  private void Update() {
    if (CurrentState is not State.NORMAL) {
      m_transitionTimer.Update(Time.unscaledDeltaTime);
      Time.timeScale = Mathf.Lerp(m_fromTimeScale, m_toTimeScale, m_transitionTimer.Progress);
      //Debug.Log($"Time scale: {Time.timeScale}");
      Debug.Log($"Not in normal");
      
      if (m_transitionTimer.IsFinished) {
        Time.timeScale = m_toTimeScale;
        CurrentState = State.NORMAL;
      }
    }
    
    m_fixedDeltaTime = Time.timeScale * m_unscaledFixedDeltaTime;
    Time.fixedDeltaTime = m_fixedDeltaTime;
    Debug.Log($"1");
    Debug.Log($"fixedUnscaledDeltaTime: {Time.fixedUnscaledDeltaTime}");
    Debug.Log($"Time scale: {Time.timeScale}");
    Debug.Log($"fixedDeltaTime: {Time.fixedDeltaTime}");
    //Debug.Log(Time.fixedUnscaledDeltaTime);
  }

  private void FixedUpdate() {
    Debug.Log($"2");
    Debug.Log($"fixedUnscaledDeltaTime: {Time.fixedUnscaledDeltaTime}");
    Debug.Log($"Time scale: {Time.timeScale}");
    Debug.Log($"fixedDeltaTime: {Time.fixedDeltaTime}");
  }

#endregion

#region Public methods

  public void ChangeTimeScale(float targetTimeScale, float transitionDuration) {
    if (Math.Abs(m_toTimeScale - targetTimeScale) < 0.001f) {
      Debug.Log("NOPE 1");
      return;
    }

    if (Math.Abs(Time.timeScale - targetTimeScale) < 0.001f) {
      Debug.Log("NOPE 2");
      Time.timeScale = targetTimeScale;
      CurrentState = State.NORMAL;
      return;
    }
    
    if (Time.timeScale < targetTimeScale) {
      CurrentState = State.SPEEDING_UP;
    }
    else if (Time.timeScale > targetTimeScale) {
      CurrentState = State.SLOWING_DOWN;
    }
    
    m_transitionTimer.Reset(transitionDuration);
    m_fromTimeScale = Time.timeScale;
    m_toTimeScale = targetTimeScale;
  }

#endregion
  
#region Private methods

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

#endregion
  
}