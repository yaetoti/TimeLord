using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHolder : MonoBehaviour {
  public static InputHolder Instance {
    get {
      return m_instance;
    }
  }

  public MasterInput MasterInput {
    get {
      return m_masterInput;
    }
  }

  private static InputHolder m_instance;

  private MasterInput m_masterInput;

  private void Awake() {
    Initialize();
  }

  private void OnEnable() {
#if UNITY_EDITOR
    if (m_instance != this) {
      Initialize();
    }
#endif
    m_masterInput.Enable();
  }


  private void OnDisable() {
    m_masterInput?.Disable();
  }

  private void Initialize() {
    if (m_instance != null) {
      Destroy(this);
    }

    m_instance = this;
    m_masterInput = new MasterInput();
  }
}
