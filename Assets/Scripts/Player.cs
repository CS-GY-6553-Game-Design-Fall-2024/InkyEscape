using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
  public static Player current;
  private float timer = 0f;
  public AudioSource shootingSound;
  // For Flash();
  
  private Color originalColor;
  [Header("=== References ===")]
  [SerializeField] private Rigidbody2D m_rb;
  [SerializeField] private SpriteRenderer m_spriteRenderer;
  [SerializeField] private Collider2D m_sideScrollCollider;
  [SerializeField] private Collider2D m_topDownCollider;
  [SerializeField] private Collider2D m_bottomDetector;
  [SerializeField] private CompositeCollider2D m_paintTilemapCollider;
  private Tilemap m_paintTilemap;
  [SerializeField] private Transform m_eyeParent;

  [Header("=== Movement Settings ===")]
  [SerializeField] private bool m_isTopDown = false;
  public bool isTopDown => m_isTopDown;
  [SerializeField] private float m_sideScrollMovementSpeed = 1f;
  [SerializeField] private float m_topDownMovementSpeed = 1.5f;
  [SerializeField] private KeyCode m_jumpButton = KeyCode.Space;
  [SerializeField] private KeyCode m_topDownButton = KeyCode.LeftShift;
  [SerializeField] private float m_jumpSpeed = 15f;
  [SerializeField] private bool m_useCoroutineTransition = true;
  [SerializeField] private float m_transitionDuration = 0.15f;
  private IEnumerator m_transitionCoroutine = null;

  [Header("=== Shooting ===")]
  [SerializeField] private Bullet m_bulletPrefab;
  [SerializeField] private bool m_onlyOnMouseDown = true;
  [SerializeField] private float m_playerBulletSpeed = 30f;
  [SerializeField] private float m_playerBulletCooldown = 0.1f;
  [SerializeField] private int m_mouseDownBulletCost = 10;
  [SerializeField] private int m_mouseBulletCost = 4;

  [Header("=== Outputs - READ ONLY ===")]
  [SerializeField] private Vector3 m_movement = Vector3.zero;
  [SerializeField] private bool m_isOnInk = false;
  [SerializeField] private bool m_isGrounded = true;
  [SerializeField] private bool m_isJumping = false;
  [SerializeField] private float m_bulletLastShot = 0f;

  [Header("=== Player Resources ===")]
  //[SerializeField] private int MAX_HP = 100;
  //[SerializeField] private int CUR_HP = 100;
  [SerializeField] private int max_ink = 100;
  [SerializeField] private int cur_ink = 100;

  [Header("=== Energy Bar ===")]
  [SerializeField] private Image inkBarFill;

  private void Awake()
  {
    current = this;
  }

  private void Start()
  {
    m_paintTilemap = m_paintTilemapCollider.gameObject.GetComponent<Tilemap>();
    ToggleTopDown(false);
    // For Flash()
    originalColor = m_spriteRenderer.color;
    UpdateInkBar();
  }

  private void Update()
  {
    // Confirm if our player is currently above an inked wall.
    m_isOnInk = TilemapPainter.current.CheckIsInk(transform.position);

    // If the player pressed down on the button to toggle top down mode, we do all that logic here
    if (Input.GetKeyDown(m_topDownButton) && m_isOnInk && !m_isTopDown)
    {
      SetTopDown(true);
    }
    if (Input.GetKeyUp(m_topDownButton) && m_isTopDown)
    {
      SetTopDown(false);
    }
    
    // Depending on if the player is in top-down mode, some functions are availabe. Likewise, some functions are available only in side-scroll mode
    if (m_isTopDown) {
      if (cur_ink < max_ink) {
        cur_ink += TilemapPainter.current.GetInkCapacityFromTile(m_topDownCollider.bounds);
      }
      cur_ink = Mathf.Clamp(cur_ink, 0, max_ink);
    }
    else {
      // Check the input for shooting. If held down, we shoot as long as we are allowed to, as dicated by `m_bulletShot`
      if (((m_onlyOnMouseDown && Input.GetMouseButtonDown(0)) || !m_onlyOnMouseDown && Input.GetMouseButton(0)) && Time.time - m_bulletLastShot >= m_playerBulletCooldown) {
        Shoot();
      }
    }
    
    /*
    // *IF pressing R, ink + 10
    if (Input.GetKeyDown(KeyCode.R))
    {
      Refill();
    }
    // *IF pressing F, REFILL ink to 100/100
    if (Input.GetKeyDown(KeyCode.F))
    {
      RefillFully();
    }
    */

    float xInput = Input.GetAxisRaw("Horizontal");
    float yInput = m_isTopDown
        ? Input.GetAxisRaw("Vertical")
        : 0f;
    float jumpInput = !m_isTopDown && Input.GetKey(m_jumpButton)
        ? 1f
        : 0f;
    m_movement = new Vector3(xInput, yInput, jumpInput);

    // Move eyes
    m_eyeParent.localPosition = new Vector3(xInput/10f, yInput/10f, 1f);

    UpdateInkBar();
  }

  private void FixedUpdate()
  {
    MoveCharacter(m_movement);
  }

  // direction.x & direction.y = horizontal and vertical movement
  // direction.z = jump
  public void MoveCharacter(Vector3 direction)
  {
    float movementSpeed = m_isTopDown ? m_topDownMovementSpeed : m_sideScrollMovementSpeed;
    if (Mathf.Abs(direction.x) > 0.1f)
    {
      m_rb.AddForce(new Vector2(direction.x * movementSpeed, 0f), ForceMode2D.Impulse);
    }
    if (Mathf.Abs(direction.y) > 0.1f)
    {
      m_rb.AddForce(new Vector2(0f, direction.y * movementSpeed), ForceMode2D.Impulse);
    }

    if (m_isGrounded && !m_isJumping && direction.z > 0.1f)
    {
      m_rb.velocity = new Vector2(m_rb.velocity.x, 0f);
      m_rb.AddForce(new Vector2(0f, direction.z * m_jumpSpeed), ForceMode2D.Impulse);
      m_isJumping = true;
    }
  }

  public void CheckOnGround(Collider2D detector, Collider2D other)
  {
    if (other.gameObject.tag == "Environment")
    {
      m_isGrounded = true;
      m_isJumping = false;
    }
  }

  public void CheckOffGround(Collider2D detector, Collider2D other)
  {
    if (other.gameObject.tag == "Environment")
    {
      m_isGrounded = false;
      m_isJumping = true;
    }
  }

  public void ToggleTopDown(bool useCoroutine = true)
  {
    m_sideScrollCollider.isTrigger = m_isTopDown;
    m_topDownCollider.isTrigger = !m_isTopDown;
    m_paintTilemapCollider.isTrigger = !m_isTopDown;
    m_rb.gravityScale = (!m_isTopDown) ? 7.5f : 0f;
    if (m_transitionCoroutine != null) StopCoroutine(m_transitionCoroutine);
    Vector3 targetSize = m_isTopDown ? new Vector3(0.1f, 0.1f, 1f) : Vector3.one;
    if (useCoroutine) {
      Vector3 originalSize = m_isTopDown ? Vector3.one : new Vector3(0.1f, 0.1f, 1f);
      m_transitionCoroutine = TransformSpriteRenderer(targetSize, originalSize);
      StartCoroutine(m_transitionCoroutine);
    } else {
      m_spriteRenderer.transform.localScale = targetSize;
    }
   
  }
  public void SetTopDown(bool setTo)
  {
    m_isTopDown = setTo;
    ToggleTopDown(m_useCoroutineTransition);
  }
  private IEnumerator TransformSpriteRenderer(Vector3 targetSize, Vector3 originalSize) {
    float startTime = Time.time;
    float transitionRatio = 0f;
    while(transitionRatio < 1f) {
      transitionRatio = (Time.time - startTime)/m_transitionDuration;
      Vector3 curSize = Vector3.Lerp(originalSize, targetSize, transitionRatio);
      m_spriteRenderer.transform.localScale = curSize;
      yield return null;
    }
    m_spriteRenderer.transform.localScale = targetSize;
  }

  public void Shoot()
  {
    // Change cur ink if successfully firing a bullet
    if (cur_ink <= 0)
    {
      StartCoroutine(Flash());
      print("Shooting() called. But no ink!");
      return;
    }
    cur_ink -= m_onlyOnMouseDown ? m_mouseDownBulletCost : m_mouseBulletCost;
    shootingSound.Play();

    //print("Fire!");
    showCurInk();
    // Get bullet direction according to mouse position and player position
    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Convert mouse position to world position
    mousePos.z = 0f;

    // Determine the direction the bullet should move in
    Vector3 shootDirection = (mousePos - this.transform.position).normalized;

    // Generating bullet with appropriate velocity
    Bullet projectile = Instantiate(m_bulletPrefab, transform.position, Quaternion.identity) as Bullet;
    projectile.Shoot(shootDirection, m_playerBulletSpeed, mousePos);

    // Set the time last shot
    m_bulletLastShot = Time.time;
  }

  public void Refill(int amount=10) {
    if (cur_ink >= max_ink) {
      cur_ink = max_ink;
      print("Refill() called. Ink is full!");
      return;
    }

    cur_ink = Mathf.Clamp(cur_ink + amount, 0, max_ink);
    showCurInk();
  }

  void UpdateInkBar()
  {
      inkBarFill.fillAmount = (float)cur_ink / max_ink;
  }

  public void Defill(int amount=10) {
    if (cur_ink <= 0) {
      cur_ink = 0;
      UpdateInkBar();
      print("Defill() called. Ink is empty");
      return;
    }

    cur_ink = Mathf.Clamp(cur_ink - amount, 0, max_ink);
    showCurInk();
  }

  public void RefillFully()
  {

    if (!checkInkIsFull())
    {
      cur_ink = max_ink;
    }
    else
    {
      print("RefillFully() called. Ink is full!");
    }
    showCurInk();
  }
  public void RefillFully(Collider2D trigger, Collider2D other) {
    if (other.gameObject != this.gameObject) return;
    RefillFully();
  }

  /* Effects */
  // Flash when trying to shoot without ink
  IEnumerator Flash()
  {
    m_spriteRenderer.color = Color.yellow;
    yield return new WaitForSeconds(0.2f);
    m_spriteRenderer.color = originalColor;
  }

  /* Tool Functions */
  bool checkInkIsFull()
  {
    return cur_ink == max_ink ? true : false;
  }

  bool checkInkIsEmpty()
  {
    return cur_ink <= 0 ? true : false;
  }

  /* Used to Debug */
  void showCurInk()
  {
    Debug.Log("Current Ink: " + cur_ink.ToString());
  }

  void print(string str)
  {
    Debug.Log(str);
  }
}
