using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour {
	enum State { OnGround, InSpace };
	bool crouch = false;

	Rigidbody2D rb_;
	CapsuleCollider2D collider_;
	Vector2 direction_;
	Vector2 lookDirection_;
	State state_ = State.OnGround;
	Animator[] animators_;

	public float moveForce = 400f;
	public float jumpForce = 2000f;
	public float maxVelocity = 5f;
	public float attackDamage = 50;
	public float attackForce = 5;

	public float crouchColliderOffset = -0.5f;
	public float jumpColliderOffset = 0.1f;
	public float standColliderOffset = -0.25f;

	public Sprite crouchSprite, defaultSprite;

	public GameObject downRay;

	void Start() {
		rb_ = GetComponent<Rigidbody2D>();
		collider_ = GetComponent<CapsuleCollider2D>();
		var childAnimators = GetComponentsInChildren<Animator>(true);
		animators_ = new Animator[1 + childAnimators.Length];
		animators_[0] = GetComponent<Animator>();
		for (int i = 0; i < childAnimators.Length; ++i)
			animators_[i + 1] = childAnimators[i];
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
		if (context.ReadValueAsButton())
			foreach (var animator in animators_)
				animator.SetTrigger("Attack");
		attackTime_ = attackSpan_;
	}

	public void OnJump(InputAction.CallbackContext context) {
		if (state_ != State.OnGround || !context.ReadValueAsButton())
			return;

		rb_.AddForce(Vector2.up * jumpForce);
		state_ = State.InSpace;
		collider_.offset = new Vector2(collider_.offset.x, jumpColliderOffset);
		animators_[0].SetTrigger("Jump");
		animators_[0].SetBool("InSpace", true);
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
		float oldColliderY = collider_.size.y;
		if(crouch) {
			collider_.size = new Vector2(collider_.size.x, oldColliderY * k);
			collider_.offset = new Vector2(collider_.offset.x, crouchColliderOffset);
		}
		else {
			collider_.size = new Vector2(collider_.size.x, oldColliderY / k);
			collider_.offset = new Vector2(collider_.offset.x, standColliderOffset);
		}

		if (!crouch)
			k = 1 / k;
		jumpForce *= k;
		moveForce *= k;
		maxVelocity *= k;
	}

	void stateUpdateIfNeeded() {
		State oldState = state_;
		RaycastHit2D hitData =
						Physics2D.Raycast(downRay.transform.position, Vector2.down, Mathf.Infinity, ~LayerMask.GetMask("Player"));
		if (hitData.collider != null && hitData.distance < 0.2f) {
			state_ = State.OnGround;
			collider_.offset = new Vector2(collider_.offset.x, standColliderOffset);
		}
		else {
			state_ = State.InSpace;
			if(!crouch)
				collider_.offset = new Vector2(collider_.offset.x, jumpColliderOffset);
		}

		animators_[0].SetBool("InSpace", state_ == State.InSpace);
		animators_[0].SetBool("IsWalk", (direction_ != Vector2.zero) && (state_ == State.OnGround));
	}

	void OnCollisionEnter2D(Collision2D collisiton) { stateUpdateIfNeeded(); }

	void OnCollisionExit2D(Collision2D collisiton) { stateUpdateIfNeeded(); }

	void Update() {
		if (direction_ != Vector2.zero &&
						rb_.linearVelocity.magnitude < maxVelocity)
			rb_.AddForce(direction_ * moveForce);
		if (state_ == State.InSpace && direction_ != Vector2.zero && rb_.linearVelocity == Vector2.zero)
			rb_.AddForce((direction_ + Vector2.up) * moveForce);

		if(attackTime_ > 0)
			attackTime_ -= Time.deltaTime;
		stateUpdateIfNeeded();
	}

	private string EnemyTag_ = "New tag"; // I can't set name to enemy tag other than default
	private List<EntityId> hittedEntities_;
	private float attackTime_ = 0;
	private float attackSpan_ = 1;

	void OnTriggerEnter2D(Collider2D other) {
		if(!other.CompareTag(EnemyTag_) || attackTime_ <= 0)
			return;
		EntityId otherId = other.GetEntityId();
		foreach(var id in hittedEntities_)
			if(id == otherId)
				return;

		if(other.TryGetComponent<TurretController>(out TurretController turret)) {
			turret.TakeDamage(attackDamage);
			hittedEntities_.Add(otherId);
		}
		if(other.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
			rb.AddForce(direction_ * attackForce);
	}
}
