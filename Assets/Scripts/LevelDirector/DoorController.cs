using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour {
  private static readonly int CLOSE = Animator.StringToHash("Close");
  private static readonly int OPEN = Animator.StringToHash("Open");
  public string LinkedLevel => m_linkedLevel;
  
  [SerializeField]
  private string m_linkedLevel;

  private Animator m_animator;

  [Space(20)]
  [SerializeField]
  private bool m_canOpen = true;
  [SerializeField]
  private bool m_canClose = true;
  [Space(20)]
  [SerializeField]
  private bool m_isOpen = false;

  private void Start() {
    m_animator = GetComponent<Animator>();
    SetState(m_isOpen);
  }

  public void SetState(bool isOpen) {
    if (isOpen) {
      Open();
    } else {
      Close();
    }
  }
  
  public void Close() {
    if (m_canClose) {
      m_animator.ResetTrigger(OPEN);
      m_animator.SetTrigger(CLOSE);
      m_isOpen = false;
    }
  }

  public void Open() {
    if (m_canOpen) {
      m_animator.ResetTrigger(CLOSE);
      m_animator.SetTrigger(OPEN);
      m_isOpen = true;
    }
  }
}