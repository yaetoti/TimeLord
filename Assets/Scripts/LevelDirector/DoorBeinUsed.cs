using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBeinUsed : MonoBehaviour
{

  private string m_linkedScene;

  private Director m_director;

  private void Start()
  {
    m_linkedScene = transform.parent.GetComponent<DoorController>().LinkedLevel;
    m_director = FindObjectOfType<Director>();
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    
    if (collision.tag == "Player") 
    {
    m_director.ChangeScene(m_linkedScene);
    }
  }
}
