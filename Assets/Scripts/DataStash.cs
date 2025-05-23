using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Per-application singleton
public class DataStash : MonoBehaviour {
  public static DataStash Instance { get; private set; }

  public int m_health = 3;
  public int m_currentHealth = 3;
  public bool m_isDead = false;
  
  private void Awake() {
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
}
