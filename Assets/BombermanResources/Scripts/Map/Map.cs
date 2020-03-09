using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

//------------------------------------Заявленные характеристики уровня----------------------------
    // Размеры уровня:
    public int map_X_Size = 100;
    public int map_Z_Size = 100;

    // Количество противников:
    public int enemy_in_map_count = 5;
    public int extra_enemy_in_map_count = 5;

    // Время на уровень:
    public float level_time = 700f;

    // Список доступных префабов противников:
    public List<GameObject> enemyPrefabList = new List<GameObject>();
    public List<GameObject> extraEnemyPrefabList = new List<GameObject>();

    //-------------------------------------Остальные параметры---------------------------------------

    public static Map Instance = null;
        
    // Список неразрушаемых блоков(как в классическом Bomberman)
    public List<GameObject> dont_destroy_Box = new List<GameObject>();

    // Список разрушаемых блоков(они тоже могут быть разными)
    public List<GameObject> destroyed_Box = new List<GameObject>();

    // Список для быстрого доступа по индексу к "свободным" point-ам:
    public List<Point> empty_point_List = new List<Point>(); 

    // Префаб пустого Box-а:
    public GameObject empty_Box = null;

    // Префаб игрока:
    public GameObject playerPrefab = null;

    // Префаб финишного Box-a:
    public List<GameObject> finishPrefabList = null;

    // Префаб стартового Box-a:
    public List<GameObject> startPrefabList = null;

    // Список противников на карте:
    public List<BotClient> enemyInMapList = new List<BotClient>();

    // Для быстрого удаления блоков карты буду удалять их родительский обьект:
    public GameObject emptyMapBoxObject = null;
    public GameObject mapBoxObject = null;
    public float enemyChance = 0.1f;

    // Размещение главного игрока:
    public Vector3i start_Player_Position = Vector3i.zero;
    // Ссылка на скрипт игрока:
    public PlayerClient player = null;

    public AStar aStar = null;

    // Статус игровой карты:
    public mapStatus status;
    public enum mapStatus {
        generate,
        work,
        clean,
    }

    void Start() {
        aStar = AStar.Instance;
        Instance = this;
        //status = mapStatus.generate;
    }

    void Update() {
        switch (status) {
            case mapStatus.work:
                break;
            case mapStatus.generate:
                generate_level();
                // Если Astar уже имеет мап-у поинтов:
                if (aStar.init)
                {
                    status = mapStatus.work;
                }
                break;
            case mapStatus.clean:
                clean();
                status = mapStatus.work;
                break;
        }
    }

    void clean() {
        // Одинм махом уничтожаем все box:
        Destroy(mapBoxObject, 1f);
        mapBoxObject = null;
        aStar.astarClean();
        // Уничтожаем противников:
        if (enemyInMapList.Count > 0)
        {
            for (int i = 0; i < enemyInMapList.Count; i++)
            {
                BotClient next = enemyInMapList[i];
                next.state = BotClient.bot_state.destroy;
                Destroy(next.gameObject);
            }
        }
        Map.Instance.empty_point_List.Clear();
        enemyInMapList.Clear();
        // Сбрасываем параметры:
        aStar.init = false;
        aStar.autoInit = false;
        aStar.clientList.Clear();
        aStar.state = AStar.aStarState.pause;
        // Удаляем игрока:
        if (player != null) {
            Destroy(player.cameraObject);
            Destroy(player.gameObject);
            player = null;
        }

    }

    // Метод генерирует рандомный уровень:
    void generate_level() {
        // Запускаем генерацию блоков до заполнения списка поинтов:
        if (!aStar.init && !aStar.autoInit)
        { 
            //Запускаем формирование списка поинтов:
            aStar.autoInit = true;

            GameObject new_Box = null;
            Vector3i next_Box_Position = Vector3i.zero;

            mapBoxObject = (GameObject)Instantiate(emptyMapBoxObject, this.gameObject.transform.position, Quaternion.identity);
            mapBoxObject.transform.parent = this.gameObject.transform;

            // Итак генерируем игровую плоскость уровня(от 0.0.0) 
            for (int i = 0; i < map_X_Size; i++)
            {
                for (int j = 0; j < map_Z_Size; j++)
                {
                    // Формируем следующую позицию:
                    next_Box_Position.x = i * aStar.stepRight;
                    next_Box_Position.y = 0;
                    next_Box_Position.z = j * aStar.stepForward;

                    /* Итак интересно что за блок мы получим:
                     * 0 - пустой Box:
                     * 1 - разрушаемый заблокированный Box:
                     * 2 - непроходимый и не разрушаемый Box:
                    */
                    int random_type_Box = getRandom(0, 2);
                    switch (random_type_Box)
                    {
                        case 0:
                            new_Box = (GameObject)Instantiate(empty_Box, next_Box_Position, Quaternion.identity);
                            new_Box.transform.parent = mapBoxObject.transform;
                            Box box = new_Box.GetComponent<Box>();
                            AStar.Instance.allBox.Add(box);
                            empty_point_List.Add(box.boxpoints[0]);
                            break;
                        case 1:
                            int random_Destroyed_Box = getRandom(0, destroyed_Box.Count);
                            new_Box = (GameObject)Instantiate(destroyed_Box[random_Destroyed_Box], next_Box_Position, Quaternion.identity);
                            new_Box.transform.parent = mapBoxObject.transform;
                            AStar.Instance.allBox.Add(new_Box.GetComponent<Box>());
                            break;
                        case 2:
                            int random_Dont_Destroy_Box = getRandom(0, dont_destroy_Box.Count);
                            new_Box = (GameObject)Instantiate(destroyed_Box[random_Dont_Destroy_Box], next_Box_Position, Quaternion.identity);
                            new_Box.transform.parent = mapBoxObject.transform;
                            AStar.Instance.allBox.Add(new_Box.GetComponent<Box>());
                            break;
                    }
                }
            }

            // Теперь размещаем финишный Box:
            setFinish();

            // Теперь размещаем противников на карте:
            for (int i = 0; i < enemy_in_map_count; i++)
            {
                createEnemyToMap(0);
            }
        }

        // Размещаем игрока(если его еще нет и настраиваем GUI):
        if (player == null && aStar.init)
        {
            start_Player_Position = setPlayerStartPosition();
            GameObject playerGameObject = (GameObject)Instantiate(playerPrefab, start_Player_Position, Quaternion.identity);
            player = playerGameObject.GetComponent<PlayerClient>();
            Main.Instance.minimapCameraScript.FolowObject = playerGameObject.transform;
        }
    }

    // Метод для получения(нерандомного на самом деле) рандома:
    int getRandom(int min_count, int max_count) {
        int returned = 0;

        returned = (int)Random.Range(min_count, max_count);

        return returned;
    }

    // Метод устанавливает стартовую позицию игрока:
    /*
     *  Внимание! Проанализировав возможные варианты,
     *  я пришел к выводу что МИНИМУМ необходимы 3 поинта в форме угла
     *  2 поинта рядом по одной оси, и 1 с отличием в еще одной оси(зависит от специфики направления взрывной волны)
     *  П.с. в рамках тестового задания не стал полностью рандомную позицию реализовывать. Решил как в оригинале(на сколько мне известно).
    */
    Vector3i setPlayerStartPosition() {

        Vector3i start_position = Vector3i.zero;
        start_position.x = 2;
        start_position.y = 1;
        start_position.z = 2;

        Vector3i start_position_2 = Vector3i.zero;
        start_position_2.x = 2;
        start_position_2.y = 1;
        start_position_2.z = 4;

        Vector3i start_position_3 = Vector3i.zero;
        start_position_3.x = 4;
        start_position_3.y = 1;
        start_position_3.z = 2;

        Point select = null;

        //Debug.Log("start_position = " + start_position + ", start_postion_2 = " + start_position_2 + ", start_position_3 = " + start_position_3);

        //Теперь производим очистку данных поинтов:
        if (aStar.mapPointDictionary.ContainsKey(start_position)) {
            select = aStar.mapPointDictionary[start_position];
            if (select.point_type == Point.typePoint.blocked) {
                Destroy(select.thisPointCube);
                select.point_type = Point.typePoint.empty;
            }
        }

        if (aStar.mapPointDictionary.ContainsKey(start_position_2))
        {
            select = aStar.mapPointDictionary[start_position_2];
            if (select.point_type == Point.typePoint.blocked)
            {
                Destroy(select.thisPointCube);
                select.point_type = Point.typePoint.empty;
            }
        }

        if (aStar.mapPointDictionary.ContainsKey(start_position_3))
            select = aStar.mapPointDictionary[start_position_3];
            if (select.point_type == Point.typePoint.blocked)
            {
                Destroy(select.thisPointCube);
                select.point_type = Point.typePoint.empty;
            }
        return start_position;
    }

    // Финиш в игре это просто специальный блок:
    public void setFinish() {
        Vector3 new_finish_position = Vector3.zero;
        // Получаем Box вместо которого будем устанавливать финишь
        Box destroyBox = getRandomBox();
        new_finish_position = destroyBox.gameObject.transform.position;
        aStar.allBox.Remove(destroyBox);
        aStar.mapPointList.Remove(destroyBox.boxpoints[0]);
        aStar.mapBoxDictionary.Remove(aStar.Vector3toVector3i(destroyBox.transform.position));
        aStar.mapPointDictionary.Remove(destroyBox.boxpoints[0].pointPosition);
        Destroy(destroyBox.gameObject);
        // Выбираем случайный финиш:
        GameObject finishPrefab = null;
        int random_Prefab = (int)Random.Range(0, this.finishPrefabList.Count);
        finishPrefab = this.finishPrefabList[random_Prefab];
        // Теперь создаем на позиции уничтоженного Box-a Box
        GameObject new_Finish = (GameObject)Instantiate(finishPrefab, new_finish_position, Quaternion.identity);
        new_Finish.transform.parent = mapBoxObject.transform;
        Box finishBox = new_Finish.GetComponent<Box>();
        aStar.allBox.Add(finishBox);
        aStar.mapPointList.Add(finishBox.boxpoints[0]);
        aStar.mapBoxDictionary.Add(aStar.Vector3toVector3i(new_Finish.transform.position), finishBox);
        aStar.mapPointDictionary.Add(finishBox.boxpoints[0].pointPosition, finishBox.boxpoints[0]);
    }

    // Получаем случайный 
    Box getRandomBox() {
        Box random = null;
        int random_Box = (int)Random.Range(0, aStar.allBox.Count);
        random = aStar.allBox[random_Box];
        return random;
    }

    // Метод размещает противников:
    public void createEnemyToMap(int type_enemy) {
        GameObject newEnemyPrefab = null;
        Vector3 enemy_Position = getEmptyRandomPoint().gameObject.transform.position;
        switch (type_enemy) {
            case 0:
                int rand = (int)Random.Range(0, enemyPrefabList.Count);
                newEnemyPrefab = enemyPrefabList[rand];
                break;
            case 1:
                int ext_rand = (int)Random.Range(0, extraEnemyPrefabList.Count);
                newEnemyPrefab = extraEnemyPrefabList[ext_rand];
                BotClient bot = newEnemyPrefab.GetComponent<BotClient>();
                bot.enemy_player = player.gameObject;
                break;
        } 
        GameObject enemy_clone = (GameObject)Instantiate(newEnemyPrefab, enemy_Position, Quaternion.identity);
        enemyInMapList.Add(enemy_clone.GetComponent<BotClient>());
    }


    // Метод возвращает пустой случайный поинт:
    public Point getEmptyRandomPoint() {
        Point returned = null;

        int random = (int)Random.Range(0,empty_point_List.Count);
        returned = empty_point_List[random];

        return returned;
    }
}
