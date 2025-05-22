using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {
  [SerializeField]
  private GameObject m_arrowPrefab;
  [SerializeField]
  private float m_arrowSpeed = 500;
  [SerializeField]
  private float m_meleeOffsetX = 0f;
  [SerializeField]
  private float m_meleeOffsetY = 0f;
  [SerializeField]
  [Min(0)]
  private float m_meleeRadius = 1f;
  [SerializeField]
  [Min(0)]
  private float m_meleeReload = 0.3f;
  [SerializeField]
  [Min(0)]
  private float m_crossbowReload = 1.5f;

  private WeaponType m_weaponType;
  private bool m_isFacingRight;
  private Rigidbody2D m_entityRigidbody;
  private float m_meleeReloadCurrent = 0f;
  private float m_crossbowReloadCurrent = 0f;
  // Animation variable
  private Animator m_animator;

  void Start() {
    m_entityRigidbody = GetComponent<Rigidbody2D>();
    m_isFacingRight = false;
    m_weaponType = WeaponType.KNIFE;
    m_meleeReloadCurrent = 0f;
    m_crossbowReloadCurrent = 0f;

    // Animation script
    m_animator = transform.GetChild(0).GetComponent<Animator>();
  }

  void Update() {
    if (m_meleeReloadCurrent > 0) {
      m_meleeReloadCurrent -= Time.deltaTime;
      if (m_meleeReloadCurrent < 0) {
        m_meleeReloadCurrent = 0;
      }
    }
    if (m_crossbowReloadCurrent > 0) {
      m_crossbowReloadCurrent -= Time.deltaTime;
      if (m_crossbowReloadCurrent < 0) {
        m_crossbowReloadCurrent = 0;
      }
    }

    float movement = InputHolder.Instance.MasterInput.Player.Movement.ReadValue<float>();
    if (movement > 0) {
      m_isFacingRight = false;
    } else if (movement < 0) {
      m_isFacingRight = true;
    }

    if (InputHolder.Instance.MasterInput.Player.Weapon1.WasPressedThisFrame()) {
      m_weaponType = WeaponType.KNIFE;
    } else if (InputHolder.Instance.MasterInput.Player.Weapon2.WasPressedThisFrame()) {
      m_weaponType = WeaponType.CROSSBOW;
    }

    if (InputHolder.Instance.MasterInput.Player.Attack.WasPressedThisFrame()) {
      if (m_weaponType == WeaponType.KNIFE && m_meleeReloadCurrent == 0) {
        m_meleeReloadCurrent = m_meleeReload;
        //Animation script
        m_animator.SetTrigger("KnifeHit");

        Vector2 offset = new Vector2(m_isFacingRight ? m_meleeOffsetX : -m_meleeOffsetX, m_meleeOffsetY);
        Collider2D[] enemies = Physics2D.OverlapCircleAll(m_entityRigidbody.position + offset, m_meleeRadius, 1 << LayerMask.NameToLayer("Enemy"));
        foreach (Collider2D enemy in enemies) {
          enemy.GetComponent<Health>().Damage(1);
        }
      } else if (m_weaponType == WeaponType.CROSSBOW && m_crossbowReloadCurrent == 0) {
        //Animation script
        m_animator.SetTrigger("BowShot");
        //Delay for Animation of crossbow
        m_crossbowReloadCurrent = m_crossbowReload;
        StartCoroutine(BowShot());
      }
    }
  }

  private IEnumerator BowShot()
  {
    yield return new WaitForSeconds(0.15f);
    
    Vector2 direction = Mouse.current.position.ReadValue() - (Vector2)Camera.main.WorldToScreenPoint(GetComponent<Transform>().position);
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    GameObject newArrow = Instantiate(m_arrowPrefab, GetComponent<Transform>().position, Quaternion.AngleAxis(
          angle,
          Vector3.forward));
    Physics2D.IgnoreCollision(newArrow.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    newArrow.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(m_arrowSpeed / 100, 0), ForceMode2D.Impulse);
  }

  private void OnDrawGizmos() {
    Vector2 offset = new Vector2(m_isFacingRight ? m_meleeOffsetX : -m_meleeOffsetX, m_meleeOffsetY);
    Gizmos.DrawWireSphere(GetComponent<Transform>().position + (Vector3)offset, m_meleeRadius);
  }
}

enum WeaponType {
  KNIFE,
  CROSSBOW
}
