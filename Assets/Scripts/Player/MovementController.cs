using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class MovementController : MonoBehaviour {
  // Components
  private Animator m_animator;
  private Rigidbody2D m_rigidbody;

  // Parameters
  [SerializeField] private float m_speed = 5.0f;
  [SerializeField] [Min(0)] private float m_jumpHeight = 1.0f;
  [SerializeField] private float m_deceleration = 0.9f;
  [SerializeField] private float m_groundAngle = 55.0f;
  [SerializeField] [Min(0)] private int m_additionalJumps = 1;
  [SerializeField] [Min(0)] private int m_jumpCacheTime = 5;
  [SerializeField] [Min(0)] private int m_jumpCooldown = 5;
  [SerializeField] [Min(0)] private int m_coyoteTime = 0;

  // State
  private bool m_isControllerEnabled;
  private List<Vector2> m_collisionNormals;
  private bool m_desireToJump = false;
  private float m_wishDir = 0.0f;

  private int m_jumpsLeft = 0;
  private float m_jumpCacheTimeCurrent = 0;
  private int m_jumpCooldownCurrent = 0;
  private int m_coyoteTimeCurrent = 0;
  private bool m_firstJumpPerformed = false;

  // Animations
  private static readonly int RUN = Animator.StringToHash("Run");
  private static readonly int KNIFE = Animator.StringToHash("Knife");
  private static readonly int CROSSBOW = Animator.StringToHash("Crossbow");

#region Lifecycle methods

  private void Start() {
    m_animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
    m_rigidbody = GetComponent<Rigidbody2D>();
    
    m_collisionNormals = new List<Vector2>();
    m_isControllerEnabled = true;
  }

  private void Update() {
    if (!m_isControllerEnabled) {
      return;
    }

    // Collect input
    m_wishDir = InputHolder.Instance.MasterInput.Player.Movement.ReadValue<float>();

    if (InputHolder.Instance.MasterInput.Player.Jump.WasPressedThisFrame()) {
      m_desireToJump = true;
      m_jumpCacheTimeCurrent = m_jumpCacheTime;
    }
    
    // Update animations
    if (m_wishDir != 0) {
      m_animator.SetBool(RUN, true);
      m_animator.gameObject.transform.localScale = new Vector3(m_wishDir, 1.0f, 1.0f);
    }
    else {
      m_animator.SetBool(RUN, false);
    }
  }

  private void FixedUpdate() {
    // Ground detection, calculating the movement vector
    var steepestMovementVector = GetSteepestMovementVector();
    Vector2 moveVec = steepestMovementVector ?? new Vector2(m_wishDir, 0);
    bool isOnGround = steepestMovementVector is not null;

    // Apply movement forces
    m_rigidbody.AddForce(m_speed * Mathf.Abs(moveVec.x) * new Vector2(m_wishDir, 0), ForceMode2D.Impulse);
    m_rigidbody.AddForce(m_rigidbody.linearVelocity.x * m_deceleration * Vector2.left, ForceMode2D.Impulse);

    // Jumping
    if (m_desireToJump && m_jumpCooldownCurrent == 0) {
      float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);
      
      // Reduce jumping speed if the player is already moving upwards
      if (m_rigidbody.linearVelocity.y > 0f) {
        jumpSpeed = Math.Max(0, jumpSpeed - m_rigidbody.linearVelocity.y);
      }

      if (!m_firstJumpPerformed && m_coyoteTimeCurrent > 0) {
        m_desireToJump = false;
        m_firstJumpPerformed = true;
        m_coyoteTimeCurrent = 0;
        m_jumpCooldownCurrent = m_jumpCooldown;
        m_rigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
      }
      else if (m_jumpsLeft > 0) {
        --m_jumpsLeft;
        m_jumpCooldownCurrent = m_jumpCooldown;
        m_desireToJump = false;
        m_rigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
      }
    }

    // Decrement jumping timer
    if (m_jumpCooldownCurrent > 0) {
      --m_jumpCooldownCurrent;
    }

    // Update jumping state
    if (isOnGround) {
      // Reset jumping state
      if (m_jumpCooldownCurrent == 0) {
        m_firstJumpPerformed = false;
        m_coyoteTimeCurrent = m_coyoteTime;
        m_jumpsLeft = m_additionalJumps;
      }
    }
    else {
      // Decrement coyote time
      if (m_coyoteTimeCurrent > 0) {
        --m_coyoteTimeCurrent;
      }
    }

    // Update jumping cache time
    if (m_desireToJump) {
      if (m_jumpCacheTimeCurrent > 0) {
        --m_jumpCacheTimeCurrent;
      }
      else {
        m_desireToJump = false;
      }
    }

    m_collisionNormals.Clear();
  }

  private void OnCollisionStay2D(Collision2D collision) {
    if (!collision.enabled) {
      return;
    }
    
    var contacts = new List<ContactPoint2D>();
    collision.GetContacts(contacts);
    
    foreach (var contact in contacts) {
      m_collisionNormals.Add(contact.normal);
    }
  }

#endregion

#region Public methods

  public void DisableController() {
    m_isControllerEnabled = false;
    m_wishDir = 0;
  }

#endregion
  
#region Private methods
  
  private Vector2? GetSteepestMovementVector() {
    // Result - movement vector, perpendicular to the steepest ground normal
    Vector2? steepestForward = null;
    var steepestNormalY = float.MinValue;
    
    for (int i = 0; i < m_collisionNormals.Count; ++i) {
      // Filter out normals that are not perpendicular to the ground
      if (Vector2.Angle(Vector2.up, m_collisionNormals[i]) > m_groundAngle) {
        continue;
      }
      
      //Debug.DrawRay(m_rigidbody.position, m_collisionNormals[i], Color.cyan);

      // Find move vector
      var moveVec = m_wishDir > 0
        ? new Vector2(m_collisionNormals[i].y, -m_collisionNormals[i].x) // Rotate by 90
        : m_wishDir < 0
        ? new Vector2(-m_collisionNormals[i].y, m_collisionNormals[i].x) // Rotate by -90
        : Vector2.zero;

      // Compare move vector with the current steepest move vector
      if (moveVec.y > steepestNormalY) {
        steepestNormalY = moveVec.y;
        steepestForward = moveVec;
      }
    }

    return steepestForward;
  }
  
#endregion
}