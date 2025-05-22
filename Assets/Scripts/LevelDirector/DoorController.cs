using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
  private Animator m_animator;

  public string LinkedLevel;

  [Space(20)]
  [SerializeField]
  private bool m_canOpen = true;
  [SerializeField]
  private bool m_canClose = true;
  [Space(20)]
  [SerializeField]
  private bool m_isOpen = false;

  private void Start()
  {
    m_animator = GetComponent<Animator>();
    
    if (m_isOpen)
      Open();
  }

  public void Close()
  {
    if(m_canClose)
    m_animator.SetTrigger("Close");
  }
  public void Open() 
  {
    m_isOpen = false;
    if(m_canOpen)
    m_animator.SetTrigger("Open");
  }
}
