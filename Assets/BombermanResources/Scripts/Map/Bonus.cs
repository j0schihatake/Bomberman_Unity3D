using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour {

    public GameObject bonusPrefab = null;

    // Разделяем бонусы по типу:
    public bonus_type type;
    public enum bonus_type {
        speed,
        armor,
        bomb_power,
        bomb_count,
    }

    void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.tag.Equals("player")) {
            switch (type) {
                case bonus_type.armor:
                    Map.Instance.player.armor_Bonus();
                    break;
                case bonus_type.bomb_count:
                    Map.Instance.player.bombLimit = Map.Instance.player.bombLimit + 1;
                    break;
                case bonus_type.bomb_power:
                    Map.Instance.player.powerPomb = Map.Instance.player.powerPomb + 1;
                    break;
                case bonus_type.speed:
                    Map.Instance.player.speed = Map.Instance.player.speed + 0.5f;
                    break;
            }
            Destroy(this.gameObject);
        }
    }
}
