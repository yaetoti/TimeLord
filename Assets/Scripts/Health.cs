using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
  public int CurrentHealth {
    get {
      return m_currentHealth;
    }
  }

  public bool ShieldUp {
    get; set;
  }

  [SerializeField]
  [Min(0)]
  private int m_health = 3;
  private bool m_shieldUp;

  private int m_currentHealth;
  private bool m_isDead;

  //Animation variables
  [SerializeField]
  private ParticleSystem m_damageFX;

  void Start() {
    m_shieldUp = false;

    if (tag.Equals("Player") && !DataStash.Instance.m_firstRun) {
      m_health = DataStash.Instance.m_health;
      m_currentHealth = DataStash.Instance.m_currentHealth;
      m_isDead = DataStash.Instance.m_isDead;
      return;
    } else {
      m_currentHealth = m_health;
      m_isDead = m_health == 0;
    }
  }

  private void OnDestroy() {
    if (tag.Equals("Player")) {
      DataStash.Instance.m_health = m_health;
      DataStash.Instance.m_currentHealth = m_currentHealth;
      DataStash.Instance.m_isDead = m_isDead;
      DataStash.Instance.m_firstRun = false;
    }
  }

  public void Damage(int damage) {
    if (m_shieldUp) {
      m_shieldUp = false;
      return;
    }

    m_currentHealth -= damage;

    //Animation script
    if (m_currentHealth >= 0)
      StartCoroutine(DamageFX());
    //End of animation script

    if (m_currentHealth <= 0) {
      m_isDead = true;

      //Animation script
      //if(m_currentHealth == 0)
      Die();
    } else {
      m_isDead = false;
    }
  }

  public void ForceSet(int health) {
    if (health < 0) {
      m_currentHealth = 0;
    } else {
      m_currentHealth = health;
    }

    if (m_currentHealth <= 0) {
      m_isDead = true;

      //Animation script
      //if (m_currentHealth == 0) {
        Die();
      //}
    }
  }

  public bool IsDead() {
    return m_isDead;
  }

  //Animation script

  private IEnumerator DamageFX() 
  {
    ParticleSystem system = Instantiate(m_damageFX, transform.position, Quaternion.identity);
    system.Play();
    yield return new WaitForSeconds(1);
    Destroy(system);
  }
  private void Die() 
  {
    LevelDirector.Instance.IAmDead(transform.tag);
    if (transform.tag != "Player")
    {
      transform.GetComponent<CapsuleCollider2D>().enabled = false;
      GetComponent<Animator>().SetTrigger("Die");
    }
    else 
    {
      GetComponent<MovementController>().Die();
    }
  }
}
