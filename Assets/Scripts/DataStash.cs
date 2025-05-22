using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStash : MonoBehaviour {
  public static DataStash Instance {
    get {
      if (m_instance == null) {
        //throw new NullReferenceException("DataStash instance is null!");
      }
      return m_instance;
    }
  }

  private static DataStash m_instance;

  public bool m_firstRun;
  public int m_health;
  public int m_currentHealth;
  public bool m_isDead;
  
  private void Awake() {
    if (m_instance == null) {
      m_instance = this;
      m_firstRun = true;
      DontDestroyOnLoad(this);
    } else {
      Destroy(gameObject);
    }
  }

  private void Start() {

  }
}
