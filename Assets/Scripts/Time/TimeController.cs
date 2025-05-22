using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeController : MonoBehaviour {
  private Rigidbody2D m_rigidbody;
  private Transform m_transform;

  public int BufferTicks {
    set {
      m_bufferTicks = value;
    }
  }

  public float TimeSlowing {
    set {
      m_timeSlowing = value;
    }
  }

  [SerializeField]
  [Min(1)]
  private int m_bufferTicks = 500;
  [SerializeField]
  [Min(0)]
  private float m_timeSlowing = 2;

  private List<PointInTime> m_timePoints;
  private ControllerState state;
  private float m_timeSlowingCurrent;
  private float m_rewindTime;
  private Vector2 m_lastPosition;
  private Quaternion m_lastRotation;

  //Animation variables
  private VolumeController m_volumeController;
  private bool m_amIPlayer;

  private void Start() {
    
    m_rigidbody = GetComponent<Rigidbody2D>();
    m_transform = GetComponent<Transform>();
    m_timePoints = new List<PointInTime>();
    state = ControllerState.WAITING;

    if (transform.tag == "Player")
      m_amIPlayer = true;
    else
      m_amIPlayer = false;
    m_volumeController = FindObjectOfType<VolumeController>();
    Debug.Log(FindObjectsOfType<VolumeController>().Length);
  }

  private void Update() {
    /*if (state == ControllerState.WAITING && m_input.Player.TimeRewind.WasPressedThisFrame()) {
      m_isRewinding = true;
      BroadcastMessage("RewindStartHandler");
      Debug.Log("Rewind Start");
    }
    if (state != ControllerState.WAITING && m_input.Player.TimeRewind.WasReleasedThisFrame()) {
      m_isRewinding = false;
      BroadcastMessage("RewindStopHandler");
      Debug.Log("Rewind End");
    }*/

    // Processing
    switch (state) {
    case ControllerState.SLOWING_TIME:
      m_timeSlowingCurrent -= Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingCurrent / m_timeSlowing);
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      if (m_timeSlowingCurrent <= 0) {
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.simulated = false;
        state = ControllerState.REWINDING;
        m_lastPosition = m_timePoints[0].Position;
        m_lastRotation = m_timePoints[0].Rotation;
        m_rewindTime = 0f;
          if (m_amIPlayer)
        m_volumeController.SwitchTimeRevese();
      }
      // TODO: Broadcast event + resume all player handling
      break;
    case ControllerState.SPEEDING_UP_TIME:
      m_timeSlowingCurrent += Time.unscaledDeltaTime;
      Time.timeScale = Mathf.Clamp01(m_timeSlowingCurrent / m_timeSlowing);
      Time.fixedDeltaTime = Time.timeScale * 0.02f;
      if (m_timeSlowingCurrent >= m_timeSlowing) {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
        //m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
        //m_rigidbody.simulated = true;
        state = ControllerState.WAITING;
          if (m_amIPlayer)
        m_volumeController.SwitchTimeRevese();
        }
      // TODO: Broadcast event + stop all player handling
      break;
    case ControllerState.REWINDING:
      // TODO: Rewind handling. Move to the next frame after the fixed time step passed
      m_rewindTime += Time.unscaledDeltaTime;
      m_transform.position = Vector2.Lerp(m_lastPosition, m_timePoints[0].Position, Mathf.Clamp01(m_rewindTime / 0.02f));
      m_transform.rotation = Quaternion.Lerp(m_lastRotation, m_timePoints[0].Rotation, Mathf.Clamp01(m_rewindTime / 0.02f));
      if (m_rewindTime > 0.02f) {
        m_rewindTime -= 0.02f;
        // TODO lerp position percent = m_rewindTime / 0.02f
        m_transform.position = m_timePoints[0].Position;
        m_rigidbody.linearVelocity = -m_timePoints[0].Velocity;
        m_rigidbody.position = m_timePoints[0].Position;
        m_lastPosition = m_timePoints[0].Position;
        m_lastRotation = m_timePoints[0].Rotation;
        m_timePoints.RemoveAt(0);
        if (m_timePoints.Count == 0) {
          m_rigidbody.linearVelocity = Vector2.zero;
          m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
          m_rigidbody.simulated = true;
          state = ControllerState.SPEEDING_UP_TIME;
          break;
        }
      }
      break;
    default:
      break;
    }


    // Input
    if (InputHolder.Instance.MasterInput.Player.TimeRewind.WasPressedThisFrame()) {
      switch (state) {
      case ControllerState.WAITING:
        state = ControllerState.SLOWING_TIME;
        m_timeSlowingCurrent = m_timeSlowing;
        break;
      case ControllerState.SLOWING_TIME:
        state = ControllerState.SPEEDING_UP_TIME;
        break;
      case ControllerState.REWINDING:
        m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_rigidbody.simulated = true;
        state = ControllerState.SPEEDING_UP_TIME;
        break;
      case ControllerState.SPEEDING_UP_TIME:
        state = ControllerState.SPEEDING_UP_TIME;
        break;
      }
    }
  }

  private void FixedUpdate() {
    if (state != ControllerState.REWINDING) {
      // TODO: Handle time slowind. Capture position every fixed time step 0.02
      if (m_timePoints.Count > m_bufferTicks) {
        m_timePoints.RemoveAt(m_timePoints.Count - 1);
      }
      m_timePoints.Insert(0, new PointInTime(m_rigidbody.position, m_rigidbody.linearVelocity, m_transform.rotation));
    }
  }
}

enum ControllerState {
  WAITING,
  SLOWING_TIME,
  REWINDING,
  SPEEDING_UP_TIME,
}
