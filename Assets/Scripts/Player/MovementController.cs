using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
  private Animator m_animator;
    private Rigidbody2D m_rigidbody;

    [SerializeField]
    private float m_speed = 5.0f;
    [SerializeField]
    [Min(0)]
    private float m_jumpHeight = 1.0f;
    [SerializeField]
    private float m_deceleration = 0.9f;
    [SerializeField]
    private float m_groundAngle = 55.0f;
    [SerializeField]
    [Min(0)]
    private int m_additionalJumps = 1;
    [SerializeField]
    [Min(0)]
    private int m_jumpCacheTime = 5;
    [SerializeField]
    [Min(0)]
    private int m_jumpCooldown = 5;
    [SerializeField]
    [Min(0)]
    private int m_coyoteTime = 0;

    //DeathChecker
    private bool m_isPlayerAlive;

    //Animation variables
    //private Material m_playerMat;

    private float m_wishDir = 0.0f;
    private bool m_desireToJump = false;
    private float m_jumpCacheTimeCurrent = 0;
    private int m_jumpCooldownCurrent = 0;
    private bool m_firstJumpPerformed = false;
    private int m_jumpsLeft = 0;
    private int m_coyoteTimeCurrent = 0;
    private List<Vector2> m_groundNormals;
    private static readonly int Speed = Shader.PropertyToID("_Speed");
    private static readonly int Flip = Shader.PropertyToID("_flip");
    
    private static readonly int RUN = Animator.StringToHash("Run");
    private static readonly int KNIFE = Animator.StringToHash("Knife");
    private static readonly int CROSSBOW = Animator.StringToHash("Crossbow");

    void Start() {
      m_animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_groundNormals = new List<Vector2>();

        m_isPlayerAlive = true;

        //m_playerMat = transform.GetChild(0).GetComponent<SpriteRenderer>().material;
    }

    void Update() {
      if (m_isPlayerAlive) {
        m_wishDir = InputHolder.Instance.MasterInput.Player.Movement.ReadValue<float>();

        //Player animation controller
        if (m_wishDir != 0) {
          m_animator.SetBool(RUN, true);
          m_animator.gameObject.transform.localScale = new Vector3(m_wishDir, 1.0f, 1.0f);
          //m_playerMat.SetFloat(Flip, (m_wishDir > 0) ? 0 : 1);
          //m_playerMat.SetFloat(Speed, 30);
        }
        else {
          m_animator.SetBool(RUN, false);
          //m_playerMat.SetFloat(Speed, 5);
        }

        if (InputHolder.Instance.MasterInput.Player.Jump.WasPressedThisFrame()) {
          m_desireToJump = true;
          m_jumpCacheTimeCurrent = m_jumpCacheTime;
        }
      }
    }

    public void Die() {
        //m_playerMat.SetFloat(Speed, 30);
        m_isPlayerAlive = false;
        m_wishDir = 0;
    }

    private void FixedUpdate() {
        int groundNormals = 0;
        Vector2 steepestForward = new Vector2(m_wishDir, 0);

        // Normals processing
        {
            var coordY = float.MinValue;
            for (int i = 0; i < m_groundNormals.Count; ++i) {
                if (Vector2.Angle(Vector2.up, m_groundNormals[i]) <= m_groundAngle) {
                    ++groundNormals;
                    var moveVector = m_wishDir > 0
                      ? new Vector2(m_groundNormals[i].y, -m_groundNormals[i].x) // Rotate by 90
                      : new Vector2(-m_groundNormals[i].y, m_groundNormals[i].x); // Rotate by -90
                    if (m_wishDir != 0 && moveVector.y > coordY) {
                        coordY = moveVector.y;
                        steepestForward = moveVector;
                    }
                }
            }
        }
        Debug.DrawRay(m_rigidbody.position, Vector2.up, groundNormals == 0 ? Color.green : Color.red);

        // Movement
        //m_rigidbody.AddForce(steepestForward * m_speed, ForceMode2D.Impulse);
        m_rigidbody.AddForce(m_speed * Mathf.Abs(steepestForward.x) * new Vector2(m_wishDir, 0), ForceMode2D.Impulse);
        m_rigidbody.AddForce(m_rigidbody.linearVelocity.x * m_deceleration * Vector2.left, ForceMode2D.Impulse);

        // Jumping
        if (m_desireToJump && m_jumpCooldownCurrent == 0) {
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);
            if (m_rigidbody.linearVelocity.y > 0f) {
                jumpSpeed = jumpSpeed - m_rigidbody.linearVelocity.y;
            }
            if (!m_firstJumpPerformed && m_coyoteTimeCurrent > 0) {
                m_firstJumpPerformed = true;
                m_coyoteTimeCurrent = 0;
                m_jumpCooldownCurrent = m_jumpCooldown;
                m_desireToJump = false;
                m_rigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            }
            else if (m_jumpsLeft > 0) {
                --m_jumpsLeft;
                m_jumpCooldownCurrent = m_jumpCooldown;
                m_desireToJump = false;
                m_rigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            }
        }

        /*if (m_groundNormals.Count != 0) {
          m_rigidbody.gravityScale = 0;
          m_rigidbody.velocity = Vector2.Lerp(m_rigidbody.velocity, steepestForward * m_speed, m_deceleration);
          if ((m_rigidbody.velocity - steepestForward * m_speed).magnitude < 0.5) {
            m_rigidbody.velocity = steepestForward * m_speed;
          }
        } else {
          m_rigidbody.gravityScale = 1;
        }
        m_rigidbody.velocity = steepestForward * m_speed;*/
        if (m_jumpCooldownCurrent > 0) {
            --m_jumpCooldownCurrent;
        }
        if (groundNormals > 0) {
            if (m_jumpCooldownCurrent == 0) {
                m_firstJumpPerformed = false;
                m_coyoteTimeCurrent = m_coyoteTime;
                m_jumpsLeft = m_additionalJumps;
            }
        }
        else {
            if (m_coyoteTimeCurrent > 0) {
                --m_coyoteTimeCurrent;
            }
        }
        if (m_desireToJump) {
            if (m_jumpCacheTimeCurrent > 0) {
                --m_jumpCacheTimeCurrent;
            }
            else {
                m_desireToJump = false;
            }
        }
        m_groundNormals.Clear();
    }

    private void OnCollisionStay2D(Collision2D collision) {
        var contacts = new List<ContactPoint2D>();
        collision.GetContacts(contacts);

        foreach (var contact in contacts) {
            m_groundNormals.Add(contact.normal);
        }
    }
}
