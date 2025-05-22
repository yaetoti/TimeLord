using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class VolumeController : MonoBehaviour {
  private Volume m_volume;
  private bool m_is_grainy;
  private bool m_is_saturate;
  private bool m_is_chromatic;
  private bool m_is_active;
  private FilmGrain m_grain;
  private ColorAdjustments m_color;
  private ChromaticAberration m_chromatic;
  private Material m_playerNoise;
  private float m_lightIntensity;
  private Vector2 m_saturationBorders;
  private Vector2 m_chromaticBorders;
  [SerializeField]
  private Material m_material;
  private float m_shaderScale = 1;
  private bool m_applyShader;

  void Start() {
    m_saturationBorders = new Vector2(-50, 70);
    m_chromaticBorders = new Vector2(0.07f, 1);
    m_volume = GetComponent<Volume>();
    m_is_grainy = false;
    m_is_saturate = true;
    m_is_active = true;
    m_is_chromatic = false;
    m_applyShader = false;
    m_volume.profile.TryGet<FilmGrain>(out m_grain);
    m_volume.profile.TryGet<ColorAdjustments>(out m_color);
    m_lightIntensity = m_color.postExposure.value;
    m_color.postExposure.value = -10;
    m_volume.profile.TryGet<ChromaticAberration>(out m_chromatic);

    m_playerNoise = FindObjectOfType<MovementController>().transform.GetChild(1).GetComponent<SpriteRenderer>().material;
  }

  private void Update() {
    m_grain.intensity.value = Mathf.Lerp(m_grain.intensity.value, m_is_grainy ? 1 : 0, 0.1f);
    m_playerNoise.SetFloat("_MaxAlpha", m_grain.intensity.value);

    m_color.saturation.value = (Mathf.Lerp(m_color.saturation.value, m_is_saturate ? m_saturationBorders.y : m_saturationBorders.x, 0.1f));
    m_chromatic.intensity.value = (Mathf.Lerp(m_chromatic.intensity.value, m_is_chromatic ? m_chromaticBorders.y : m_chromaticBorders.x, 0.05f));

    m_color.postExposure.value = (Mathf.Lerp(m_color.postExposure.value, m_is_active ? m_lightIntensity : -10, 0.1f));
    m_shaderScale = Mathf.Lerp(m_shaderScale, (m_applyShader ? 0f : 1f), 0.02f);
    m_material.SetFloat("_TextureLerp", m_shaderScale);
  }

  private void SwitchGrain() {
    m_is_grainy = !m_is_grainy;
  }
  public void SwitchActive() {
    m_is_active = !m_is_active;
  }
  private void SwitchSaturation() {
    Debug.Log("saturation");
    m_is_saturate = !m_is_saturate;
  }
  private void SwitchChromatic() {
    Debug.Log("chromatic");
    m_is_chromatic = !m_is_chromatic;
  }

  public void SwitchTimeRevese() {
    SwitchGrain();
    SwitchSaturation();
    SwitchChromatic();
    m_applyShader = !m_applyShader;
  }
}
