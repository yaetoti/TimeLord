using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHolder : MonoBehaviour {
  public static InputHolder Instance { get; private set; }

  public MasterInput MasterInput { get; private set; }

  private void Awake() {
    Initialize();
  }

  private void OnEnable() {
#if UNITY_EDITOR
    Initialize();
#endif
    MasterInput.Enable();
  }


  private void OnDisable() {
    MasterInput?.Disable();
  }

  private void Initialize() {
    if (Instance == this) {
      return;
    }
    
    if (Instance is not null) {
      Destroy(this);
      return;
    }

    Instance = this;
    MasterInput = new MasterInput();
    DontDestroyOnLoad(this);
  }
}
