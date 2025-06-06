public interface IRewindable {
  void OnRewindStart();
  void OnRewindStop();
  void OnRewindSave();
  void OnRewindRollback(RewindManager.RollbackEventData data);
  void OnRewindStateChanged(RewindManager.StateChangedEventData data);
}
