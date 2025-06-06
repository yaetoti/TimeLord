using UnityEngine;

public abstract class RewindableComponent : MonoBehaviour, IRewindable {
  protected virtual void Start() {
    RewindManager.Instance.Register(this);
  }

  protected virtual void OnDestroy() {
    RewindManager.Instance?.Unregister(this);
  }

  public virtual void OnRewindStart() {
  }

  public virtual void OnRewindStop() {
  }

  public virtual void OnRewindSave() {
  }

  public virtual void OnRewindRollback(RewindManager.RollbackEventData data) {
  }

  public virtual void OnRewindStateChanged(RewindManager.StateChangedEventData data) {
  }
}