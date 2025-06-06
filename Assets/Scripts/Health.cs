using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Health : RewindableComponent {
  [Serializable]
  public enum TransitionState {
    IDLE,
    DYING,
    DEAD,
    RESURRECTING
  }
  
  [Serializable]
  public sealed class State {
    public int maxHealth = 1;
    public int health = 1;
    public float transitionTime = 1.0f;
    public float transitionTimeCurrent = 0.0f;
    public TransitionState state = TransitionState.IDLE;

    public State() {
    }

    public State(int maxHealth, int health, float transitionTime, float transitionTimeCurrent, TransitionState state) {
      this.maxHealth = maxHealth;
      this.health = health;
      this.transitionTime = transitionTime;
      this.transitionTimeCurrent = transitionTimeCurrent;
      this.state = state;
    }

    public override string ToString() {
      return $"State[maxHealth={maxHealth}, health={health}, transitionTime={transitionTime}, transitionTimeCurrent={transitionTimeCurrent}, state={state}]";
    }
    
    public State Clone() {
      return new State(maxHealth, health, transitionTime, transitionTimeCurrent, state);
    }
  }
  
  [SerializeField]
  private bool m_disableOnDeath = true;
  
  public sealed class TimeState {
    public ulong frameNumber;
    public State state;

    public TimeState(ulong frameNumber, State state) {
      this.frameNumber = frameNumber;
      this.state = state;
    }
  }

  // TODO interpolator
  public sealed class StateInterpolator {
    public State Interpolate(State previousState, State currentState, float progress) {
      return new State();
    }
  }
  
  public sealed class StateChangedEventData {
    public TransitionState previousState;
    public TransitionState currentState;
    
    public StateChangedEventData(TransitionState previousState, TransitionState currentState) {
      this.previousState = previousState;
      this.currentState = currentState;
    }
  }
  
  private static readonly int DIE = Animator.StringToHash("Die");
  public State CurrentState => m_state;

  // Animation
  [SerializeField] private ParticleSystem m_damageFX;

  // Config
  [SerializeField] private State m_state;

  // State
  private UnityEvent<StateChangedEventData> m_healthStateChangedEvent;
  private List<TimeState> m_snapshots;

  private void Awake() {
    m_healthStateChangedEvent = new UnityEvent<StateChangedEventData>();
    m_snapshots = new List<TimeState>();
  }

  protected override void Start() {
    base.Start();
    // TODO ???
    if (tag.Equals("Player")) {
      m_state.maxHealth = DataStash.Instance.m_maxHealth;
      m_state.health = DataStash.Instance.m_health;
      if (m_state.health <= 0) {
        StartDying();
      }

      return;
    }

    m_state.health = m_state.maxHealth;
    if (m_state.health <= 0) {
      StartDying();
    }
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    
    // Save player health
    // TODO factor out
    if (tag.Equals("Player")) {
      DataStash.Instance.m_maxHealth = m_state.maxHealth;
      DataStash.Instance.m_health = m_state.health;
      return;
    }
  }

  private void Update() {
    // No updates when rewinding
    if (RewindManager.Instance.State is RewindManager.RewindState.REWINDING) {
      return;
    }
    
    if (m_state.state is TransitionState.RESURRECTING or TransitionState.DYING) {
      m_state.transitionTimeCurrent += Time.deltaTime;
      if (m_state.transitionTimeCurrent >= m_state.transitionTime) {
        if (m_state.state == TransitionState.RESURRECTING) {
          SwitchTransitionState(TransitionState.IDLE);
        }

        if (m_state.state == TransitionState.DYING) {
          SwitchTransitionState(TransitionState.DEAD);
        }
      }
    }
  }

  public void ReduceHealth(int amount) {
    if (amount <= 0 || m_state.health <= 0) {
      return;
    }

    Debug.Log($"REDUCE HEALTH {amount}, {m_state}");
    m_state.health -= Math.Min(amount, m_state.health);
    
    // Damage effect
    if (m_state.health >= 0) {
      StartCoroutine(DamageEffectCoroutine());
    }

    // Start dying
    if (m_state.health <= 0) {
      StartDying();
    }
  }

  public void SetHealth(int health) {
    if (health == m_state.health) {
      return;
    }
    
    m_state.health = health < 0 ? 0 : health;
    
    // Start dying
    if (m_state.health <= 0) {
      StartDying();
      return;
    }

    // Health > 0
    if (m_state.state is TransitionState.DYING or TransitionState.DEAD) {
      SwitchTransitionState(TransitionState.RESURRECTING);
    }
  }

  private void StartDying() {
    if (m_state.state is TransitionState.DEAD or TransitionState.DYING) {
      return;
    }
    
    SwitchTransitionState(TransitionState.DYING);
    
    // Notify director
    // TODO move
    LevelDirector.Instance.IAmDead(transform.tag);
    if (tag.Equals("Player")) {
      GetComponent<MovementController>().Die();
      return;
    }
  }

  private void SwitchTransitionState(TransitionState state) {
    // TODO hotfix, remove
    if (m_state.state != TransitionState.DEAD && state == TransitionState.DEAD) {
      Debug.Log("Health destroyed at " + RewindManager.Instance.FrameNumber);
      
      if (m_disableOnDeath) {
        gameObject.SetActive(false);
      }
    }
    
    TransitionState previousState = m_state.state;
    m_state.state = state;
    m_state.transitionTimeCurrent = 0.0f;
    m_healthStateChangedEvent.Invoke(new StateChangedEventData(previousState, state));
    
    // TODO factor out
    if (m_state.transitionTimeCurrent >= m_state.transitionTime) {
      if (m_state.state == TransitionState.RESURRECTING) {
        SwitchTransitionState(TransitionState.IDLE);
      }

      if (m_state.state == TransitionState.DYING) {
        SwitchTransitionState(TransitionState.DEAD);
      }
    }
  }
  
  // Coroutines

  private IEnumerator DamageEffectCoroutine() {
    ParticleSystem system = Instantiate(m_damageFX, transform.position, Quaternion.identity);
    system.Play();
    yield return new WaitForSeconds(1);
    Destroy(system);
  }
  
  public override void OnRewindStateChanged(RewindManager.StateChangedEventData data) {
    base.OnRewindStateChanged(data);

    if (data.CurrentState is RewindManager.RewindState.SLOWING_DOWN) {
      // TODO ???
      //m_state = m_snapshots[0];
      return;
    }
    
    if (data.CurrentState is RewindManager.RewindState.WAITING) {
    }
  }

  public override void OnRewindSave() {
    base.OnRewindSave();

    if (!gameObject.activeSelf) {
      return;
    }
    
    // Add snapshots
    if (m_snapshots.Count > RewindManager.Instance.BufferSizeLimit) {
      m_snapshots.RemoveAt(m_snapshots.Count - 1);
    }

    
    m_snapshots.Insert(0, new TimeState(RewindManager.Instance.FrameNumber, m_state.Clone()));
  }

  public override void OnRewindRollback(RewindManager.RollbackEventData data) {
    base.OnRewindRollback(data);

    if (m_snapshots.Count == 0) {
      return;
    }

    if (RewindManager.Instance.FrameNumber > m_snapshots[0].frameNumber) {
      //Debug.Log($"HEALTH LESS. {m_snapshots[0].frameNumber} < {RewindManager.Instance.FrameNumber}");
      return;
    }

    // TODO hotfix, remove
    if (m_state.state == TransitionState.DEAD && m_snapshots[0].state.state != TransitionState.DEAD) {
      //Debug.Log("Health resurrected at: " + RewindManager.Instance.FrameNumber);
      if (m_disableOnDeath) {
        gameObject.SetActive(true);
      }
    }
    
    m_state = m_snapshots[0].state;
    m_snapshots.RemoveAt(0);
  }
  
  public void RegisterHealthStateChangedListener(UnityAction<StateChangedEventData> listener) {
    m_healthStateChangedEvent.AddListener(listener);
  }
  
  public void UnregisterHealthStateChangedListener(UnityAction<StateChangedEventData> listener) {
    m_healthStateChangedEvent.RemoveListener(listener);
  }
}