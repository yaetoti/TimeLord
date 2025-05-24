public interface IRewindable {
  void OnRewindStart();
  void OnRewindStop();
  void OnRewindSave();
  void OnRewindRollback(TimeManager.RollbackEventData data);
  void OnRewindStateChanged(TimeManager.StateChangedEventData data);
}
