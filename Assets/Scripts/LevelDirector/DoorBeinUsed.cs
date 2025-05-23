using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBeinUsed : MonoBehaviour
{

  private string m_linkedScene;

  private LevelDirector m_levelDirector;

  private void Start()
  {
    m_linkedScene = transform.parent.GetComponent<DoorController>().LinkedLevel;
    m_levelDirector = FindObjectOfType<LevelDirector>();
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    
    if (collision.tag == "Player") 
    {
    m_levelDirector.ChangeScene(m_linkedScene);
    }
  }
}
