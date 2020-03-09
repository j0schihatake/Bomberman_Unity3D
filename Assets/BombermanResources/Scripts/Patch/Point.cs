using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Point : MonoBehaviour
{
	//Для 2д это клетка 1x1 (решил отказаться от Block-Point теперь клетка это сразу Point)
	public Transform pointTransform;

	//для работы со словарем:
	public Vector3i pointPosition = Vector3i.zero;
	public Point parent = null;

    public Box box = null;

    // Теперь реализую разрушающийся куб:
    public GameObject thisPointCube = null;
    public Vector3 thisPointCubePosition = Vector3.zero;
    public GameObject destroyCubePrefab = null;

	public bool blocked = false;

    // Эффект взрыва:
    public ParticleSystem exlosive = null;

	public bool DebugCoordinate = false;
	public bool selected = false;

	// Все обьекты находящиеся на этом поинте:
	public List<GameObject> pointObject = new List<GameObject> ();

	public bool point = false;

	public int F = 0;

	// Cтоимость
	public int G = 0;

	// Cуммарная стоимость
	public int H = 0;

	public int myPrice = 0;

    // Подразделяем поинты на типы:
	public typePoint point_type;
	public enum typePoint
	{
        empty,
        blocked,                            // Блокирована разрушемым блоком(у неразрушаемых просто нет поинта)
	}

	void Start ()
	{
		this.gameObject.name = "point = " + this.gameObject.transform.position;
		pointTransform = this.gameObject.transform;
		// Задаем специальный округленный вектор
		pointPosition = Vector3toVector3i (pointTransform.position);
        if (thisPointCube != null) {
            thisPointCubePosition = thisPointCube.gameObject.transform.position;
        }
	}

	/**
     * Функция вычисления манхеттенского расстояния от текущей
     * клетки до finish
     * @param finish конечная клетка
     * @return расстояние
     */
	public float mandist (Point finish)
	{
		return 10f * (Mathf.Abs (this.pointTransform.position.x - finish.pointTransform.position.x)
		+ Mathf.Abs (this.pointTransform.position.z - finish.pointTransform.position.z));
	}

	/**
     * Вычисление стоимости пути до соседней клетки finish
     * @param finish соседняя клетка
     * @return 10, если клетка по горизонтали или вертикали от текущей, 14, если по диагонали
     * (это типа 1 и sqrt(2) ~ 1.44)
     */
	public int price (Point finish)
	{
		if (myPrice == 0) {
			if (Mathf.Approximately (this.pointTransform.position.x, finish.pointTransform.position.x) ||
			    Mathf.Approximately (this.pointTransform.position.y, finish.pointTransform.position.y) ||
			    Mathf.Approximately (this.pointTransform.position.z, finish.pointTransform.position.z)) {
				return 10;
			} else {
				return 30;
			}
		} else {
			return myPrice;
		}
	}

	/**
     * Сравнение клеток
     * @param second вторая клетка
     * @return true, если координаты клеток равны, иначе - false
     */
	public bool equals (Point second)
	{
		return (Mathf.Approximately (this.pointTransform.position.x, second.pointTransform.position.x)) &&
		(Mathf.Approximately (this.pointTransform.position.y, second.pointTransform.position.y)) &&
		(Mathf.Approximately (this.pointTransform.position.z, second.pointTransform.position.z));
	}

	//Метод выполняет преобразование обычного вектора в интовый
	public Vector3i Vector3toVector3i (Vector3 vector)
	{
		Vector3i returned = Vector3i.zero;
		returned.x = Mathf.RoundToInt (vector.x);
		returned.y = Mathf.RoundToInt (vector.y);
		returned.z = Mathf.RoundToInt (vector.z);
		return returned;
	}
}
