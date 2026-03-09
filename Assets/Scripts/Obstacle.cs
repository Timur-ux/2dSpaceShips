using UnityEngine;

public class Obstacle : MonoBehaviour {
  public float minSize = 0.5f, maxSize = 2.0f;
  public float minSpeed = 50f, maxSpeed = 150f;
	public float spinRange = 10f;
  Rigidbody2D rb_;

  void Start() {
    float scale = Random.Range(minSize, maxSize);
    transform.localScale = new Vector3(scale, scale, 1);

    Vector2 speed = Random.insideUnitCircle * Random.Range(minSpeed, maxSpeed) / scale;
		float spin = Random.Range(-spinRange, spinRange) / scale;

    rb_ = GetComponent<Rigidbody2D>();
    rb_.AddForce(speed);
		rb_.AddTorque(spin);
  }

  void Update() {}
}
