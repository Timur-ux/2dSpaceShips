using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour {
	enum State { OnGround, InSpace };
	bool crouch = false;

	SpriteRenderer sr_;
	Rigidbody2D rb_;
	Vector2 direction_;
	Vector2 lookDirection_;
	State state_ = State.OnGround;
	Animator[] animators_;

	public float moveForce = 10f;
	public float jumpForce = 400f;
	public float maxVelocity = 5f;

	public Sprite crouchSprite, defaultSprite;

	public GameObject downRay;

	void Start() {
		rb_ = GetComponent<Rigidbody2D>();
		sr_ = GetComponent<SpriteRenderer>();
		var childAnimators = GetComponentsInChildren<Animator>(true);
		animators_ = new Animator[1 + childAnimators.Length];
		animators_[0] = GetComponent<Animator>();
		for(int i = 0; i < childAnimators.Length; ++i)
			animators_[i+1] = childAnimators[i];
	}

	public void OnMove(InputAction.CallbackContext context) {
		direction_ = context.ReadValue<Vector2>();
		float localDirection =
						direction_ == Vector2.zero ? lookDirection_.x : direction_.x;
		lookDirection_ = new Vector2(localDirection, 0);

		Vector2 localScale = transform.localScale;
		localScale.x = lookDirection_.x > 0 ? -1 : 1;

		transform.localScale = localScale;
		bool isWalk = context.ReadValue<Vector2>() != Vector2.zero;
		animators_[0].SetBool("IsWalk", isWalk);
		Debug.Log("IsWalk: " + isWalk);
	}

	public void OnFire(InputAction.CallbackContext context) {
		if (context.ReadValueAsButton())
			foreach(var animator in animators_)
				animator.SetTrigger("Attack");
	}

	public void OnJump(InputAction.CallbackContext context) {
		if (state_ != State.OnGround || !context.ReadValueAsButton())
			return;

		rb_.AddForce(Vector2.up * jumpForce);
		state_ = State.InSpace;
	}

	public void OnCrouch(InputAction.CallbackContext context) {
		bool newCrouchState = context.ReadValueAsButton();
		if (!newCrouchState)
			return;
		newCrouchState = !crouch;
		RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, 1f,
																								 ~LayerMask.GetMask("Player"));
		if (!newCrouchState && hitUp.collider != null)
			return;

		crouch = newCrouchState;
		animators_[0].SetBool("IsCrouch", newCrouchState);

		float k = 0.7f;
		if (!crouch)
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
