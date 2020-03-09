using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Выносим фильтры поиска пти во вне
public abstract class PatchFilter : MonoBehaviour {

	public abstract void min_searcher (AStar astar);

	public abstract void filter (Point min, AStar astar);
		/*
		int otherx = 0;
		int othery = 0;
		int otherz = 0;

		min = astar.min;

		int iteration = astar.iterations;
		int i = 0;

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

		if (astar.find != AStar.typeFind.xz) {
			//-----------------------------------------------------Up
			othery = min.pointPosition.y + astar.stepUp;
			//формируем позицию запроса
			vector.x = min.pointPosition.x;
			vector.y = othery;
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

			//-----------------------------------------------------Down
			othery = min.pointPosition.y - astar.stepUp;
			//формируем позицию запроса
			vector.x = min.pointPosition.x;
			vector.y = othery;
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
	*/
}
