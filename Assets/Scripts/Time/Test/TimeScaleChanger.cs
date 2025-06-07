using UnityEngine;

public class TimeScaleChanger : MonoBehaviour {
  public float targetTimeScale = 0.0f;
  public float duration = 20.0f;
  
  private void Start() {
    Debug.Log("Changing");
    TimeManager.Instance.ChangeTimeScale(targetTimeScale, duration);
  }
}