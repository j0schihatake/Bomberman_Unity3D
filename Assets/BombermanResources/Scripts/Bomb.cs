using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {
    // Bomb это заряд игрока:
    // Правила:
    // 1. Дистанция взрыва на пустом пространстве может увеличиваться во всех направлениях
    // 2. Заряд может разнести лишь один Box в одном нарпавлении

    // Сила взрыва в поинтах:
    public int power_strenght = 1;

    // Дистанция взрывной волны:
    public float damage_distance = 0.5f;

    public string type_animation = "bomb_destroy";

    public Point bombPoint = null;

    // Время до взрыва:
    public float damage_time = 10f;

    private AStar astar = null;
    private Map map = null;

    public Vector3i center_position = Vector3i.zero;
    public Vector3i forward_position = Vector3i.zero;
    public Vector3i back_position = Vector3i.zero;
    public Vector3i left_position = Vector3i.zero;
    public Vector3i right_position = Vector3i.zero;

    // Префаб взрыва:
    public GameObject explosive = null;
    public GameObject clone_explosive = null;

    // Список бонусов на уровне:
    public List<Bonus> allBonusPrefab = new List<Bonus>();
    public float bonus_Chance = 0.3f;

    void Start() {
        astar = AStar.Instance;
        map = Map.Instance;
    }

    void Update() {
        boom();
    }

    // Метод выполняет взрыв:
    public void boom() {
        /*
         * Итак необходимо уничтожить данный обьект и создать(взрыв)
         */

        //Ведем обратный отсчет:
        if (damage_time > 0)
        {
            damage_time -= 0.01f;
        }
        else {
            damage_time = 0f;
        }
        if (damage_time == 0) {
            
            // Формируем позицию бомбы:
            center_position = astar.Vector3toVector3i(this.gameObject.transform.position);
            //Debug.Log("Bomb = " + center_position);

            // Формируем позицию впереди:
            forward_position = astar.Vector3toVector3i(this.gameObject.transform.position);
            forward_position.z = forward_position.z + astar.stepForward;
            //Debug.Log("Forward = " + forward_position);

            // Формируем позицию позади бомбы:
            back_position = astar.Vector3toVector3i(this.gameObject.transform.position);
            back_position.z = back_position.z - astar.stepForward;
            //Debug.Log("Back = " + back_position);

            //Формируем позицию слева от бомбы:
            left_position = astar.Vector3toVector3i(this.gameObject.transform.position);
            left_position.x = left_position.x - astar.stepRight;
            //Debug.Log("Left = " + left_position);

            //Формируем позицию справа от бомбы:
            right_position = astar.Vector3toVector3i(this.gameObject.transform.position);
            right_position.x = right_position.x + astar.stepRight;
            //Debug.Log("Right = " + right_position);

            bool forward_destroy = false;
            bool left_destroy = false;
            bool right_destroy = false;
            bool back_destroy = false;

            Point forwardPoint = null;
            Point backPoint = null;
            Point leftPoint = null;
            Point rightPoint = null;
            Point centerPoint = null;

            //Место для спавна префаба разрушенного куба:
            Vector3 cube_position = Vector3.zero;

            //Переменная для определения дистанции от взрыва:
            float distance_to_explosive = 0f;

            // Теперь создаем взрывы на каждом из 4-х направлении:
            for (int i = 0; i < power_strenght; i++)
            {
                // Теперь шагаем во все стороны(если есть поинты или пока не уткнулись и не разрушили первый Box):
                if (!forward_destroy) {

                    forward_position.z = forward_position.z + (astar.stepForward * i);
                    if (astar.mapPointDictionary.ContainsKey(forward_position)) {
                        forwardPoint = astar.mapPointDictionary[forward_position];
                    }
                    // Проверяем наличие поинта в нужном направлении:
                    if (forwardPoint != null) {
                        switch (forwardPoint.point_type) {
                            case Point.typePoint.empty:
                                clone_explosive = (GameObject)Instantiate(explosive, forward_position, Quaternion.identity);
                                // Теперь уничтожаем игрока или противников расположенных на опасной дистанции ко взрыву:
                                distance_to_explosive = Vector3.Distance(forwardPoint.transform.position, map.player.gameObject.transform.position);
                                if (distance_to_explosive <= damage_distance) {
                                    // О нет игрока задело и убивает:
                                    map.player.destroy_anim = this.type_animation;
                                    map.player.state = PlayerClient.playerState.destroy;
                                }
                                // Теперь выполняем такуюже проверку для врагов(при большом количестве врагов такой подход даст просадку производительности, но это уже другая история :) ):
                                for (int k = 0; k < map.enemyInMapList.Count; k++) {
                                    BotClient nextBot = map.enemyInMapList[k];
                                    distance_to_explosive = Vector3.Distance(forwardPoint.transform.position, nextBot.gameObject.transform.position);
                                    if (distance_to_explosive <= damage_distance)
                                    {
                                        // О нет игрока задело и убивает:
                                        nextBot.destroy_anim = this.type_animation;
                                        nextBot.state = BotClient.bot_state.destroy;
                                    }
                                }
                                forwardPoint = null;
                                break;
                            case Point.typePoint.blocked:
                                // Значит необходимо разрушить Box и освободить проход:
                                forwardPoint.point_type = Point.typePoint.empty;
                                // Почемуто решил сделать так:
                                cube_position.x = forwardPoint.thisPointCube.transform.position.x;
                                cube_position.y = forwardPoint.thisPointCube.transform.position.y;
                                cube_position.z = forwardPoint.thisPointCube.transform.position.z;
                                // Уничтожаем нормальный куб:
                                Destroy(forwardPoint.thisPointCube,0f);
                                Instantiate(forwardPoint.destroyCubePrefab, cube_position, Quaternion.identity);
                                createBonus(forwardPoint.pointPosition);
                                forwardPoint = null;
                                forward_destroy = true;
                                break;
                        }
                    }
                }
                if (!back_destroy) {
                    back_position.z = back_position.z - (astar.stepForward * i);
                    if (astar.mapPointDictionary.ContainsKey(back_position)) {
                        backPoint = astar.mapPointDictionary[back_position];
                    }
                    // Проверяем наличие поинта в нужном направлении:
                    if (backPoint != null)
                    {
                        switch (backPoint.point_type) {
                            case Point.typePoint.empty:
                                clone_explosive = (GameObject)Instantiate(explosive, back_position, Quaternion.identity);
                                // Теперь уничтожаем игрока или противников расположенных на опасной дистанции ко взрыву:
                                distance_to_explosive = Vector3.Distance(backPoint.transform.position, map.player.gameObject.transform.position);
                                if (distance_to_explosive <= damage_distance)
                                {
                                    // О нет игрока задело и убивает:
                                    map.player.destroy_anim = this.type_animation;
                                    map.player.state = PlayerClient.playerState.destroy;
                                }
                                // Теперь выполняем такуюже проверку для врагов(при большом количестве врагов такой подход даст просадку производительности, но это уже другая история :) ):
                                for (int k = 0; k < map.enemyInMapList.Count; k++)
                                {
                                    BotClient nextBot = map.enemyInMapList[k];
                                    distance_to_explosive = Vector3.Distance(backPoint.transform.position, nextBot.gameObject.transform.position);
                                    if (distance_to_explosive <= damage_distance)
                                    {
                                        // О нет игрока задело и убивает:
                                        nextBot.destroy_anim = this.type_animation;
                                        nextBot.state = BotClient.bot_state.destroy;
                                    }
                                }
                                backPoint = null;
                                break;
                            case Point.typePoint.blocked:
                                // Значит необходимо разрушить Box и освободить проход:
                                backPoint.point_type = Point.typePoint.empty;
                                // Почемуто решил сделать так:
                                cube_position.x = backPoint.thisPointCubePosition.x;
                                cube_position.y = backPoint.thisPointCubePosition.y;
                                cube_position.z = backPoint.thisPointCubePosition.z;
                                // Уничтожаем нормальный куб:
                                Destroy(backPoint.thisPointCube, 0f);
                                Instantiate(backPoint.destroyCubePrefab, cube_position, Quaternion.identity);
                                createBonus(backPoint.pointPosition);
                                backPoint = null;
                                back_destroy = true;
                                break;
                        }
                    } 
                }
                if (!left_destroy)
                {
                    left_position.x = left_position.x - (astar.stepRight * i);
                    if (astar.mapPointDictionary.ContainsKey(left_position)) {
                        leftPoint = astar.mapPointDictionary[left_position];
                    }
                    // Проверяем наличие поинта в нужном направлении:
                    if (leftPoint != null)
                    {
                        switch (leftPoint.point_type) {
                            case Point.typePoint.empty:
                                clone_explosive = (GameObject)Instantiate(explosive, left_position, Quaternion.identity);
                                // Теперь уничтожаем игрока или противников расположенных на опасной дистанции ко взрыву:
                                distance_to_explosive = Vector3.Distance(leftPoint.transform.position, map.player.gameObject.transform.position);
                                if (distance_to_explosive <= damage_distance)
                                {
                                    // О нет игрока задело и убивает:
                                    map.player.destroy_anim = this.type_animation;
                                    map.player.state = PlayerClient.playerState.destroy;
                                }
                                // Теперь выполняем такуюже проверку для врагов(при большом количестве врагов такой подход даст просадку производительности, но это уже другая история :) ):
                                for (int k = 0; k < map.enemyInMapList.Count; k++)
                                {
                                    BotClient nextBot = map.enemyInMapList[k];
                                    distance_to_explosive = Vector3.Distance(leftPoint.transform.position, nextBot.gameObject.transform.position);
                                    if (distance_to_explosive <= damage_distance)
                                    {
                                        // О нет игрока задело и убивает:
                                        nextBot.destroy_anim = this.type_animation;
                                        nextBot.state = BotClient.bot_state.destroy;
                                    }
                                }
                                leftPoint = null;
                                break;
                            case Point.typePoint.blocked:
                                // Значит необходимо разрушить Box и освободить проход:
                                leftPoint.point_type = Point.typePoint.empty;
                                // Почемуто решил сделать так:
                                cube_position.x = leftPoint.thisPointCubePosition.x;
                                cube_position.y = leftPoint.thisPointCubePosition.y;
                                cube_position.z = leftPoint.thisPointCubePosition.z;
                                // Уничтожаем нормальный куб:
                                Destroy(leftPoint.thisPointCube, 0f);
                                Instantiate(leftPoint.destroyCubePrefab, cube_position, Quaternion.identity);
                                createBonus(leftPoint.pointPosition);
                                leftPoint = null;
                                left_destroy = true;
                                break;
                        }
                    }
                }
                if (!right_destroy)
                {
                    right_position.x = right_position.x + (astar.stepRight * i);
                    if (astar.mapPointDictionary.ContainsKey(right_position)) {
                        rightPoint = astar.mapPointDictionary[right_position];
                    }
                    // Проверяем наличие поинта в нужном направлении:
                    if (rightPoint != null)
                    {
                        switch (rightPoint.point_type) {
                            case Point.typePoint.empty:
                                clone_explosive = (GameObject)Instantiate(explosive, right_position, Quaternion.identity);
                                // Теперь уничтожаем игрока или противников расположенных на опасной дистанции ко взрыву:
                                distance_to_explosive = Vector3.Distance(rightPoint.transform.position, map.player.gameObject.transform.position);
                                if (distance_to_explosive <= damage_distance)
                                {
                                    // О нет игрока задело и убивает:
                                    map.player.destroy_anim = this.type_animation;
                                    map.player.state = PlayerClient.playerState.destroy;
                                }
                                // Теперь выполняем такуюже проверку для врагов(при большом количестве врагов такой подход даст просадку производительности, но это уже другая история :) ):
                                for (int k = 0; k < map.enemyInMapList.Count; k++)
                                {
                                    BotClient nextBot = map.enemyInMapList[k];
                                    distance_to_explosive = Vector3.Distance(rightPoint.transform.position, nextBot.gameObject.transform.position);
                                    if (distance_to_explosive <= damage_distance)
                                    {
                                        // О нет игрока задело и убивает:
                                        nextBot.destroy_anim = this.type_animation;
                                        nextBot.state = BotClient.bot_state.destroy;
                                    }
                                }
                                rightPoint = null;
                                break;
                            case Point.typePoint.blocked:
                                // Значит необходимо разрушить Box и освободить проход:
                                rightPoint.point_type = Point.typePoint.empty;
                                // Почемуто решил сделать так:
                                cube_position.x = rightPoint.thisPointCubePosition.x;
                                cube_position.y = rightPoint.thisPointCubePosition.y;
                                cube_position.z = rightPoint.thisPointCubePosition.z;
                                // Уничтожаем нормальный куб:
                                Destroy(rightPoint.thisPointCube, 0f);
                                Instantiate(rightPoint.destroyCubePrefab, cube_position, Quaternion.identity);
                                createBonus(rightPoint.pointPosition);
                                rightPoint = null;
                                right_destroy = true;
                                break;
                        }
                    }
                }
                // И обязательно центр бомбы:
                if (astar.mapPointDictionary.ContainsKey(center_position))
                {
                    centerPoint = astar.mapPointDictionary[center_position];
                }
            }
            // Проверяем наличие поинта в нужном направлении:
            if (centerPoint != null)
            {
                switch (centerPoint.point_type)
                {
                    case Point.typePoint.empty:
                        clone_explosive = (GameObject)Instantiate(explosive, center_position, Quaternion.identity);
                        // Теперь уничтожаем игрока или противников расположенных на опасной дистанции ко взрыву:
                        distance_to_explosive = Vector3.Distance(this.gameObject.transform.position, map.player.gameObject.transform.position);
                        //Debug.Log(distance_to_explosive);
                        if (distance_to_explosive <= damage_distance)
                        {
                            // О нет игрока задело и убивает:
                            map.player.destroy_anim = this.type_animation;
                            map.player.state = PlayerClient.playerState.destroy;
                        }
                        // Теперь выполняем такуюже проверку для врагов(при большом количестве врагов такой подход даст просадку производительности, но это уже другая история :) ):
                        for (int k = 0; k < map.enemyInMapList.Count; k++)
                        {
                            BotClient nextBot = map.enemyInMapList[k];
                            distance_to_explosive = Vector3.Distance(this.gameObject.transform.position, nextBot.gameObject.transform.position);
                            if (distance_to_explosive <= damage_distance)
                            {
                                // О нет игрока задело и убивает:
                                nextBot.destroy_anim = this.type_animation;
                                nextBot.state = BotClient.bot_state.destroy;
                            }
                        }
                        centerPoint = null;
                        break;
                }
            }

            // Удаляем нашу бомбу из списка бомб(который предотвращает установку нескольких бомб в одну)
            Map.Instance.player.allBombInMap.Remove(this);
            // Проверяем возможность бонуса:
            createBonus(this.bombPoint.pointPosition);
            // Уничтожаем нашу бомбу:
            Destroy(this.gameObject, 0.1f);
        }
    }

    void createBonus(Vector3i bonusPosition) {
        GameObject bonus_object = null;
        // Если выпадает шанс бонуса:
        if (isBonus()) {
            // Так Unity рандом специфичен и минимальные значения будут выпадать чаще, следует заполнять список с "дешевых бонусов"
            int random_Bonus = (int)Random.Range(0,allBonusPrefab.Count);
            Bonus new_Bonus = allBonusPrefab[random_Bonus];
            if (bonus_object == null) {
               bonus_object = (GameObject)Instantiate(new_Bonus.bonusPrefab, bonusPosition, Quaternion.identity);
            }
        }
    }

    // Рассчтываем шанс на выпадение бонуса
    bool isBonus() {
        bool result = false;
        float random = (float)Random.Range(0, 3f);
        // теперь проверяем шанс:
        if (random > bonus_Chance) {
            result = true;
        }
        //Debug.Log(random);
        return result;
    }
}
