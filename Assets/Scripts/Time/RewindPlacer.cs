using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindPlacer : MonoBehaviour {
  [SerializeField]
  [Min(1)]
  private int m_bufferTicks = 500;
  [SerializeField]
  [Min(0)]
  private float m_timeSlowing = 2;

  void Start() {
    Rigidbody2D[] rigidbodies = FindObjectsOfType<Rigidbody2D>();
    foreach (Rigidbody2D rigidbody in rigidbodies) {
      TimeController controller = rigidbody.gameObject.AddComponent<TimeController>();
      controller.BufferTicks = m_bufferTicks;
      controller.TimeSlowing = m_timeSlowing;
    }
  }
}
