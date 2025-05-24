using System;
using UnityEngine;

public class RewindableTest : RewindableComponent {
  public override void OnRewindRollback(TimeManager.RollbackEventData data) {
    base.OnRewindRollback(data);
    Debug.Log("Step");
  }
}