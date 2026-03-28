using UnityEngine;

public class Camera : MonoBehaviour {

	public GameObject target;
	void Start() { }

	void Update() {
		Vector3 tPos = target.transform.localPosition;
		tPos.z = transform.localPosition.z;
		transform.localPosition = tPos;
	}
}
