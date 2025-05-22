using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class RewindableManequin : Rewindable {
  private Health m_health;
  private List<PointInTimeWithHealth> m_timePoints;
  private bool m_isRewinding;

  private float m_rewindTime;
  private Vector2 m_lastPosition;
  private Quaternion m_lastRotation;

  private void Awake() {
    m_health = GetComponent<Health>();
    m_timePoints = new List<PointInTimeWithHealth>();
    m_isRewinding = false;
  }

  protected override void Start() {
    base.Start();
  }

  protected void Update() {
    if (m_isRewinding) { // crutch - m_timePoints.Count > 0.
      m_rewindTime += Time.unscaledDeltaTime;
      transform.position = Vector2.Lerp(m_lastPosition, m_timePoints[0].Position, Mathf.Clamp01(m_rewindTime / 0.02f));
      transform.rotation = Quaternion.Lerp(m_lastRotation, m_timePoints[0].Rotation, Mathf.Clamp01(m_rewindTime / 0.02f));
    }
  }



  private void FixedUpdate() {
    if (!m_isRewinding) {
      if (m_timePoints.Count > TimeManager.Instance.BufferTicks) {
        m_timePoints.RemoveAt(m_timePoints.Count - 1);
      }
      m_timePoints.Insert(0, new PointInTimeWithHealth(transform.position, new Vector2(0, 0), transform.rotation, m_health.CurrentHealth));
    }
  }

  protected override void RewindStartHandler() {
    base.RewindStartHandler();
    m_isRewinding = true;

    m_rewindTime = 0f;
    m_lastPosition = m_timePoints[0].Position;
    m_lastRotation = m_timePoints[0].Rotation;
  }

  protected override void RewindStopHandler() {
    base.RewindStopHandler();
    m_isRewinding = false;
  }

  protected override void RewindStepHandler() {
    base.RewindStepHandler();
    m_rewindTime -= 0.02f;
    transform.position = m_timePoints[0].Position;
    m_lastPosition = m_timePoints[0].Position;
    m_lastRotation = m_timePoints[0].Rotation;
    //Debug.Log("DO: " + m_health.CurrentHealth);
    if (tag.Equals("Entity") && m_health.CurrentHealth <= 0 && m_timePoints[0].Health > 0) {
      Debug.Log(m_health.CurrentHealth);
      GetComponent<Animator>().SetTrigger("Res");
    }
    m_health.ForceSet(m_timePoints[0].Health);
    m_timePoints.RemoveAt(0);
  }
}
