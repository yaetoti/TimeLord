using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class VolumeController : MonoBehaviour {
  [SerializeField]
  private Material m_material;
  
  // Components
  private Volume m_volume;
  private FilmGrain m_grain;
  private ColorAdjustments m_color;
  private ChromaticAberration m_chromatic;
  private Material m_playerNoise;

  // Settings
  private bool m_isGrainActive;
  private bool m_isSaturationActive = true;
  private bool m_isChromaticActive;
  private bool m_applyShader;
  private bool m_isSceneExposed = true;
  private Vector2 m_saturationBorders;
  private Vector2 m_chromaticBorders;
  private float m_lightIntensity;
  private float m_shaderScale = 1;

  void Start() {
    // Register
    TimeManager.Instance.RegisterStateChangedEvent(HandleTimeManagerStateChanged);
    
    // Init components
    m_volume = GetComponent<Volume>();
    m_volume.profile.TryGet(out m_grain);
    m_volume.profile.TryGet(out m_color);
    m_volume.profile.TryGet(out m_chromatic);
    m_playerNoise = FindFirstObjectByType<MovementController>().transform.GetChild(1).GetComponent<SpriteRenderer>().material;

    // Settings
    m_saturationBorders = new Vector2(-50, 70);
    m_chromaticBorders = new Vector2(0.07f, 1);
    m_lightIntensity = m_color.postExposure.value;
    m_color.postExposure.value = -10;
  }

  private void OnDestroy() {
    TimeManager.Instance?.UnregisterStateChangedEvent(HandleTimeManagerStateChanged);
  }

  private void Update() {
    m_grain.intensity.value = Mathf.Lerp(m_grain.intensity.value, m_isGrainActive ? 1 : 0, 0.1f);
    m_playerNoise.SetFloat("_MaxAlpha", m_grain.intensity.value);

    m_color.saturation.value = (Mathf.Lerp(m_color.saturation.value, m_isSaturationActive ? m_saturationBorders.y : m_saturationBorders.x, 0.1f));
    m_chromatic.intensity.value = (Mathf.Lerp(m_chromatic.intensity.value, m_isChromaticActive ? m_chromaticBorders.y : m_chromaticBorders.x, 0.05f));

    m_color.postExposure.value = (Mathf.Lerp(m_color.postExposure.value, m_isSceneExposed ? m_lightIntensity : -10, 0.1f));
    m_shaderScale = Mathf.Lerp(m_shaderScale, (m_applyShader ? 0f : 1f), 0.02f);
    m_material.SetFloat("_TextureLerp", m_shaderScale);
  }
  
  private void HandleTimeManagerStateChanged(TimeManager.StateChangedEventData data) {
    // Time rewind start
    if (data.CurrentState is TimeManager.State.SPEEDING_UP_TIME or TimeManager.State.WAITING) {
      m_isGrainActive = false;
      m_isSaturationActive = true;
      m_isChromaticActive = false;
      m_applyShader = false;
      return;
    }

    // Time rewind stop
    if (data.CurrentState is TimeManager.State.SLOWING_TIME or TimeManager.State.REWINDING) {
      m_isGrainActive = true;
      m_isSaturationActive = false;
      m_isChromaticActive = true;
      m_applyShader = true;
      return;
    }
  }

  public void SwitchActive() {
    m_isSceneExposed = !m_isSceneExposed;
  }
}
