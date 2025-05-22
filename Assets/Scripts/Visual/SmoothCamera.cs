using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour {
  [SerializeField]
  [InspectorName("Target")]
  private GameObject m_targetObject;
  [SerializeField]
  [Min(0)]
  [InspectorName("Time to move")]
  private float m_time = 0.3f;
  [SerializeField]
  [Min(0)]
  [InspectorName("Stop delta X")]
  private float m_innerDeltaX = 0.5f;
  [SerializeField]
  [Min(0)]
  [InspectorName("Stop delta Y")]
  private float m_innerDeltaY = 0.5f;
  [SerializeField]
  [Min(0)]
  [InspectorName("Start delta X")]
  private float m_outerDeltaX = 3.0f;
  [SerializeField]
  [Min(0)]
  [InspectorName("Start delta Y")]
  private float m_outerDeltaY = 3.0f;

  private Rigidbody2D m_targetRigidbody;
  private Transform m_cameraPos;
  private float m_xVelocity = 0.0f;
  private float m_yVelocity = 0.0f;
  private bool m_needMove = false;

  private void Start() {
    m_cameraPos = GetComponent<Transform>();
    m_targetRigidbody = m_targetObject.GetComponent<Rigidbody2D>();
  }

  private void LateUpdate() {
    float deltaX = Mathf.Abs(m_cameraPos.position.x - m_targetRigidbody.position.x - m_targetRigidbody.linearVelocity.x);
    float deltaY = Mathf.Abs(m_cameraPos.position.y - m_targetRigidbody.position.y - m_targetRigidbody.linearVelocity.y);
    if (deltaX > m_outerDeltaX || deltaY > m_outerDeltaY) {
      m_needMove = true;
    }
    if (deltaX < m_innerDeltaX && deltaY < m_innerDeltaY) {
      m_needMove = false;
    }

    if (m_needMove) {
      //Debug.Log("LOH!");
      m_cameraPos.position = new Vector3(
        Mathf.SmoothDamp(m_cameraPos.position.x, m_targetRigidbody.position.x + m_targetRigidbody.linearVelocity.x, ref m_xVelocity, m_time, Mathf.Infinity, Time.unscaledDeltaTime),
        Mathf.SmoothDamp(m_cameraPos.position.y, m_targetRigidbody.position.y + m_targetRigidbody.linearVelocity.y, ref m_yVelocity, m_time, Mathf.Infinity, Time.unscaledDeltaTime),
        m_cameraPos.position.z);
    }
  }
}
