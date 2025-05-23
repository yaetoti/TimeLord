using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Per-application singleton
public class DataStash : MonoBehaviour {
  public static DataStash Instance { get; private set; }

  public bool m_firstRun;
  public int m_health;
  public int m_currentHealth;
  public bool m_isDead;
  
  private void Awake() {
    if (Instance == this) {
      return;
    }
    
    if (Instance is not null) {
      Destroy(this);
      return;
    }
    
    Instance = this;
    m_firstRun = true;
    DontDestroyOnLoad(this);
  }
}
