using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiPlayerHealth : MonoBehaviour {
  [SerializeField]
  Texture m_heartTexture;
  [SerializeField]
  private float m_heartDistance = 10;
  [SerializeField]
  private float m_offsetX = 0;
  [SerializeField]
  private float m_offsetY = 0;
  [SerializeField]
  private float m_heartScale = 1;

  Health m_playerHealth;

  void Start() {
    m_playerHealth = GetComponent<Health>();
  }

  void Update() {

  }

  private void OnGUI() {
    Vector2 offset = new Vector2(m_offsetX, m_offsetY);
    Vector2 size = new Vector2(m_heartTexture.width * m_heartScale, m_heartTexture.height * m_heartScale);
    for (int i = 0; i < m_playerHealth.CurrentHealth; ++i) {
      GUI.DrawTexture(new Rect(offset, size), m_heartTexture);
      offset.x += size.x + m_heartDistance * Camera.main.aspect;
    }
  }
}
