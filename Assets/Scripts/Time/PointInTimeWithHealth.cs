using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointInTimeWithHealth : PointInTime {
  public int Health {
    get {
      return m_health;
    }
  }

  private int m_health;

  public PointInTimeWithHealth(Vector2 position, Vector2 velocity, Quaternion rotation, int health)
    : base(position, velocity, rotation) {
    m_health = health;
  }
}
