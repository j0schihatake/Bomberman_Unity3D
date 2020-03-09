using UnityEngine;
using UnityEngine.UI;
using CnControls;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerClient : ClientPatch
{
	// для предотвращения постоянного создания векторов:
	public Vector3 returned = Vector3.zero;
	public Vector3 testVector = Vector3.zero;

	public Animator animator = null;
    public string runAnimation = "";
    public string iddleAnimation = "";
    public string winAnimation = "";

	public GameObject cameraObject = null;
	public GameObject playerCanvas = null;

    public int bombLimit = 1;
    public int powerPomb = 1;

    // Переменная обозначает с каким типом гибели мы столкнулись:
    public string destroy_anim = "";

	// Следующая позиция:
	public Vector3 lostPoint = Vector3.zero;

	// ссылка на риджидбоди:
	public Rigidbody myRigidbody = null;

	// точка выстрела:
	public GameObject schootPoint = null;
	public GameObject BombPrefab = null;

	public float stopDistance = 0.1f;
    // большее значение этого параметра предотвратит "проскакивание"

    // Этот параметр означает заметен ли игрок противникам:
    public bool invisible = false;
    public float invisible_time = 10f;                      

    public Camera playerCamera = null;

	public bool IsMoving = false;
	// Все угловые вращения:
	public float angleY = 0f;
	public float angleX = 0f;
	public float angleZ = 0f;

	public float rotationSpeed = 3f;
	public float speed = 10f;

    // UI:
    public GameObject win = null;
    public GameObject loose = null;

    // Таймер (по умолчанию на каждый уровень даю 300)
    public Text level_time_info = null;
    public float level_time = 30f;

    // Заел на будущее:
	public bool islocalPlayer = false;

	// Текущее состояние машины:
	public playerState state;

    // Список установленных игроком бомб:
    public List<Bomb> allBombInMap = new List<Bomb>();
	public enum playerState
	{
		live,
		destroy,
        off,
        win,
	}

	// Храним представление о платформе на которой мы находимся:
	public clientPlatform platform;
	public enum clientPlatform
	{
		up,
		down,
		forward,
		back,
		left,
		right,
	}

	void Awake ()
	{
		platform = clientPlatform.up;
		myRigidbody = this.gameObject.GetComponent<Rigidbody> ();
		lostPoint = myRigidbody.transform.position;
		nextPosition = myRigidbody.transform.position;

        // Отключаем лишний UI:
        win.SetActive(false);
        loose.SetActive(false);
	}

	void Start ()
	{
        aStar = AStar.Instance;
        activateControll ();
        // Получаем настройки:
        level_time = Map.Instance.level_time;
        
        level_time_info.text = level_time.ToString();
    }

    void Update() {
        //Запускаем таймер:
        if (level_time > 0)
        {
            level_time -= Time.deltaTime;
            level_time_info.text = level_time.ToString();
        }
        else {
            // Тут выпускаем гончих:
            for (int i = 0; i < Map.Instance.extra_enemy_in_map_count; i++) {
                Map.Instance.createEnemyToMap(1);
            }
            // Пусть и не из оригинала но ставим таймер снова
            level_time = Map.Instance.level_time;
        }
        
        // Выполняем любой код только если игрок жив
        switch (state) {
            case playerState.live:
                 // Остальные методы:
                 rotate();
                 move(nextPosition);
                 if (islocalPlayer)
                 {
                     Schoot();

                     // Обрабатываем движение вперед:
                     if ((Input.GetAxis("Vertical") > 0) & (!IsMoving)
                        || (CnInputManager.GetButtonUp("moveButton") && (!IsMoving)))
                        {
                            IsMoving = true;

                            nextPosition = getNextPosition(aStar.mapPointDictionary);
                        }
                    }
                break;
            case playerState.destroy:
                // Проигрываем последнюю анимацию:
                destroy();
                state = playerState.off;
                break;
            case playerState.off:
                StartCoroutine(looses());
                break;
            case playerState.win:
                StartCoroutine(wines());
                // Играем победную анимацию:
                winn();
                break;
        }
	}

    void destroy() {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(destroy_anim))
        {
            animator.Play(destroy_anim, 0);
        }
    }

    void winn() {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(winAnimation))
        {
            animator.Play(winAnimation, 0);
        }
    }

    // С небольшой паузой выводим UI поражения:
    IEnumerator looses() {
        yield return new WaitForSeconds(5);
        if (!loose.activeSelf)
        {
            loose.SetActive(true);
        }
    }

    // С небольшой паузой выводим UI победы:
    IEnumerator wines()
    {
        yield return new WaitForSeconds(5);
        if (!win.activeSelf)
        {
            win.SetActive(true);
        }
    }

    // Выстрел
    public void Schoot ()
	{
        if (Input.GetKeyDown(KeyCode.Space) || CnInputManager.GetButtonUp("bombButton")) {

            bool clone = false;
            if (allBombInMap.Count > 0)
            {
                // Проверяем а есть ли у нас еще бомбы, и не установили ли мы уже бомбу здесь:
                for (int i = 0; i < allBombInMap.Count; i++)
                {
                    Bomb next = allBombInMap[i];
                    if (next.bombPoint.pointPosition == aStar.Vector3toVector3i(this.lostPoint))
                    {
                        clone = true;
                    }
                }
            }
 
            if (!clone && allBombInMap.Count < bombLimit) {
                // Отлично создаем новую бомбу:
                GameObject clone_Bomb = (GameObject)Instantiate(BombPrefab, lostPoint, Quaternion.identity);
                Bomb bomb = clone_Bomb.GetComponent<Bomb>();
                bomb.bombPoint = aStar.mapPointDictionary[aStar.Vector3toVector3i(this.lostPoint)];
                // Реализуем возможность управлять взрывной силой через бонусы:
                bomb.power_strenght = powerPomb;
                allBombInMap.Add(bomb);
            }
		}
	}

	public void activateControll ()
	{
		if (islocalPlayer) {
			//playerCanvas.SetActive (true);
			//cameraObject.SetActive (true);
			//myWeapon.enabled = true;
			aStar.initialize ();
		} else {
			//playerCanvas.SetActive (false);
			//cameraObject.SetActive (false);
			//myWeapon.enabled = false;
			//AStar.initialize();
		}
	}

    // метод возвращает необходимый поинт
    public Vector3 getNextPosition(Dictionary<Vector3i, Point> allPoint)
    {
        Vector3i finded = Vector3i.zero;
        //Формирую текущую позицию:
        returned.x = lostPoint.x;
        returned.y = lostPoint.y;
        returned.z = lostPoint.z;
        //Ускоренный доступ к переменной
        float x = lostPoint.x;
        float y = lostPoint.y;
        float z = lostPoint.z;
        //может это ускорит процесс:
        //testVector = lostPoint;
        Vector3 direction = myRigidbody.transform.forward.normalized;
        testVector.x = returned.x;
        testVector.y = returned.y;
        testVector.z = returned.z + AStar.Instance.stepForward;
        //----------------------------------------------------Позиция впереди:
        //База сейчас повернута вперед(относительно мировых координат)
        if (myRigidbody.transform.forward.normalized == Vector3.forward)
        {
            //---------------------------Проверяем есть ли пойнт на полу перед нами
            testVector.x = x;
            testVector.y = y;
            testVector.z = z + aStar.stepForward;
            finded = aStar.Vector3toVector3i(testVector);
            //едем прямо:
            if (aStar.mapPointDictionary.ContainsKey(finded) 
                && aStar.mapPointDictionary[finded].point_type != Point.typePoint.blocked)
            {
                returned.z += aStar.stepForward;
            }
        }
        //База сейчас повернута назад:
        if (myRigidbody.transform.forward.normalized == (Vector3.back))
        {
            testVector.x = x;
            testVector.y = y;
            testVector.z = z - aStar.stepForward;
            finded = aStar.Vector3toVector3i(testVector);
            //едем прямо:
            if (aStar.mapPointDictionary.ContainsKey(finded)
                && aStar.mapPointDictionary[finded].point_type != Point.typePoint.blocked)
            {
                returned.z -= aStar.stepForward;
            }
        }
        //база повернута влево:
        if (myRigidbody.transform.forward.normalized == (Vector3.left))
        {
            testVector.x = x - aStar.stepRight;
            testVector.y = y;
            testVector.z = z;
            finded = aStar.Vector3toVector3i(testVector);
            //едем прямо:
            if (aStar.mapPointDictionary.ContainsKey(finded)
                && aStar.mapPointDictionary[finded].point_type != Point.typePoint.blocked)
            {
                returned.x -= aStar.stepRight;
            }
        }
        //база повернута вправо:
        if (myRigidbody.transform.forward.normalized == (Vector3.right))
        {
            testVector.x = x + aStar.stepRight;
            testVector.y = y;
            testVector.z = z;
            finded = aStar.Vector3toVector3i(testVector);
            //едем прямо:
            if (aStar.mapPointDictionary.ContainsKey(finded)
                && aStar.mapPointDictionary[finded].point_type != Point.typePoint.blocked)
            {
                returned.x += aStar.stepRight;
            }
        }
        return returned;
    }

    public void armor_Bonus() {
        StartCoroutine(invisible_inc());
    }

    // Реализуем временную неуязвимость:
    IEnumerator invisible_inc() {
        invisible = true;
        yield return new WaitForSeconds(invisible_time);
        invisible = false;
    }        

	// Выполнение перемещения к точке:
	void move (Vector3 nextPosition)
	{
		if (myRigidbody.transform.position != nextPosition) {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(runAnimation))
            {
                animator.Play(runAnimation, 0);
            }
            
            //играем звук движения
            /*
            if (!sourceBase.isPlaying) {
                 sourceBase.clip = movementBaseSound;
                 sourceBase.Play();
            }
            */
            if (islocalPlayer) {
                IsMoving = true;
				myRigidbody.transform.position = Vector3.MoveTowards (myRigidbody.transform.position, nextPosition, speed * Time.deltaTime);
			} else {
				myRigidbody.transform.position = nextPosition;
			}
		} else {
            //Проигрываю анимацию (бездействия) проверка на инпут чтоб избежать рывков:
            if ((Input.GetAxis("Vertical") == 0) 
                || (CnInputManager.GetAxis("Vertical") == 0))
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(iddleAnimation))
                {
                    animator.Play(iddleAnimation, 0);
                }
            }
            lostPoint = nextPosition;
			IsMoving = false;
			//sourceBase.Stop();
		}
	}

	// Поворот Игрока на угол:
	// округление углов
	void angler ()
	{
			 //Это вращение платформы кнопками.
            //------------------------------------------------------------Вращение базы влево---------------------------------------
            if (Input.GetKeyUp(KeyCode.A) || CnInputManager.GetButtonUp("leftButton"))
            {
                baseToLeft();
            }

            //---------------------------------------------------------------Вращение базы вправо------------------------------------
            if (Input.GetKeyUp(KeyCode.D) || CnInputManager.GetButtonUp("rightButton"))
            {
                baseToRight();
            }
            
	}

	public void baseToRight ()
	{
        if (!IsMoving)
        {
            //up плоскость
            if (platform == clientPlatform.up)
            {
                angleX = 0;
                angleZ = 0;
                if (angleY < 180)
                {
                    angleY += 90;
                }
                else
                {
                    angleY -= 90;
                    angleY *= (-1);
                }
            }
        }
	}

	public void baseToLeft ()
	{
		if (!IsMoving) {
			//up плоскость
			if (platform == clientPlatform.up) {
				angleX = 0;
				angleZ = 0;
				if (angleY > -180) {
					angleY -= 90;
				} else {
					angleY += 90;
					angleY *= (-1);
				}
			}
		}
	}

	// Метод поворота(в данном методе в дальнейшем возможно добавить еще какие либо вращения)
	public void rotate ()
	{
        angler();
        //----------------------------------------Выполнение
        unitRotate (angleX, angleY, angleZ);
	}

	//поворот базы на угол за одно нажатие
	void unitRotate (float angleX, float angleY, float angleZ)
	{
		Quaternion nextRotate = Quaternion.Euler (new Vector3 (angleX, angleY, angleZ));
		myRigidbody.transform.rotation = Quaternion.Lerp (myRigidbody.transform.rotation, nextRotate, rotationSpeed);
	}

	public void Damaged ()
	{
		Debug.Log ("Я получил урон");
	}

    //-----------------------------------------------АНИМАЦИЯ---------------------------------------------------------------
    public void randomAnimation(GameObject meshObject, List<string> list)
    {
        int index = (int)Random.Range(0.0f, list.Count);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Scelet.001|Idle"))
        {
            playOnlyAnimation(meshObject, list[index]);
        }
    }

    public void playOnlyAnimation(GameObject meshObject, string animationState)
    {
        meshObject.GetComponent<Animator>().ResetTrigger(animationState);
        meshObject.GetComponent<Animator>().Play(animationState, 0);
    }

    //-----------------------------------------------UI---------------------------------------------
    public void onNextLevelButton() {
        Main.Instance.onNextLevelButton();
    }

    //Назад к главному меню:
    public void onExitButton() {
        Main.Instance.onMainMenuButton();
    }

}
