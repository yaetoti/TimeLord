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
  [SerializeField]
  private float m_predictionModifier = 1.0f;
  [SerializeField]
  private float m_cursorPredictionModifier = 1.0f;
  [SerializeField]
  private float m_cursorDistanceModifier = 1.0f;

  private Camera m_camera;
  private Rigidbody2D m_targetRigidbody;
  private Transform m_cameraPos;
  private float m_xVelocity = 0.0f;
  private float m_yVelocity = 0.0f;
  private bool m_needMove = false;

  private void Start() {
    m_camera = GetComponent<Camera>();
    m_cameraPos = GetComponent<Transform>();
    m_targetRigidbody = m_targetObject.GetComponent<Rigidbody2D>();
  }

  private void LateUpdate() {
    float height = 2f * m_camera.orthographicSize;
    float width = height * m_camera.aspect;
    float diagonal = Mathf.Sqrt(width * width + height * height);
    
    Vector2 mouseCoords = InputHolder.Instance.MasterInput.Player.MousePosition.ReadValue<Vector2>();
    Vector3 worldCoords = m_camera.ScreenToWorldPoint(new Vector3(mouseCoords.x, mouseCoords.y, m_camera.nearClipPlane));
    Vector3 targetDelta = worldCoords - m_targetObject.transform.position;
    Vector2 targetDelta2D = new Vector2(targetDelta.x, targetDelta.y);
    targetDelta2D = Vector2.ClampMagnitude(targetDelta2D, m_cursorDistanceModifier * diagonal * 0.5f) / (0.5f * diagonal);


    float deltaX = Mathf.Abs(m_cameraPos.position.x - m_targetRigidbody.position.x - m_targetRigidbody.linearVelocity.x * m_predictionModifier);
    float deltaY = Mathf.Abs(m_cameraPos.position.y - m_targetRigidbody.position.y - m_targetRigidbody.linearVelocity.y * m_predictionModifier);
    if (deltaX > m_outerDeltaX || deltaY > m_outerDeltaY) {
      m_needMove = true;
    }
    if (deltaX < m_innerDeltaX && deltaY < m_innerDeltaY) {
      m_needMove = false;
    }

    if (m_needMove) {
      // m_cameraPos.position = new Vector3(
      //   Mathf.SmoothDamp(m_cameraPos.position.x, m_targetRigidbody.position.x + m_targetRigidbody.linearVelocity.x, ref m_xVelocity, m_time, Mathf.Infinity, Time.unscaledDeltaTime),
      //   Mathf.SmoothDamp(m_cameraPos.position.y, m_targetRigidbody.position.y + m_targetRigidbody.linearVelocity.y, ref m_yVelocity, m_time, Mathf.Infinity, Time.unscaledDeltaTime),
      //   m_cameraPos.position.z);
      m_cameraPos.position = new Vector3(
        MathUtils.SmoothDampOvershoot(m_cameraPos.position.x, m_targetRigidbody.position.x + m_targetRigidbody.linearVelocity.x + targetDelta2D.x * m_cursorPredictionModifier, ref m_xVelocity, m_time, Mathf.Infinity, Time.unscaledDeltaTime),
        MathUtils.SmoothDampOvershoot(m_cameraPos.position.y, m_targetRigidbody.position.y + m_targetRigidbody.linearVelocity.y + targetDelta2D.y * m_cursorPredictionModifier, ref m_yVelocity, m_time, Mathf.Infinity, Time.unscaledDeltaTime),
        m_cameraPos.position.z);
    }
  }
  
  private void OnDrawGizmosSelected() {
    if (!m_targetRigidbody) {
      return;
    }

    // Inner boundary
    Gizmos.color = Color.red;
    Gizmos.DrawWireCube(m_targetRigidbody.position + m_targetRigidbody.linearVelocity, new Vector3(m_innerDeltaX * 2, m_innerDeltaY * 2, 0));

    // Outer boundary
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireCube(m_targetRigidbody.position + m_targetRigidbody.linearVelocity, new Vector3(m_outerDeltaX * 2, m_outerDeltaY * 2, 0));
  }
}
