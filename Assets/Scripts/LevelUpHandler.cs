using UnityEngine;

public class LevelUpHandler : MonoBehaviour {
	public GameObject levelUpUIElement;
	void Start() {
			if(levelUpUIElement == null) {
				Debug.LogWarning("Level up ui element not set!");
				return;
			}

			levelUpUIElement.SetActive(false);
	}

	public void OnLevelUpHandler() {
		if(levelUpUIElement == null) {
			Debug.LogWarning("Level up ui element not set!");
			return;
		}

		levelUpUIElement.SetActive(true);
	}
}
