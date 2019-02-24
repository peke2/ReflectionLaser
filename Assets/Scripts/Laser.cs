using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Shot{

	GameObject m_laserBodyObj;
	GameObject m_laserTailObj;

	const int LASER_LENGTH = 32;
	int m_laserBodyLast = 0;
	int m_laserBodyCount = 0;
	GameObject[] m_laserBodies = new GameObject[LASER_LENGTH];

	// Use this for initialization
	void Start () {
		m_stageObj = GameObject.Find("Tilemap");

		m_laserBodyObj = Resources.Load<GameObject>("LaserBody");
	}

	// Update is called once per frame
	void Update () {
		var prevDir = m_direction;
		var prevPos = m_position;

		move0();

		GameObject obj;
		if( m_laserBodies[m_laserBodyLast] == null )
		{
			obj = GameObject.Instantiate<GameObject>(m_laserBodyObj);
			m_laserBodies[m_laserBodyLast] = obj;
		}
		else
		{
			obj = m_laserBodies[m_laserBodyLast];
		}
		obj.transform.position = prevPos;

		var angle = Vector2.Angle(new Vector2(1, 0), m_direction);

		var o = Vector3.Cross(new Vector3(1, 0, 0), m_direction);
		if( o.z < 0 )
		{
			angle = -angle;
		}

		obj.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));

		m_laserBodyLast = (m_laserBodyLast + 1) % LASER_LENGTH;
	}
}
