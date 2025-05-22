using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointInTime {
  public Vector2 Position {
    get {
      return m_position;
    }
  }

  public Vector2 Velocity {
    get {
      return m_velocity;
    }
  }

  public Quaternion Rotation {
    get {
      return m_rotation;
    }
  }

  private Vector2 m_position;
  private Vector2 m_velocity;
  private Quaternion m_rotation;

  public PointInTime(Vector2 position, Vector2 velocity, Quaternion rotation) {
    m_position = position;
    m_velocity = velocity;
    m_rotation = rotation;
  }
}
