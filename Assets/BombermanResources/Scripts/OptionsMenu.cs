using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {

    // Элементы UI куда будут ВЫВОДИТЬСЯ данные:
    public Text map_Info_X = null;
    public Text map_Info_Z = null;
    public Text enemy_In_Map_Count = null;
    public Text level_Time_info = null;

    // Элементы UI откуда будут считываться данные:
    public Text map_X_Info_Input_Field = null;
    public Text map_Z_Info_Input_Field = null;
    public Text enemy_In_Map_Count_Input_Field = null;
    public Text level_Time_Info_Input_Field = null;

    public void onClosed() {
        this.gameObject.SetActive(false);
    }

    public void onOptionsMenu()
    {
        gameObject.SetActive(true);
        // Теперь выводим актуальные на данный момент данные:
        getInfo();
    }

    void getInfo() {
        // обновляем корретность выводимых данных:
        map_Info_X.text = Map.Instance.map_X_Size.ToString();
        map_Info_Z.text = Map.Instance.map_Z_Size.ToString();
        enemy_In_Map_Count.text = Map.Instance.enemy_in_map_count.ToString();
        level_Time_info.text = Map.Instance.level_time.ToString();
    }

    public void setMapX() {
        Map.Instance.map_X_Size = Convert.ToInt32(map_X_Info_Input_Field.text);
        getInfo();
    }

    public void setMapZ() {
        Map.Instance.map_Z_Size = Convert.ToInt32(map_Z_Info_Input_Field.text);
        getInfo();
    }

    public void setEnemyCount() {
        Map.Instance.enemy_in_map_count = Convert.ToInt32(enemy_In_Map_Count_Input_Field.text);
        getInfo();
    }

    public void setLeelTime() {
        Map.Instance.level_time = Convert.ToInt32(level_Time_Info_Input_Field.text);
        getInfo();
    }
}
