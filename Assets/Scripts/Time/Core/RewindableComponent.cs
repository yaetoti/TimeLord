using UnityEngine;

public abstract class RewindableComponent : MonoBehaviour, IRewindable {
  protected virtual void Start() {
    TimeManager.Instance.Register(this);
  }

  protected virtual void OnDestroy() {
    TimeManager.Instance?.Unregister(this);
  }

  public virtual void OnRewindStart() {
  }

  public virtual void OnRewindStop() {
  }

  public virtual void OnRewindSave() {
  }

  public virtual void OnRewindRollback(TimeManager.RollbackEventData data) {
  }

  public virtual void OnRewindStateChanged(TimeManager.StateChangedEventData data) {
  }
}