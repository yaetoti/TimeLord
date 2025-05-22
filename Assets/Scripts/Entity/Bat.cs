using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour
{
  private Vector2[] m_targets;

  private Vector2 m_currentTarget;
  private Rigidbody2D m_rBody;

  [SerializeField]
  private float speed = 5;

  private void Start()
  {
    m_rBody = GetComponent<Rigidbody2D>();
    m_targets = new Vector2[transform.parent.childCount];
    for (int i = 0; i < transform.parent.childCount; i++) 
    {
      Transform child = transform.parent.GetChild(i);
      m_targets[i] = new Vector2(child.position.x, child.position.y);
    }
    StartCoroutine(ChooseTarget());
  }

  private void FixedUpdate()
  {
    m_rBody.AddForce((m_currentTarget - new Vector2(transform.position.x, transform.position.y)) * speed / 100, ForceMode2D.Impulse);
    if (m_rBody.linearVelocity.x < 0)
      transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
    else
      transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
  }

  private IEnumerator ChooseTarget() 
  {
    while (true)
    {
      m_currentTarget = m_targets[Random.Range(1, m_targets.Length)];
      Debug.Log(m_currentTarget);
      yield return new WaitForSeconds(1);
    }
  }
}
