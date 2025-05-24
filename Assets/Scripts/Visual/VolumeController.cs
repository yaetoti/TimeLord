using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class VolumeController : MonoBehaviour {
  private static readonly int MAX_ALPHA = Shader.PropertyToID("_MaxAlpha");
  private static readonly int TEXTURE_LERP = Shader.PropertyToID("_TextureLerp");

  [SerializeField]
  private Material m_material;
  
  // Components
  private Volume m_volume;
  private FilmGrain m_grain;
  private ColorAdjustments m_color;
  private ChromaticAberration m_chromatic;
  private Material m_playerNoise;

  // Config
  private Vector2 m_saturationBorders;
  private Vector2 m_chromaticBorders;
  private float m_lightIntensity;
  
  // State
  private bool m_isGrainActive;
  private bool m_isSaturationActive = true;
  private bool m_isChromaticActive;
  private bool m_applyShader;
  private bool m_isSceneExposed = true;
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
    // Lerp values
    float newGrain = Mathf.Lerp(m_grain.intensity.value, m_isGrainActive ? 1 : 0, 0.1f);
    float newSaturation = Mathf.Lerp(m_color.saturation.value, m_isSaturationActive ? m_saturationBorders.y : m_saturationBorders.x, 0.1f);
    float newChromatic = Mathf.Lerp(m_chromatic.intensity.value, m_isChromaticActive ? m_chromaticBorders.y : m_chromaticBorders.x, 0.05f);
    float newExposure = Mathf.Lerp(m_color.postExposure.value, m_isSceneExposed ? m_lightIntensity : -10, 0.1f);
    float newShaderScale = Mathf.Lerp(m_shaderScale, (m_applyShader ? 0f : 1f), 0.02f);
    
    // Update volume
    m_color.saturation.value = newSaturation;
    m_chromatic.intensity.value = newChromatic;
    m_color.postExposure.value = newExposure;
    m_grain.intensity.value = newGrain;
    m_shaderScale = newShaderScale;

    // Update shader
    m_playerNoise.SetFloat(MAX_ALPHA, newGrain);
    m_material.SetFloat(TEXTURE_LERP, newShaderScale);
  }
  
  private void HandleTimeManagerStateChanged(TimeManager.StateChangedEventData data) {
    // Time rewind start
    if (data.CurrentState is TimeManager.RewindState.SPEEDING_UP or TimeManager.RewindState.WAITING) {
      m_isGrainActive = false;
      m_isSaturationActive = true;
      m_isChromaticActive = false;
      m_applyShader = false;
      return;
    }

    // Time rewind stop
    if (data.CurrentState is TimeManager.RewindState.SLOWING_DOWN or TimeManager.RewindState.REWINDING) {
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
