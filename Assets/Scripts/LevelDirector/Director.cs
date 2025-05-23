using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class Director : MonoBehaviour {
  public static Director Instance => m_instance;
  private static Director m_instance;
  
  private VolumeController m_volume;

  [SerializeField] private bool m_killAll;

  private int m_enemyCount;

  private bool m_isEnteringScene;

  private void Awake() {
    Initialize();
  }
  
  private void Initialize() {
    if (m_instance is not null) {
      Destroy(this);
      return;
    }

    m_instance = this;
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
    InputHolder.Instance.MasterInput.Disable();
    TimeManager.Instance.ForceResetRewind();
    yield return new WaitForSeconds(0.3f);
    SceneManager.LoadScene(sceneName);
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