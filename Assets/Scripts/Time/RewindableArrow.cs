using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class RewindableArrow : RewindableComponent {
  private sealed class TimeState {
    public ulong frameNumber;
    public bool isAlive;
    public Vector2 position;
    public Vector2 velocity;
    public Quaternion rotation;
    
    public TimeState(ulong frameNumber, bool isAlive, Vector2 position, Vector2 velocity, Quaternion rotation) {
      this.frameNumber = frameNumber;
      this.isAlive = isAlive;
      this.position = position;
      this.velocity = velocity;
      this.rotation = rotation;
    }
  }
  
  private Rigidbody2D m_rigidbody;
  private List<TimeState> m_snapshots = new List<TimeState>();

  private float m_rewindTime;
  private Vector2 m_lastPosition;
  private Quaternion m_lastRotation;
  private int m_framesSinceCreation;

  protected override void Start() {
    base.Start();
    
    m_rigidbody = GetComponent<Rigidbody2D>();
  }

  protected void Update() {
    if (TimeManager.Instance.CurrentState is TimeManager.State.REWINDING) {
      m_rewindTime += Time.unscaledDeltaTime;
      transform.position = Vector2.Lerp(m_lastPosition, m_snapshots[0].position, Mathf.Clamp01(m_rewindTime / 0.02f));
      transform.rotation = Quaternion.Lerp(m_lastRotation, m_snapshots[0].rotation, Mathf.Clamp01(m_rewindTime / 0.02f));
    }
  }

  public override void OnRewindStart() {
    base.OnRewindStart();
    m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
    m_rigidbody.simulated = false;

    m_rewindTime = 0f;
    m_lastPosition = m_snapshots[0].position;
    m_lastRotation = m_snapshots[0].rotation;
  }

  public override void OnRewindStop() {
    base.OnRewindStop();
    m_rigidbody.linearVelocity = -m_rigidbody.linearVelocity; // TODO ???
    m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
    m_rigidbody.simulated = true;
  }

  public override void OnRewindUpdate() {
    base.OnRewindUpdate();

    if (!gameObject.activeSelf) {
      return;
    }
    
    ++m_framesSinceCreation;
    if (m_snapshots.Count > TimeManager.Instance.BufferSizeMax) {
      m_snapshots.RemoveAt(m_snapshots.Count - 1);
    }
      
    m_snapshots.Insert(0, new TimeState(TimeManager.Instance.FrameNumber, true, m_rigidbody.position, m_rigidbody.linearVelocity, transform.rotation));
  }

  public override void OnRewindStep(TimeManager.StepEventData data) {
    base.OnRewindStep(data);
    m_rewindTime -= 0.02f;

    // No more snapshots
    if (m_snapshots.Count == 0) {
      return;
    }

    // Wait for the next frame
    TimeState snapshot = m_snapshots[0];
    if (TimeManager.Instance.FrameNumber > snapshot.frameNumber) {
      Debug.Log($"LESS. {snapshot.frameNumber} < {TimeManager.Instance.FrameNumber}");
      return;
    }

    if (!gameObject.activeSelf && m_snapshots[0].isAlive) {
      gameObject.SetActive(true);
    }
    
    transform.position = m_snapshots[0].position;
    m_rigidbody.linearVelocity = -m_snapshots[0].velocity;
    m_rigidbody.position = m_snapshots[0].position;
    m_lastPosition = m_snapshots[0].position;
    m_lastRotation = m_snapshots[0].rotation;
    
    m_snapshots.RemoveAt(0);
    --m_framesSinceCreation;
    
    if (m_framesSinceCreation <= 0) {
      Destroy(gameObject);
    }
  }
}
