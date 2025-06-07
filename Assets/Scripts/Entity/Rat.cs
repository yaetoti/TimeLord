using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour {
  private float[] m_borders;

  [SerializeField] private float m_speed = 10;
  [SerializeField] private float m_borderWidth = 1;

  void Start() {
    m_borders = new[] {transform.parent.GetChild(1).position.x, transform.parent.GetChild(2).position.x };
  }

  private void FixedUpdate() {
    var direction = -Math.Sign(transform.localScale.x);
    transform.Translate(new Vector2(m_speed / 100 * direction, 0));

    // Flip
    if (Mathf.Abs(transform.position.x - m_borders[0]) < m_borderWidth || Mathf.Abs(transform.position.x - m_borders[1]) < m_borderWidth) {
      transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
  }
}