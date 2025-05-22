using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageImpact : MonoBehaviour
{
  [SerializeField]
  private float m_impulseStrength = 1;

  private Material m_material;

  private float m_alpha;

  private bool m_isTweaking;

  private Rigidbody2D m_rBody;
    void Start()
    {
    m_isTweaking = false;
    m_alpha = 1;
    m_rBody = GetComponent<Rigidbody2D>();
    m_material = transform.GetChild(0).GetComponent<SpriteRenderer>().material;
    }

  public void Damage( Vector2 enemyPos) 
  {
    m_rBody.AddForce((new Vector2(transform.position.x, transform.position.y) - enemyPos).normalized * m_impulseStrength, ForceMode2D.Impulse);

    StartCoroutine(damageCoolDown());
  }

  private void FixedUpdate()
  {
    m_alpha = Mathf.Lerp(m_alpha, m_isTweaking ? 0.2f : 1, 0.5f);
    m_material.SetFloat("_Alpha", m_alpha);
  }

  private IEnumerator damageCoolDown() 
  {
    m_isTweaking = true;
    string tag = transform.tag;
    transform.tag = "DamagedPlayer";
    yield return new WaitForSeconds(1);
    transform.tag = tag;
    m_isTweaking = false;
  }
}
