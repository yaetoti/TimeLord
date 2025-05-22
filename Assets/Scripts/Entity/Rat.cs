using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
  private Vector2 m_borders;
  private Rigidbody2D m_rBody;
  private SpriteRenderer m_sprite;

  private float m_direction = 1;
  
  [SerializeField]
  private float m_speed = 10;
  [SerializeField]
  private float m_borderWidth = 1;
    void Start()
    {
    m_sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    m_sprite.flipX = true;
    m_rBody = GetComponent<Rigidbody2D>();
    m_borders.x = transform.parent.GetChild(1).position.x;
    m_borders.y = transform.parent.GetChild(2).position.x;
    }

  private void FixedUpdate()
  {
    transform.Translate(new Vector2(m_speed / 100 * m_direction, 0));

    if (Mathf.Abs(transform.position.x - m_borders.x) < m_borderWidth ||
      Mathf.Abs(transform.position.x - m_borders.y) < m_borderWidth)
    {
      Flip();
    }
  }
  private void Flip()
  {
    m_direction *= -1;
    m_sprite.flipX = !m_sprite.flipX;
  }
}
