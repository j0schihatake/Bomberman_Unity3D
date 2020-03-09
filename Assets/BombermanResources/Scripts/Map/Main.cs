using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public static Main Instance = null;
    // Ссылки на миникарту:
    public GameObject minimapСamera = null;
    public CameraScript minimapCameraScript = null;
    // Элементы стартовой сцены:
    public GameObject startScene = null;
    public OptionsMenu options_Menu = null;

    //Ссылки на UI:
    public GameObject playerUI = null;
    public Canvas playerCanvas = null;

    public Map map = null;

    void Start() {
        //Пока что не пригодилось:
        DontDestroyOnLoad(this.gameObject);
        Instance = this;

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            onMainMenuButton();
        }
    }

    // Метод начинает новую игру:
    public void onStartButton() {
        startScene.SetActive(false);
        //Включаю миникарту:
        minimapСamera.SetActive(true);
        map.status = Map.mapStatus.generate;
    }

    // Метод переключает нас в главное меню:
    public void onMainMenuButton() {
        startScene.SetActive(true);
        //Отключаю миникарту
        minimapСamera.SetActive(false);
        map.status = Map.mapStatus.clean;
    }

    // Метод выполняет запуск следующего уровня:
    public void onNextLevelButton()
    {
        StartCoroutine(nextLevel());
    }

    IEnumerator nextLevel() {
        minimapСamera.SetActive(false);
        map.status = Map.mapStatus.clean;
        yield return new WaitForSeconds(3);
        map.status = Map.mapStatus.generate;
        yield return new WaitForSeconds(1);
        minimapСamera.SetActive(true);
    }

}
