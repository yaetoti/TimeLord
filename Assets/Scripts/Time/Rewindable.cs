using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class Rewindable : MonoBehaviour {
  protected virtual void Start() {
    TimeManager.Instance.RegisterRewindStartEvent(RewindStartHandler);
    TimeManager.Instance.RegisterRewindStopEvent(RewindStopHandler);
    TimeManager.Instance.RegisterRewindStepEvent(RewindStepHandler);
  }

  protected virtual void RewindStartHandler() {
  }

  protected virtual void RewindStopHandler() {
  }

  protected virtual void RewindStepHandler() {
  }
}
