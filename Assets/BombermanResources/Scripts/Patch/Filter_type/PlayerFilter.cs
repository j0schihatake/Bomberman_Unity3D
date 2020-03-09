using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFilter :  PatchFilter {

	public override void min_searcher(AStar astar){
		astar.min = astar.openList[0];
		int m = astar.openList.Count;
		for (int k = 0; k < m; k++)
		{
			Point point = astar.openList[k];
			// тут я специально тестировал, при < или <= выбираются разные пути,
			// но суммарная стоимость G у них совершенно одинакова. Забавно, но так и должно быть.
			if (point.F <= astar.min.F)
			{
				astar.min = point;
			}
		}

		//b) Помещаем ее в закрытый список. (И удаляем с открытого)
		astar.closedList.Add(astar.min);
		astar.openList.Remove(astar.min);
	}
	
	public override void filter (Point min, AStar astar)
	{
		int otherx = 0;
		int othery = 0;
		int otherz = 0;

		min = astar.min;

		Vector3i vector = Vector3i.zero;

		Point otherPoint;

		//идем от старта:
		//--------------------------------------------------FORWARD		
		otherz = min.pointPosition.z + astar.stepForward;
		//формируем позицию запроса
		vector.x = min.pointPosition.x;
		vector.y = min.pointPosition.y;
		vector.z = otherz;
		//Проверяем содержит ли коллекция поинт с такой позицией(если да то определяем его как поинт впереди)
		if (astar.mapPointDictionary.ContainsKey(vector)) {
			otherPoint = astar.mapPointDictionary[vector];
			if (!astar.closedList.Contains(otherPoint))
			{
				astar.tmpList.Add(otherPoint);
			}
		}

		//---------------------------------------------------BACK	
		otherz = min.pointPosition.z - astar.stepForward;
		//формируем позицию запроса
		vector.x = min.pointPosition.x;
		vector.y = min.pointPosition.y;
		vector.z = otherz;
		//Проверяем содержит ли коллекция поинт с такой позицией(если да то определяем его как поинт впереди)
		if (astar.mapPointDictionary.ContainsKey(vector))
		{
			otherPoint = astar.mapPointDictionary[vector];
			if (!astar.closedList.Contains(otherPoint))
			{
				astar.tmpList.Add(otherPoint);
			}
		}

		//----------------------------------------------------RIGHT
		otherx = min.pointPosition.x + astar.stepRight;
		//формируем позицию запроса
		vector.x = otherx;
		vector.y = min.pointPosition.y;
		vector.z = min.pointPosition.z;
		//Проверяем содержит ли коллекция поинт с такой позицией(если да то определяем его как поинт впереди)
		if (astar.mapPointDictionary.ContainsKey(vector))
		{
			otherPoint = astar.mapPointDictionary[vector];
			if (!astar.closedList.Contains(otherPoint))
			{
				astar.tmpList.Add(otherPoint);
			}
		}

		//-----------------------------------------------------LEFT
		otherx = min.pointPosition.x - astar.stepRight;
		//формируем позицию запроса
		vector.x = otherx;
		vector.y = min.pointPosition.y;
		vector.z = min.pointPosition.z;
		//Проверяем содержит ли коллекция поинт с такой позицией(если да то определяем его как поинт впереди)
		if (astar.mapPointDictionary.ContainsKey(vector))
		{
			otherPoint = astar.mapPointDictionary[vector];
			if (!astar.closedList.Contains(otherPoint))
			{
				astar.tmpList.Add(otherPoint);
			}
		}
	}
}
