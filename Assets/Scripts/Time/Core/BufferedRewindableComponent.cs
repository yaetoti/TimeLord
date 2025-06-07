using System;
using System.Collections.Generic;
using JetBrains.Annotations;

public abstract class BufferedRewindableComponent<T> : RewindableComponent where T : RewindSnapshot, new() {
  protected List<T> Snapshots { get; set; } = new List<T>();

  protected void PushSnapshot(T snapshot) {
    if (Snapshots.Count > RewindManager.Instance.BufferSizeLimit) {
      Snapshots.RemoveAt(Snapshots.Count - 1);
    }
    
    Snapshots.Insert(0, snapshot);
  }

  [CanBeNull]
  protected T PeekSnapshot() {
    return Snapshots.Count == 0 ? null : Snapshots[0];
  }
  
  [CanBeNull]
  protected T PullSnapshot() {
    if (Snapshots.Count == 0) {
      return null;
    }
    
    var snapshot = Snapshots[0];
    Snapshots.RemoveAt(0);
    return snapshot;
  }

  protected void RemoveSnapshot() {
    Snapshots.RemoveAt(0);
  }

  protected abstract void HandleRewindSave();
  protected abstract void HandleRewindRollback(T snapshot);

  public override void OnRewindSave() {
    base.OnRewindSave();

    if (!gameObject.activeSelf) {
      return;
    }
    
    HandleRewindSave();
  }

  public override void OnRewindRollback(RewindManager.RollbackEventData data) {
    base.OnRewindRollback(data);
    
    var snapshot = PeekSnapshot();
    if (snapshot == null) {
      return;
    }
    
    if (RewindManager.Instance.FrameNumber > snapshot.frameNumber) {
      return;
    }
    
    HandleRewindRollback(snapshot);
    RemoveSnapshot();
  }
}