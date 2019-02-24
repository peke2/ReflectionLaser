using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {

	//List<Shot> m_shots;
	GameObject m_baseObject;

	public Vector2 m_direction = new Vector2(1,0);
	public float m_speed = 1;
	

	// Use this for initialization
	void Start () {
		//m_baseObject = Resources.Load<GameObject>("Shot");
		m_baseObject = Resources.Load<GameObject>("Laser");

	}

	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Fire1"))
		{
			shoot(gameObject.transform.position, m_direction, m_speed);
		}
	}


	void shoot(Vector2 pos, Vector2 dir, float speed)
	{
		var shotObj = GameObject.Instantiate<GameObject>(m_baseObject);
		//var shot = shotObj.GetComponent<Shot>();
		var shot = shotObj.GetComponent<Laser>();
		shot.setInfo(pos, dir, speed);
		shotObj.transform.SetParent(gameObject.transform, false);
	}
}
