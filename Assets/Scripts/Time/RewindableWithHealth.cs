using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class RewindableWithHealth : RewindableComponent {
  // Components
  private Rigidbody2D m_rigidbody;

  // State
  private float m_rewindTime;
  private Vector2 m_lastPosition;
  private Quaternion m_lastRotation;
  private List<PointInTime> m_snapshots;

  protected override void Start() {
    base.Start();
    m_snapshots = new List<PointInTime>();
    m_rigidbody = GetComponent<Rigidbody2D>();
  }

  protected void Update() {
    if (RewindManager.Instance.State is RewindManager.RewindState.REWINDING) {
      if (m_snapshots.Count == 0) {
        return;
      }
      
      // TODO time must be smaller when speeding up or slowing down
      m_rewindTime += Time.unscaledDeltaTime;
      transform.position = Vector2.Lerp(m_lastPosition, m_snapshots[0].Position, Mathf.Clamp01(m_rewindTime / 0.02f));
      transform.rotation = Quaternion.Lerp(m_lastRotation, m_snapshots[0].Rotation, Mathf.Clamp01(m_rewindTime / 0.02f));
    }
  }

  public override void OnRewindStart() {
    base.OnRewindStart();
    m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
    m_rigidbody.simulated = false;

    m_rewindTime = 0f;
    m_lastPosition = m_snapshots[0].Position;
    m_lastRotation = m_snapshots[0].Rotation;
  }

  public override void OnRewindStop() {
    base.OnRewindStop();
    // TODO don't touch velocity, create camera rewind controller instead
    m_rigidbody.linearVelocity = -m_rigidbody.linearVelocity; // TODO ???
    m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
    m_rigidbody.simulated = true;
  }

  public override void OnRewindSave() {
    base.OnRewindSave();
    
    if (m_snapshots.Count > RewindManager.Instance.BufferSizeLimit) {
      m_snapshots.RemoveAt(m_snapshots.Count - 1);
    }

    m_snapshots.Insert(0, new PointInTime(transform.position, m_rigidbody.linearVelocity, transform.rotation));
  }

  public override void OnRewindRollback(RewindManager.RollbackEventData data) {
    base.OnRewindRollback(data);
    
    if (m_snapshots.Count == 0) {
      return;
    }
    
    // m_rewindTime -= 0.02f;
    m_rewindTime = 0.0f;
    
    // Restore state
    transform.position = m_snapshots[0].Position;
    transform.rotation = m_snapshots[0].Rotation;
    m_rigidbody.linearVelocity = -m_snapshots[0].Velocity;
    m_rigidbody.position = m_snapshots[0].Position;
    
    // Store previous state
    m_lastPosition = m_snapshots[0].Position;
    m_lastRotation = m_snapshots[0].Rotation;
    
    // TODO FIX
    //if (tag.Equals("Entity") && m_health.CurrentState.health <= 0 && m_snapshots[0].Health > 0) {
    //  GetComponent<Animator>().SetTrigger("Res");
    //}

    m_snapshots.RemoveAt(0);
  }
}