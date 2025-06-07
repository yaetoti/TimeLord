using System;
using System.Collections.Generic;
using UnityEngine;

public class RewindableTransform : RewindableComponent {
  [SerializeField]
  private bool m_savePosition = true;
  [SerializeField]
  private bool m_saveScale = true;
  [SerializeField]
  private bool m_saveRotation = true;
  
  public sealed class TimeState {
    public ulong frameNumber;
    public Vector3 position;
    public Vector3 localScale;
    public Quaternion rotation;
  }
  
  private List<TimeState> m_snapshots;
  
  private void Awake() {
    m_snapshots = new List<TimeState>();
  }

  private void Update() {
    if (RewindManager.Instance.State is RewindManager.RewindState.REWINDING) {
      
    }
  }

  public override void OnRewindStart() {
    base.OnRewindStart();
  }

  public override void OnRewindStop() {
    base.OnRewindStop();
  }

  public override void OnRewindSave() {
    base.OnRewindSave();
    
    var state = new TimeState();
    state.frameNumber = RewindManager.Instance.FrameNumber;
    if (m_savePosition) {
      state.position = transform.position;
    }
    
    if (m_saveRotation) {
      state.rotation = transform.rotation;
    }
    
    if (m_saveScale) {
      state.localScale = transform.localScale;
    }
    
    m_snapshots.Insert(0, state);
  }

  public override void OnRewindRollback(RewindManager.RollbackEventData data) {
    base.OnRewindRollback(data);

    if (m_snapshots.Count == 0) {
      return;
    }
    
    var state = m_snapshots[0];
    if (state.frameNumber < RewindManager.Instance.FrameNumber) {
      return;
    }
    
    if (m_savePosition) {
      transform.position = state.position;
    }
    
    if (m_saveRotation) {
      transform.rotation = state.rotation;
    }
    
    if (m_saveScale) {
      transform.localScale = state.localScale;
    }
    
    m_snapshots.RemoveAt(0);
  }

  public override void OnRewindStateChanged(RewindManager.StateChangedEventData data) {
    base.OnRewindStateChanged(data);
  }
}