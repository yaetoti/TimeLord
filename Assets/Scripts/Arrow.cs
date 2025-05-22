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
    TimeController controller = gameObject.AddComponent<TimeController>();
    controller.BufferTicks = 500;
    controller.TimeSlowing = 0.5f;
  }

  void Update() {
    if (m_rigidbody.simulated) {
      transform.rotation = Quaternion.FromToRotation(Vector3.right, (Vector3)m_rigidbody.linearVelocity);
    }
    // ������� �������, ���������� ����
    /*RaycastHit2D[] hits = new RaycastHit2D[10];
    int i = m_rigidbody.Cast(transform.forward, hits, m_rigidbody.velocity.magnitude * Time.deltaTime);
    if (i == 0) {
      return;
    }
    i -= 1;
    for (; i >= 0; --i) {
      if (hits[i].collider.TryGetComponent(out Health health)) {
        health.Damage(m_damage);
        Destroy(gameObject);
      }
    }*/
  }

  private void FixedUpdate() {
  }

  private void OnCollisionEnter2D(Collision2D collision) {
    print(m_rigidbody.rotation);
    m_rigidbody.simulated = false;
  }

  private void OnTriggerEnter2D(Collider2D other) {
    if (other.gameObject.TryGetComponent(out Health health)) {
      health.Damage(m_damage);
      Destroy(gameObject);
    }
  }
}
