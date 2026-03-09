using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	enum State {
		OnGround,
		InSpace
	};
	bool crouch = false;

  Rigidbody2D rb_;
	Vector2 direction_;
	State state_ = State.OnGround;

  public float moveForce = 10f;
	public float jumpForce = 400f;
	public float maxVelocity = 5f;

	public GameObject downRay;


  void Start() {
    rb_ = GetComponent<Rigidbody2D>();
  }

  public void OnMove(InputAction.CallbackContext context) {
    direction_ = context.ReadValue<Vector2>();
  }

  public void OnFire(InputAction.CallbackContext context) { 
		rb_.AddForce(direction_ * moveForce * 10);
	}

	public void OnJump(InputAction.CallbackContext context) {
		if(state_ != State.OnGround)
			return;

		rb_.AddForce(Vector2.up * jumpForce);
		state_ = State.InSpace;
	}

	public void OnCrouch(InputAction.CallbackContext context) {
		Debug.Log(context.ReadValueAsButton() + " " + crouch);
		bool newCrouchState = context.ReadValueAsButton();
		if(newCrouchState == crouch)
			return;
		crouch = newCrouchState;
		float k;
		if(crouch) {
			transform.Rotate(new Vector3(0, 0, 90));
			k = 0.5f;
		}
		else {
			transform.Rotate(new Vector3(0, 0, -90));
			k = 2.0f;
		}

		jumpForce *= k;
		moveForce *= k;
	}

	void stateUpdateIfNeeded(bool logState = true) {
		RaycastHit2D hitData = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
		if(hitData.collider != null && hitData.distance < transform.localScale.y + 0.02f)
			state_ = State.OnGround;
		else
			state_ = State.InSpace;
		if(logState)
			Debug.Log(state_.ToString() + " " + hitData.distance);
	}

	void OnCollisionEnter2D(Collision2D collisiton) {
		stateUpdateIfNeeded();
	}

	void OnCollisionExit2D(Collision2D collisiton) {
		stateUpdateIfNeeded();
	}

  void Update() {
		if(direction_ != Vector2.zero && rb_.linearVelocity.magnitude < maxVelocity)
			rb_.AddForce(direction_ * moveForce);

		stateUpdateIfNeeded(false);
	}
}
