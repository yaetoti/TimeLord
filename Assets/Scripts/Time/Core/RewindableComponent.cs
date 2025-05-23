using UnityEngine;

public class RewindableComponent : MonoBehaviour, IRewindableComponent {
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

  public virtual void OnRewindStep(TimeManager.StepEventData data) {
  }

  public virtual void OnRewindStateChanged(TimeManager.StateChangedEventData data) {
  }
}