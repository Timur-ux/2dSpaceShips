using UnityEngine;

public class TurretDetect : MonoBehaviour {
	TurretController turret_;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {
		turret_ = GetComponentInParent<TurretController>();
	}
	void OnTriggerStay2D(Collider2D other) {
		if(turret_)
			turret_.OnTriggerStay2D_(other);
	}

	void OnTriggerExit2D(Collider2D other) {
		if(turret_)
			turret_.OnTriggerExit2D_(other);
	}

	// Update is called once per frame
	void Update() {

	}
}
