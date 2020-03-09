using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {
//Создаю универсальную камеру:
private float leftrightspeed = 14;
private float upbackspeed = 14;
private float xSpeed = 14;
private Quaternion rotate;
private Vector3 movedirection;
private Vector3 rotation;
//Покачто это скопище всех существующих скриптов MouseLook:
private float xorbitSpeed = 250;
private float yorbitSpeed = 120.0f;
private float yMinLimit = -20f;
private float yMaxLimit = 80f;
private Vector3 angles;
private float x;
private float y;
public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
public RotationAxes axes = RotationAxes.MouseXAndY;
public float sensitivityX = 15F;
public float sensitivityY = 15F;
public float minimumX = -360F;
public float maximumX = 360F;
public float minimumY = -60F;
public float maximumY = 60F;
float rotationY = 0F;
//Режим следовани за обьектом:
public Transform FolowObject;
public float distance = 10f;
public float height = 5f;
private float heightDamping = 2f;
private float rotationDamping = 3f;

    public Camera spacesCamera = null;
	
//Переключением типа меняем поведение:
public TypesCamera cameraTypes;
public enum TypesCamera{
	followtounit,
	loocator,
	free,
	editor,
	orbit,
}
	
void Awake(){
	//DontDestroyOnLoad(this.gameObject);			
}
void Start(){
	angles = transform.eulerAngles;
	x = angles.y;
	y = angles.x;
//Р Р°Р·СЂРµС€Р°РµРј Rigidbody РІСЂР°С‰РµРЅРёРµ...
if (GetComponent<Rigidbody>()){
	GetComponent<Rigidbody>().freezeRotation = true;
}
}
void FixedUpdate(){
	WorldFreeCameraMetod();
        //if (spacesCamera!=null) {
          //  spacesCamera.fov = this.gameObject.GetComponent<Camera>().fov;
        //}
}
void LateUpdate(){}

//РњРµС‚РѕРґ РѕРїРёСЃС‹РІР°РµС‚ РїРѕРІРµРґРµРЅРёРµ РєР°РјРµСЂС‹ РґР»СЏ СЂР°Р·РЅС‹С… СЂРµР¶РёРјРѕРІ С‚РёРїР° РєР°РјРµСЂС‹...	
private void WorldFreeCameraMetod(){
if(cameraTypes == TypesCamera.free){						//Р”Р»СЏ СЂРµР¶РёРјР° СЃРІРѕР±РѕРґРЅРѕР№ РєР°РјРµСЂС‹...
	//РїРµСЂРµРјРµС‰РµРЅРёРµ РєР°РјРµСЂС‹:
		movedirection = (transform.rotation*new Vector3(Input.GetAxis("Horizontal")*leftrightspeed,0,Input.GetAxis("Vertical")*upbackspeed));
		this.transform.position = (transform.position+movedirection*Time.deltaTime);
	//РІСЂР°С‰РµРЅРёРµ РєР°РјРµСЂС‹:
if(Input.GetMouseButton(1)){
if(axes == RotationAxes.MouseXAndY){
	float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
}
else if (axes == RotationAxes.MouseX){
		transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
}
else{
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
}
}
}
if(cameraTypes == TypesCamera.followtounit){
	if(FolowObject != null){
	//РІСЃРµ РґР»СЏ РїР»Р°РІРЅРѕРіРѕ СЃР»РµРґРѕРІР°РЅРёСЏ РєР°РјРµСЂС‹ Р·Р° СЋРЅРёС‚РѕРј... FolowObject - С‚СЂР°РЅСЃС„РѕСЂРј СЋРЅРёС‚Р° Р·Р° РєРѕС‚РѕСЂС‹Рј РЅСѓР¶РЅРѕ СЃР»РµРґРѕРІР°С‚СЊ...
	float wantedRotationAngle = FolowObject.transform.eulerAngles.y;	
	float wantedHeight = FolowObject.transform.position.y + height;	
	float currentRotationAngle = transform.eulerAngles.y;
	float currentHeight = transform.position.y;
	currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
	currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
	Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
	transform.position = FolowObject.transform.position;
	transform.position -= currentRotation * Vector3.forward * distance;
	this.transform.position = (new Vector3(transform.position.x,currentHeight,transform.position.z));
	transform.LookAt (FolowObject.transform);
	}
}
if(cameraTypes == TypesCamera.editor){						//Р РµР¶РёРј РєР°РјРµСЂР° РІ СЂРµРґР°РєС‚РѕСЂРµ
	//РїРµСЂРµРјРµС‰РµРЅРёРµ РєР°РјРµСЂС‹:
if(Input.GetKey(KeyCode.LeftShift)){
		movedirection = (transform.rotation*new Vector3(Input.GetAxis("Horizontal")*leftrightspeed,0,Input.GetAxis("Vertical")*upbackspeed));
		this.transform.position = (transform.position+movedirection*Time.deltaTime);
}
	//РІСЂР°С‰РµРЅРёРµ РєР°РјРµСЂС‹:
if(Input.GetMouseButton(1)){
if(axes == RotationAxes.MouseXAndY){
	float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
}
else if (axes == RotationAxes.MouseX){
		transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
}
else{
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
}
}			
}
if (cameraTypes == TypesCamera.orbit) {						//Р РµР¶РёРј РєР°РјРµСЂР° РІ СЂРµРґР°РєС‚РѕСЂРµ
	if (FolowObject) {
		if(Input.GetMouseButton(1)){
			x += Input.GetAxis ("Mouse X") * xorbitSpeed * 0.02f;
			y -= Input.GetAxis ("Mouse Y") * yorbitSpeed * 0.02f;
		}
		if (y < -360) {y += 360;}
		if (y > 360) {y -= 360;}
		y = Mathf.Clamp (y, yMinLimit, yMaxLimit);	
		rotate = Quaternion.Euler (y, x, 0);
		Vector3 position = rotate * new Vector3(0.0f, 0.0f, -distance) + FolowObject.position;
		transform.rotation = rotate;
		transform.position = position;
		}	
	}

}	
}

/*
 * РўР—, РєР°РјРµСЂР° СЂРµРґР°РєС‚РѕСЂР°:
 * 
 * РїСЂР°РІР°СЏ РєРЅРѕРїРєР° РјС‹С€РєРё РІСЂР°С‰Р°РµС‚ РєР°РјРµСЂСѓ РІРѕРєСЂСѓРі С‚РѕС‡РєРё РІ С†РµРЅС‚СЂРµ РІРёРґР°
 * РїСЂРё РЅР°Р¶Р°С‚РёРё РєРЅРѕРїРѕРє WASD РїСЂРѕРёСЃС…РѕРґРёС‚ РїРµСЂРµРјРµС‰РµРЅРёРµ РєР°РјРµСЂС‹
 * 
 * **/
