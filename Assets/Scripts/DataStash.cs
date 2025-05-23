using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Per-application singleton
public class DataStash : MonoBehaviour {
  public static DataStash Instance { get; private set; }

  public int m_maxHealth = 3;
  public int m_health = 3;
  public ApplicationData m_data;
  
  private void Awake() {
    Initialize();
  }

  private void OnEnable() {
#if UNITY_EDITOR
    Initialize();
#endif
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
    DontDestroyOnLoad(this);
  }

  private void Start() {
    m_data = new ApplicationData();
  }
}
