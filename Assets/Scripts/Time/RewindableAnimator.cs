using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RewindableAnimator : BufferedRewindableComponent<RewindableAnimator.TimeState> {
  [SerializeField]
  private bool m_verbose = false;
  
  public sealed class TimeState : RewindSnapshot {
    public int statePathHash;
    public float normalizedTime;
  }

  private Animator m_animator;

  private void Awake() {
    m_animator = GetComponent<Animator>();
  }

  // private void Update() {
  //   if (RewindManager.Instance.State is RewindManager.RewindState.REWINDING) {
  //     
  //   }
  // }

  protected override void HandleRewindSave() {
    var animatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
    
    var state = new TimeState();
    state.frameNumber = RewindManager.Instance.FrameNumber;
    state.statePathHash = animatorStateInfo.fullPathHash;
    state.normalizedTime = animatorStateInfo.normalizedTime;
    
    PushSnapshot(state);
  }

  protected override void HandleRewindRollback(TimeState snapshot) {
    m_animator.Play(snapshot.statePathHash, 0, snapshot.normalizedTime);
  }
}