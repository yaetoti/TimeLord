using System;
using UnityEngine;

public class RewindableTest : RewindableComponent {
  public override void OnRewindStep(TimeManager.StepEventData data) {
    base.OnRewindStep(data);
    Debug.Log("Step");
  }
}