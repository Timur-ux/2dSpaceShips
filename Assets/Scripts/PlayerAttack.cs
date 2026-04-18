using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
		PlayerController player_;
		BoxCollider2D trigger_;

    void Start()
    {
			player_ = GetComponentInParent<PlayerController>();
			trigger_ = GetComponent<BoxCollider2D>();
    }

		void OnTriggerEnter2D(Collider2D other) {
			if(player_)
				player_.OnTriggerEnter2D_(other);
		}

		public void Enable() {
			trigger_.enabled = true;
		}

		public void Disable() {
			trigger_.enabled = false;
		}


    // Update is called once per frame
    void Update()
    {
        
    }
}
