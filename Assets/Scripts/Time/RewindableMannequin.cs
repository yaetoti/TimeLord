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
  private Health.TransitionState m_nextHealthState;
  private Health.TransitionState m_healthState;

  private void Awake() {
    m_health = GetComponent<Health>();
    m_animator = GetComponent<Animator>();
  }

  protected override void Start() {
    base.Start();
    m_health.RegisterHealthStateChangedListener(OnHealthStateChanged);
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    m_health?.UnregisterHealthStateChangedListener(OnHealthStateChanged);
  }

  public override void OnRewindStart() {
    base.OnRewindStart();
    
    m_nextHealthState = m_health.CurrentState.state;
    m_healthState = m_health.CurrentState.state;
  }

  public override void OnRewindStop() {
    base.OnRewindStop();
    m_animator.updateMode = AnimatorUpdateMode.Normal;
  }

  public override void OnRewindUpdate() {
    base.OnRewindUpdate();
    
    if (!gameObject.activeSelf) {
      return;
    }
    
    if (m_snapshots.Count > TimeManager.Instance.BufferSizeMax) {
      m_snapshots.RemoveAt(m_snapshots.Count - 1);
    }

    m_snapshots.Insert(0, new PointInTimeWithHealth(transform.position, new Vector2(0, 0), transform.rotation, m_health.CurrentState.health));
  }
  
  public override void OnRewindStep(TimeManager.StepEventData data) {
    base.OnRewindStep(data);
    
    if (m_snapshots.Count == 0) {
      return;
    }
    
    // Check health
    m_nextHealthState = m_healthState;
    m_healthState = m_health.CurrentState.state;
    bool isEnemy = tag.Equals("Enemy");
    
    if (isEnemy && (m_healthState, m_nextHealthState) is (Health.TransitionState.DYING, Health.TransitionState.DEAD)) {
      m_animator.ResetTrigger(DIE);
      m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
      m_animator.Play("Resurrect");
    }
    
    if (isEnemy && (m_healthState, m_nextHealthState) is (Health.TransitionState.IDLE, Health.TransitionState.DYING)) {
      transform.GetComponent<CapsuleCollider2D>().enabled = true;
      m_animator.ResetTrigger(DIE);
      m_animator.Play("Idle");
    }
    
    m_snapshots.RemoveAt(0);
  }

  private void OnHealthStateChanged(Health.StateChangedEventData data) {
    if (data.currentState == Health.TransitionState.DYING) {
      // TODO remove
      transform.GetComponent<CapsuleCollider2D>().enabled = false;
      m_animator.SetTrigger(DIE);
      return;
    }
  }
}