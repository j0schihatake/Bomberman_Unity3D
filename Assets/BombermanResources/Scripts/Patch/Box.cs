using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Box : MonoBehaviour
{
    //Дебаг в классе:
    public bool debug = false;

	// Ссылка на префаб такого бокса:
	public GameObject prefab = null;

	// Box это элемент окружния:
	public List<Point> boxpoints = new List<Point> ();

	public typeBox type;
	public enum typeBox
	{
		donDestroy,
		// невозможно здвинуть или уничтожить данный блок
		ground,
		// стандартный блок почва
		water,
		// водяной блок невозможно проехать(без апгрейдов), возможно прострелить(неуничтожим)
	}

	public void setToAStar ()
	{
		for (int i = 0; i < boxpoints.Count; i++) {
			if (!AStar.Instance.mapPointList.Contains (boxpoints [i])) {
				AStar.Instance.mapPointList.Add (boxpoints [i]);
			}
		}
	}

    //-------------------------------------------------Debug----------------------------------------------------------
    public void debug_find_world_Box(string s) {
        if (debug)
        {
            Debug.Log(s);
        }
    }
}
