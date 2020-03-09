using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ClientPatch : MonoBehaviour
{
// Клиент системы поиска пути позволяет настроить поиск под индивидума:

//----------------------------------------------AIПоиск пути и следование по поинтам----------------------------------
    public ClientPatch myClientPatch = null;

    // Следующая позиция:
    public Vector3 nextPosition = Vector3.zero;
    public Transform targetPosition = null;
    public Vector3 lostPointVector3D = Vector3.zero;
    public Point lostPoints = null;

    // Итоговый список поинтов пути
    public List<Point> patch = new List<Point> ();            
	public int countPoint = 0;

    public AStar aStar = null;
    public bool patch_not_realy = false;                 // Если не существует пути

    // Фильтр пойнтов этого клиента(задает правила отбора подходящих поинтов)
    public PlayerFilter my_patch_Filter = null;

    // Может ли данный клиент проходить через blocked поинты:
    public bool ignore_blocked = false;
}
