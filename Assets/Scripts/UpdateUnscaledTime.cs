using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateUnscaledTime : MonoBehaviour {
  private Renderer m_renderer;

  private void Awake() {
    m_renderer = GetComponent<Renderer>();
  }

  void Update() {
    m_renderer.material.SetFloat("_UnscaledTime", Time.unscaledTime);
  }
}
