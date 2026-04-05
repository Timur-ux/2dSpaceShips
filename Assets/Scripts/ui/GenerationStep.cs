using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
public class GenerationStep : MonoBehaviour {
	private Button button_;
	public WaveFunctionCollapse.TileMap mapGenerator_;
	private void OnEnable() {
		var uiDoc = GetComponent<UIDocument>();
		button_ = uiDoc.rootVisualElement.Q("GenerationStep") as Button;
		button_.RegisterCallback<ClickEvent>(OnClick);
	}

	private void OnDisable() {
		button_.UnregisterCallback<ClickEvent>(OnClick);
	}

	private void OnClick(ClickEvent evt) {
		if(mapGenerator_ != null) {
			Debug.Log("Starting generation");
			for(int i = 0; i < 10; ++i) 
				mapGenerator_.GenerationStep();
		}
	}
};
}
