using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishBox : MonoBehaviour {

    // Как только игрок "заходит" на Box финиша игра выйграна:
    void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.tag.Equals("player")) {
            //Включаем победу игрока:
            Map.Instance.player.state = PlayerClient.playerState.win;
        }
    }
}
