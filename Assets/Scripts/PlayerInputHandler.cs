using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] Wizard playerWizard;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButton(0)) {
            playerWizard.CastSpell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void FixedUpdate() {
        Vector2 movement = Vector2.zero;
        bool flipX = playerWizard.GetComponent<SpriteRenderer>().flipX;

        if (Input.GetKey(KeyCode.W)) {
            movement += new Vector2(0, 1);
        }
        if (Input.GetKey(KeyCode.S)) {
            movement += new Vector2(0, -1);
        }
        if (Input.GetKey(KeyCode.A)) {
            flipX = true;
            movement += new Vector2(-1, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            flipX = false;
            movement += new Vector2(1, 0);
        }

        playerWizard.Move(movement, flipX);
    }
}
