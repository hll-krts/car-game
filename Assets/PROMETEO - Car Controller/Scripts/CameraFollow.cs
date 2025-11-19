using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform carTransform;
	public float followSpeed = 2;
	Vector3 initialCameraPosition;
	Vector3 initialCarPosition;
	Vector3 absoluteInitCameraPosition;

	void Start(){
		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
	}

	void FixedUpdate()
	{
		//Look at car
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = _rot;

		//Move to car
		Vector3 _targetPos = carTransform.Find("Close").transform.position;
		transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);

	}

}