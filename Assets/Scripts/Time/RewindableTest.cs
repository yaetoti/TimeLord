using System;
using UnityEngine;

public class RewindableTest : RewindableComponent {
  public override void OnRewindRollback(RewindManager.RollbackEventData data) {
    base.OnRewindRollback(data);
    Debug.Log("Step");
  }
}