using UnityEngine;

public class MoveCamera : MonoBehaviour {

    void Update() {
        if (GameManager.localPlayer == null) return;
        Vector3 pos = GameManager.localPlayer.transform.position;
        pos.y += 0.5f;
        transform.position = pos;
    }

}