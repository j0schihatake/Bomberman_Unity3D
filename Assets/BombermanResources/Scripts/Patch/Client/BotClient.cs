using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotClient : ClientPatch
{
	//Скрипт управляет АЙ бота:

	/*
     * для классического режима реализую следующее поведение бота:
     * 1. бот получает случайную(или нет) позицию 
     * 2. бот движется в данную позицию
     */

	public bool moveRandom = true;

    // Поднимать ли камеру после смерти:
    public bool up_death_Cam = false;
    public float death_Cam_Y = 1.9f;

    // Как только этот противник встретит игрока, он сразу будет мчаться к нему:
    public GameObject enemy_player = null;

	// для предотвращения постоянного создания векторов:
	public Vector3 returned = Vector3.zero;
	public Vector3 testVector = Vector3.zero;
	public Point lostPoint = null;

    // Переменная обозначает с каким типом гибели мы столкнулись:
    public string destroy_anim = "";

    // Анимации при пойманном игроке
    public string start_eat_Player = "";
    public string continue_eat_Player = "";
    public string destroy_Player_Animation = "";

    // ссылка на риджидбоди:
    private Rigidbody myRigidbody = null;

	public float stopDistance = 0.1f;
	// большее значение этого параметра предотвратит "проскакивание"

	public bool IsMoving = false;
	//Все угловые вращения:
	public float angleY = 0f;
	public float angleX = 0f;
	public float angleZ = 0f;

	public float rotationSpeed = 3f;
	public float speed = 10f;
	// вращение без изменения плоскости:
	public bool no_change_platform = false;

	// private int countPoint;

	// храним представление о платформе на которой мы находимся
	public clientPlatform platform;
	public enum clientPlatform
	{
		up,
	}

	// Так как нереально трудно тестить повороты, завожу enum с указанием следующего направления(направление_платформа):
	public rotateDirections direction;
	public enum rotateDirections
	{
		// Плоскость up:
		forward_up,
		back_up,
		left_up,
		right_up,
	}

    //----------------------------------------Названия состояний анимации:
    public string iddleAnimation = "";
    public string moveAnimation = "";
    public Animator animator = null;

    public bot_state state;
    public enum bot_state {
        work,
        destroy,
        eat_Player,
    }

    void Awake ()
	{
		platform = clientPlatform.up;
		myRigidbody = this.gameObject.GetComponent<Rigidbody> ();
		lostPointVector3D = myRigidbody.transform.position;
		//lostPoint.y += 0.5f;
		nextPosition = myRigidbody.transform.position; 
	}

    void Start() {
        aStar = AStar.Instance;
    }

	void Update ()
	{
        switch (state)
        {
        //--------------------------------------Bot жив-------------------------------------------
            case bot_state.work:
                // Если я закину какую то позицию:
                if (targetPosition != null)
                {
                    if (patch.Count == 0 && !patch_not_realy)
                    {
                        if (!aStar.clientList.Contains(this))
                        {
                            if (lostPointVector3D != targetPosition.position)
                            {
                                aStar.clientList.Add(this);
                            }
                        }
                    }
                    else
                    {
                        // Сбрасываем параметры поиска:
                        patch_not_realy = false;
                        targetPosition = null;
                    }
                }
                // Ищем точки рандомно только если не увидели игрока
                if (enemy_player == null)
                {
                    if (moveRandom)
                    {
                        //Бот сам автоматически запрашивает следующую точку
                        if (targetPosition == null)
                        {
                            //требуется получить случайную позицию у AStar:
                            int random = (int)Random.Range(0, aStar.mapPointList.Count);
                            if (aStar.mapPointList[random].point_type != Point.typePoint.blocked)
                            {
                                targetPosition = aStar.mapPointList[random].gameObject.transform;
                            }
                        }
                    }
                }
                else {
                    //Сразу следуем к игроку:
                    targetPosition = enemy_player.transform;
                }

                //Боту нужно следовать от точки к точке:
                if (patch.Count > 0)
                {
                    if (!IsMoving)
                    {
                        if (patch.Count > 0)
                        {
                            countPoint = patch.Count - 1;
                            lostPoint = aStar.mapPointDictionary[aStar.Vector3toVector3i(lostPointVector3D)];
                            nextPosition = patch[countPoint].gameObject.transform.position;
                            //поворачиваем базу танка в сторону следующего поинта
                            rotateToNextPosition(nextPosition);
                        }
                    }
                }
                //Корректор вращения (описаны только на одной плоскости):
                testBaseRotator();
                //Остальные методы:
                baseRotate(angleX, angleY, angleZ);
                move(nextPosition);
                break;
            //----------------------------------------------Bot мертв-----------------------------------------
            case bot_state.destroy:
                destroy();
                Map.Instance.enemyInMapList.Remove(this);
                Destroy(this.gameObject,10);
                break;
            //----------------------------------------------Bot поймал игрока-----------------------
            case bot_state.eat_Player:
                move(nextPosition);
                eat();
                break;
            }
        }

    void destroy()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(destroy_anim))
        {
            animator.Play(destroy_anim, 0);
        }
    }

    void eat() {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(start_eat_Player) && !stateInfo.IsName(continue_eat_Player))
        {
            animator.Play(start_eat_Player, 0);
        }
    }

    // метод вращает бота согласно следующему поинту:
    public void rotateToNextPosition (Vector3 nextVector)
	{
		// Формирую текущую позицию:
		returned.x = lostPointVector3D.x;
		returned.y = lostPointVector3D.y;
		returned.z = lostPointVector3D.z;

        // Проверяем(в а не впереди ли следующий поинт)
        if ((lostPointVector3D.x == nextVector.x)
            & lostPointVector3D.z < nextVector.z) {
            direction = rotateDirections.forward_up;
        }

        // Проверяем(в а не позади ли следующий поинт)
        if ((lostPointVector3D.x == nextVector.x)
            & lostPointVector3D.z > nextVector.z)
        {
            direction = rotateDirections.back_up;
        }

        // Проверяем(в а не слева ли следующий поинт)
        if ((lostPointVector3D.x > nextVector.x)
            & lostPointVector3D.z == nextVector.z)
        {
            direction = rotateDirections.left_up;
        }

        // Проверяем(в а не справа ли следующий поинт)
        if ((lostPointVector3D.x < nextVector.x)
            & lostPointVector3D.z == nextVector.z)
        {
            direction = rotateDirections.right_up;
        }
    }

	//Делаю метод который будет устанавливать углы(реализация 2 я смогу сходу видеть верно или нет):
	public void testBaseRotator ()
	{
		switch (direction) {
		//Платформа вверху:
		case rotateDirections.forward_up:
			angleY = 0f;
			angleX = 0f;
			angleZ = 0f;
			break;
		case rotateDirections.back_up:
			angleY = 180f;
			angleX = 0f;
			angleZ = 0f;
			break;
		case rotateDirections.left_up:
			angleY = -90f;
			angleX = 0f;
			angleZ = 0f;
			break;
		case rotateDirections.right_up:
			angleY = 90f;
			angleX = 0f;
			angleZ = 0f;
			break;
		}
	}

	//Выполнение перемещения к точке:
	void move (Vector3 nextPosition)
	{
		if (myRigidbody.transform.position != nextPosition) {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(moveAnimation))
            {
                animator.Play(moveAnimation, 0);
            }
            IsMoving = true;
			myRigidbody.transform.position = Vector3.MoveTowards (myRigidbody.transform.position, nextPosition, speed * Time.deltaTime);
		} else {
			if (patch.Count > 0) {
				patch.RemoveAt (patch.Count - 1);
			}
			if (targetPosition != null) {
				if (lostPointVector3D == targetPosition.position) {
					targetPosition = null;
                    //Так-же играем статичную анимацию
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    if (!stateInfo.IsName(iddleAnimation))
                    {
                        animator.Play(iddleAnimation, 0);
                    }
                }
			}
			nextPosition = myRigidbody.transform.position;
			lostPointVector3D = nextPosition;
			lostPoint = null;
			IsMoving = false;
		}
	}

	//поворот базы на угол за одно нажатие
	void baseRotate (float angleX, float angleY, float angleZ)
	{
		Quaternion nextRotate = Quaternion.Euler (new Vector3 (angleX, angleY, angleZ));
		myRigidbody.transform.rotation = Quaternion.Lerp (myRigidbody.transform.rotation, nextRotate, rotationSpeed);
	}

    // Теперь если мы коснулись колайдера игрока:
    void OnCollisionEnter(Collision coll) {
        if (coll.gameObject.tag.Equals("player")) {
            //проверяем а не "употребил" ли игрок защиту:
            if (!Map.Instance.player.invisible) {
                if (Map.Instance.player.state != PlayerClient.playerState.destroy
                    && Map.Instance.player.state != PlayerClient.playerState.off
                    && this.state != bot_state.destroy) {
                    //Debug.Log("я столкнулся с игроком");
                    Map.Instance.player.destroy_anim = destroy_Player_Animation;
                    Map.Instance.player.state = PlayerClient.playerState.destroy;
                    //Так же сами переходим в состояние:
                    this.state = bot_state.eat_Player;
                    nextPosition = Map.Instance.player.myRigidbody.transform.position;
                    if (up_death_Cam) {
                        Vector3 new_Cam_Position = Vector3.zero;
                        new_Cam_Position.x = Map.Instance.player.cameraObject.transform.position.x;
                        new_Cam_Position.y = death_Cam_Y;
                        new_Cam_Position.z = Map.Instance.player.cameraObject.transform.position.z;
                        Map.Instance.player.cameraObject.transform.position = new_Cam_Position;
                        //Debug.Log(new_Cam_Position);
                    }
                }
            }
        }
    }

    // Только враги с тригером смогут обнаруживать игрока в процессе игры:
    void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.tag.Equals("player")) {
            //Начинаем приследовать игрока:
            enemy_player = coll.gameObject;
        }
    }

}
