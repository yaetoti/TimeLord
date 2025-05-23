using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRewindableComponent {
  void OnRewindStart();
  void OnRewindStop();
  void OnRewindStep(TimeManager.StepEventData data);
  void OnRewindStateChanged(TimeManager.StateChangedEventData data);
  void OnRewindUpdate();
}
