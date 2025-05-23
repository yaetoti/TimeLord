using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class RewindableObject : RewindableComponent {
  private Rigidbody2D m_rigidbody;
  private List<PointInTime> m_timePoints;
  private bool m_isRewinding;

  private float m_rewindTime;
  private Vector2 m_lastPosition;
  private Quaternion m_lastRotation;

  protected override void Start() {
    base.Start();
    m_rigidbody = GetComponent<Rigidbody2D>();
    m_timePoints = new List<PointInTime>();
    m_isRewinding = false;
  }

  protected void Update() {
    if (m_isRewinding) {
      m_rewindTime += Time.unscaledDeltaTime;
      transform.position = Vector2.Lerp(m_lastPosition, m_timePoints[0].Position, Mathf.Clamp01(m_rewindTime / 0.02f));
      transform.rotation = Quaternion.Lerp(m_lastRotation, m_timePoints[0].Rotation, Mathf.Clamp01(m_rewindTime / 0.02f));
    }
  }

  private void FixedUpdate() {
    if (!m_isRewinding) {
      if (m_timePoints.Count > TimeManager.Instance.BufferSizeMax) {
        m_timePoints.RemoveAt(m_timePoints.Count - 1);
      }
      m_timePoints.Insert(0, new PointInTime(transform.position, m_rigidbody.linearVelocity, transform.rotation));
    }
  }

  public override void OnRewindStart() {
    base.OnRewindStart();
    m_isRewinding = true;
    m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
    m_rigidbody.simulated = false;

    m_rewindTime = 0f;
    m_lastPosition = m_timePoints[0].Position;
    m_lastRotation = m_timePoints[0].Rotation;
  }

  public override void OnRewindStop() {
    base.OnRewindStop();
    m_isRewinding = false;
    m_rigidbody.linearVelocity = -m_rigidbody.linearVelocity; // TODO ???
    m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
    m_rigidbody.simulated = true;
  }

  public override void OnRewindStep(TimeManager.StepEventData data) {
    base.OnRewindStep(data);
    m_rewindTime -= 0.02f;
    transform.position = m_timePoints[0].Position;
    m_rigidbody.linearVelocity = -m_timePoints[0].Velocity;
    m_rigidbody.position = m_timePoints[0].Position;
    m_lastPosition = m_timePoints[0].Position;
    m_lastRotation = m_timePoints[0].Rotation;
    m_timePoints.RemoveAt(0);
  }
}
