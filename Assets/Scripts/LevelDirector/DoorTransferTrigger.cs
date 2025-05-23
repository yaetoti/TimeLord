using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTransferTrigger : MonoBehaviour {
  private string m_linkedScene;

  private void Start() {
    m_linkedScene = transform.parent.GetComponent<DoorController>().LinkedLevel;
  }

  private void OnTriggerEnter2D(Collider2D collision) {
    if (collision.CompareTag("Player")) {
      LevelDirector.Instance.ChangeScene(m_linkedScene);
    }
  }
}