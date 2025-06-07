using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

// Per-level singleton
public class LevelDirector : MonoBehaviour {
  public static LevelDirector Instance { get; private set; }

  [SerializeField] private bool m_killAll;
  
  private VolumeController m_volume;
  private int m_enemyCount;
  private bool m_isEnteringScene;
  
  private void Awake() {
    Initialize();
  }

  private void OnEnable() {
#if UNITY_EDITOR
    Initialize();
#endif
  }

  private void OnDestroy() {
    if (Instance == this) {
      Instance = null;
    }
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
  }

  private void Start() {
    m_enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    m_isEnteringScene = false;
    m_volume = FindObjectOfType<VolumeController>();
  }

  public void ChangeScene(string sceneName) {
    if (!m_isEnteringScene)
      StartCoroutine(IEChangeScene(sceneName));
    m_isEnteringScene = true;
  }


  private IEnumerator IEChangeScene(string sceneName) {
    //m_volume.SwitchActive();
    // Pre-load
    InputHolder.Instance.MasterInput.Disable();
    RewindManager.Instance.ForceResetRewind();
    
    // Load
    yield return new WaitForSeconds(0.3f);
    SceneManager.LoadScene(sceneName);
    
    // Post-load
    InputHolder.Instance.MasterInput.Enable();
  }

  private void OpenDoors() {
    DoorController[] doors = FindObjectsOfType<DoorController>();

    for (int i = 0; i < doors.Length; i++) {
      doors[i].Open();
    }
  }

  public void IAmDead(string tag) {
    Debug.Log(tag);
    switch (tag) {
    case "Enemy":

      m_enemyCount--;
      if (m_enemyCount <= 0 && m_killAll) {
        OpenDoors();
      }

      break;

    case "Player":

      RestartLevel();

      break;
    }
  }

  private void RestartLevel() {
    StartCoroutine(IEChangeScene(SceneManager.GetActiveScene().name));
  }
}