using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
	public class GenerationStep : MonoBehaviour {
		private Button attackUp_, speedUp_, healthUp_;

		public GameObject player;
		public GameObject levelUpUIElement;

		private void OnEnable() {
			var uiDoc = GetComponent<UIDocument>();
			attackUp_ = uiDoc.rootVisualElement.Q("AttackUp") as Button;
			attackUp_.RegisterCallback<ClickEvent>(OnAttackUp);
			speedUp_ = uiDoc.rootVisualElement.Q("SpeedUp") as Button;
			speedUp_.RegisterCallback<ClickEvent>(OnSpeedUp);
			healthUp_ = uiDoc.rootVisualElement.Q("HealthUp") as Button;
			healthUp_.RegisterCallback<ClickEvent>(OnHealthUp);
		}

		private void OnDisable() {
			attackUp_.UnregisterCallback<ClickEvent>(OnAttackUp);
			speedUp_.UnregisterCallback<ClickEvent>(OnSpeedUp);
			healthUp_.UnregisterCallback<ClickEvent>(OnHealthUp);
		}

		private void OnPicked() {
			if(levelUpUIElement == null) {
				Debug.LogWarning("Level up ui element not set!");
				return;
			}

			levelUpUIElement.SetActive(false);
		}
		private void OnAttackUp(ClickEvent evt) {
			if(player.TryGetComponent<PlayerController>(out PlayerController controller))
				controller.attackDamage *= 1.3f;
			OnPicked();
		}
		private void OnSpeedUp(ClickEvent evt) {
			if(player.TryGetComponent<PlayerController>(out PlayerController controller)) {
				controller.moveForce *= 1.3f;
				controller.jumpForce *= 1.3f;
				controller.maxVelocity *= 1.3f;
			}
			OnPicked();
		}
		private void OnHealthUp(ClickEvent evt) {
			Debug.Log("Health UP!");
			OnPicked();
		}
	};
}
