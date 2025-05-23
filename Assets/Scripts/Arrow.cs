using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {
  [SerializeField]
  [Min(0)]
  private int m_damage = 1;
  private Rigidbody2D m_rigidbody;

  private Vector3 m_lastVelocity;

  private void Awake() {
    Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Projectile"), LayerMask.NameToLayer("Projectile"));
  }

  void Start() {
    m_rigidbody = GetComponent<Rigidbody2D>();
  }

  void Update() {
    if (m_rigidbody.simulated) {
      transform.rotation = Quaternion.FromToRotation(Vector3.right, (Vector3)m_rigidbody.linearVelocity);
    }
  }

  private void OnCollisionEnter2D(Collision2D collision) {
    print(m_rigidbody.rotation);
    m_rigidbody.simulated = false;
  }

  private void OnTriggerEnter2D(Collider2D other) {
    if (other.gameObject.TryGetComponent(out Health health)) {
      health.ReduceHealth(m_damage);
      gameObject.SetActive(false);
      //Destroy(gameObject);
    }
  }
}
