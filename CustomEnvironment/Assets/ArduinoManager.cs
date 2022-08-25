using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ArduinoManager : MonoBehaviour
{
	//시리얼 포트
	SerialPort serial;

	void Start()
	{


		serial = new SerialPort("COM3", 9600);
		serial.Open();
	}

	void OnDestroy()
	{
		serial.Close();
	}

	public void Angle(float angle)
	{
		if (angle > 1)
		{
			Debug.Log(angle);
			serial.Write(angle.ToString());
		}
	}

	void Update()
	{
		//if (Input.GetKeyDown(KeyCode.Alpha1))
		//{
		//	//아두이노한테 1을 보낸다.
		//	serial.Write("1");
		//}
		//if (Input.GetKeyDown(KeyCode.Alpha2))
		//{
		//	//아두이노한테 2을 보낸다.
		//	serial.Write("2");
		//}
	}
}
