using UnityEngine;

public class TurretController : MonoBehaviour {
	Animator animator_;
	Rigidbody2D rb_;
	private enum Direction {
		Left, Right
	};

	Direction dir_ = Direction.Right;
	SpriteRenderer sprite_;
	private float damagedDelay = 0;
	public float damagedSpan_ = 1;

	public float health = 100;
	public void TakeDamage(float amount) {
		damagedDelay = damagedSpan_;
		sprite_.color = Color.red;
		health -= amount;
		if(health <= 0)
			Destroy(this, 0.5f);
	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {
		animator_ = GetComponent<Animator>();
		rb_ = GetComponent<Rigidbody2D>();
	}

	void OnTriggerEnter2D(Collider2D other) {
	}

	void OnTriggerStay2D(Collider2D other) {
		if (!other.CompareTag("Player"))
			return;
		Vector2 dist = (other.transform.position - transform.position);
		RaycastHit2D hitData = Physics2D.Raycast(transform.position, dist.normalized, dist.magnitude * 2, ~LayerMask.GetMask("Enemy", "PlayerAttack"));
		bool playerVisible = false;
		if (hitData.collider != null && hitData.collider == other)
			playerVisible = true;

		bool isOpen = animator_.GetBool("IsOpen");
		if (isOpen && !playerVisible) {
			animator_.SetTrigger("EnemyNotInRange");
			animator_.SetBool("IsOpen", false);
		}

		if (!isOpen && playerVisible) {
			animator_.SetTrigger("EnemyInRange");
			animator_.SetBool("IsOpen", true);
		}

		Direction currentDirection;
		if (other.transform.localPosition.x > transform.position.x)
			currentDirection = Direction.Right;
		else
			currentDirection = Direction.Left;
		if (currentDirection != dir_) {
			Vector3 scale = transform.localScale;
			transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
			dir_ = currentDirection;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (!other.CompareTag("Player"))
			return;
		if (animator_.GetBool("IsOpen")) {
			animator_.SetTrigger("EnemyNotInRange");
			animator_.SetBool("IsOpen", false);
		}
		Debug.Log("Player undetected");
	}


	// Update is called once per frame
	void Update() {
		if(damagedDelay > 0) {
			damagedDelay -= Time.deltaTime;
			if(damagedDelay <= 0)
				sprite_.color = Color.clear;
		}
	}
}
