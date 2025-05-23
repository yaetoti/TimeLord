using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class RewindableMannequin : RewindableComponent {
  private static readonly int DIE = Animator.StringToHash("Die");

  // Components
  private Health m_health;
  private Animator m_animator;
  
  // State
  private List<PointInTimeWithHealth> m_snapshots = new List<PointInTimeWithHealth>();
  private bool m_isRewinding;
  private float m_rewindTime;
  
  private Health.TransitionState m_nextHealthState;
  private Health.TransitionState m_healthState;

  // Rewindable
  private Vector2 m_lastPosition;
  private Quaternion m_lastRotation;

  private void Awake() {
    m_health = GetComponent<Health>();
    m_animator = GetComponent<Animator>();
    
    m_isRewinding = false;
  }

  protected override void Start() {
    base.Start();
    m_health.RegisterHealthStateChangedListener(OnHealthStateChanged);
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    m_health?.UnregisterHealthStateChangedListener(OnHealthStateChanged);
  }

  protected void Update() {
    if (TimeManager.Instance.CurrentState is not TimeManager.State.WAITING) {
      if (m_snapshots.Count == 0) {
        return;
      }
      
      m_rewindTime += Time.unscaledDeltaTime;
      transform.position = Vector2.Lerp(m_lastPosition, m_snapshots[0].Position, Mathf.Clamp01(m_rewindTime / 0.02f));
      transform.rotation = Quaternion.Lerp(m_lastRotation, m_snapshots[0].Rotation, Mathf.Clamp01(m_rewindTime / 0.02f));
    }
  }


  private void FixedUpdate() {
    if (TimeManager.Instance.CurrentState is TimeManager.State.WAITING) {
      if (m_snapshots.Count > TimeManager.Instance.BufferSizeMax) {
        m_snapshots.RemoveAt(m_snapshots.Count - 1);
      }

      m_snapshots.Insert(0, new PointInTimeWithHealth(transform.position, new Vector2(0, 0), transform.rotation, m_health.CurrentState.health));
    }
  }

  public override void OnRewindStart() {
    base.OnRewindStart();
    m_isRewinding = true;

    m_rewindTime = 0f;
    m_lastPosition = m_snapshots[0].Position;
    m_lastRotation = m_snapshots[0].Rotation;
    
    m_nextHealthState = m_health.CurrentState.state;
    m_healthState = m_health.CurrentState.state;
  }

  public override void OnRewindStop() {
    base.OnRewindStop();
    m_isRewinding = false;
    m_animator.updateMode = AnimatorUpdateMode.Normal;
  }

  public override void OnRewindStep(TimeManager.StepEventData data) {
    base.OnRewindStep(data);
    
    if (m_snapshots.Count == 0) {
      return;
    }
    
    m_rewindTime = 0.0f;
    transform.position = m_snapshots[0].Position;
    m_lastPosition = m_snapshots[0].Position;
    m_lastRotation = m_snapshots[0].Rotation;
    
    // Check health
    m_nextHealthState = m_healthState;
    m_healthState = m_health.CurrentState.state;
    if (m_nextHealthState != m_healthState) {
      //Debug.Log($"Current: {m_healthState}, Next: {m_nextHealthState}");
    }
    
    if (tag.Equals("Enemy") && m_healthState is Health.TransitionState.DYING && m_nextHealthState is Health.TransitionState.DEAD) {
      var stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
      Debug.Log("WAKE UP");
      m_animator.Play("Resurrect");
      // if (stateInfo.IsName("Dying")) {
      //   Debug.Log("Dying -> Resurrect");
      //   m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
      //   m_animator.ResetTrigger(DIE);
      //   m_animator.Play("Resurrect", 0, 1 - stateInfo.normalizedTime);
      // }
      // else {
      //   Debug.Log("Other State -> Resurrect");
      //   m_animator.ResetTrigger(DIE);
      //   m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
      //   m_animator.Play("Resurrect");
      // }
    }
    
    if (tag.Equals("Enemy") && m_healthState is Health.TransitionState.IDLE && m_nextHealthState is Health.TransitionState.DYING) {
      Debug.Log("AND STAND STILL");
      m_animator.ResetTrigger(DIE);
      m_animator.Play("Idle");
    }

    //Debug.Log($"Current = {m_health.CurrentState.health}, Previous = {m_snapshots[0].Health}");
    
    // if (tag.Equals("Enemy") && m_health.CurrentState.health <= 0 && m_snapshots[0].Health > 0) {
    //   var stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
    //   if (stateInfo.IsName("Dying")) {
    //     Debug.Log("Dying -> Resurrect");
    //     m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    //     m_animator.ResetTrigger(DIE);
    //     m_animator.Play("Resurrect", 0, 1 - stateInfo.normalizedTime);
    //   }
    //   else {
    //     Debug.Log("Other State -> Resurrect");
    //     m_animator.ResetTrigger(DIE);
    //     m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    //     m_animator.Play("Resurrect");
    //   }
    // }

    //m_health.SetHealth(m_snapshots[0].Health);
    m_snapshots.RemoveAt(0);
  }

  private void OnHealthStateChanged(Health.StateChangedEventData data) {
    if (data.currentState == Health.TransitionState.DYING) {
      Debug.Log("IM DYING BOSS");
      // TODO remove
      //transform.GetComponent<CapsuleCollider2D>().enabled = false;
      m_animator.SetTrigger(DIE);
      return;
    }
  }
}