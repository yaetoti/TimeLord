using UnityEngine;

[DefaultExecutionOrder(1)]
public class RewindableMannequin : MonoBehaviour {
  private static readonly int DIE = Animator.StringToHash("Die");

  // Components
  private Health m_health;
  private Animator m_animator;

  private void Awake() {
    m_animator = GetComponent<Animator>();
    m_health = GetComponent<Health>();
  }
  
  private void Start() {
    m_health.RegisterHealthStateChangedListener(OnHealthStateChanged);
  }

  private void OnDestroy() {
    m_health?.UnregisterHealthStateChangedListener(OnHealthStateChanged);
  }

  private void OnHealthStateChanged(Health.StateChangedEventData data) {
    if (data.previousState == Health.TransitionState.IDLE && data.currentState == Health.TransitionState.DYING) {
      m_animator.SetTrigger(DIE);
    }
  }
}