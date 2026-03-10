using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
  enum State { OnGround, InSpace }
  ;
  bool crouch = false;

  SpriteRenderer sr_;
  Rigidbody2D rb_;
  Vector2 direction_;
  Vector2 lookDirection_;
  State state_ = State.OnGround;

  public float moveForce = 10f;
  public float jumpForce = 400f;
  public float maxVelocity = 5f;

  public Sprite crouchSprite, defaultSprite;

  public GameObject downRay;

  void Start() {
    rb_ = GetComponent<Rigidbody2D>();
    sr_ = GetComponent<SpriteRenderer>();
  }

  public void OnMove(InputAction.CallbackContext context) {
    direction_ = context.ReadValue<Vector2>();
    float localDirection =
        direction_ == Vector2.zero ? lookDirection_.x : direction_.x;
    lookDirection_ = new Vector2(localDirection, 0);

    Vector2 localScale = transform.localScale;
    localScale.x = lookDirection_.x > 0 ? -1 : 1;

    transform.localScale = localScale;
  }

  public void OnFire(InputAction.CallbackContext context) {
    rb_.AddForce(direction_ * moveForce * 10);
  }

  public void OnJump(InputAction.CallbackContext context) {
    if (state_ != State.OnGround || ! context.ReadValueAsButton())
      return;

    rb_.AddForce(Vector2.up * jumpForce);
    state_ = State.InSpace;
  }

  public void OnCrouch(InputAction.CallbackContext context) {
    bool newCrouchState = context.ReadValueAsButton();
		if(!newCrouchState)
			return;
		newCrouchState = !crouch;
    if (!newCrouchState &&
        (Physics2D.Raycast(transform.position, Vector2.up, 1f,
                           ~LayerMask.GetMask("Player")))
                .collider != null)
      return;

    crouch = newCrouchState;
    float yScale = 1;
    if (crouch) {
      sr_.sprite = crouchSprite;
      yScale = 0.6f;
    } else 
      sr_.sprite = defaultSprite;
    

    Vector3 localScale = transform.localScale;
    localScale.y = yScale;
    transform.localScale = localScale;

		float k = 0.7f;
		if(!crouch)
			k = 1 / k;
    jumpForce *= k;
    moveForce *= k;
		maxVelocity *= k;
  }

  void stateUpdateIfNeeded() {
    RaycastHit2D hitData =
        Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity,
                          LayerMask.GetMask("Ground"));
    if (hitData.collider != null && hitData.distance < 1.25f)
      state_ = State.OnGround;
    else
      state_ = State.InSpace;
  }

  void OnCollisionEnter2D(Collision2D collisiton) { stateUpdateIfNeeded(); }

  void OnCollisionExit2D(Collision2D collisiton) { stateUpdateIfNeeded(); }

  void Update() {
    if (direction_ != Vector2.zero &&
        rb_.linearVelocity.magnitude < maxVelocity)
      rb_.AddForce(direction_ * moveForce);

    stateUpdateIfNeeded();
  }
}
