using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour {

	public GameObject m_headPartsObj;
	public GameObject m_neckPartsObj;

	const int NECK_PARTS_MAX = 4;
	GameObject[] m_parts = new GameObject[NECK_PARTS_MAX];
	GameObject m_headParts;

	Vector3[] m_positions = new Vector3[NECK_PARTS_MAX];
	Vector3[] m_offsets = new Vector3[NECK_PARTS_MAX];

	float m_neckTopOffsetX = 0;

	float m_rolling = 0;

	const int PARTS_HEIGHT = 16;
	const float ROLLING_SCALE = 6.0f;

	int m_growCount = 0;

	// Use this for initialization
	void Start () {
		m_headParts = GameObject.Instantiate<GameObject>(m_headPartsObj);

		for (int i=0; i<NECK_PARTS_MAX; i++)
		{
			m_parts[i] = GameObject.Instantiate<GameObject>(m_neckPartsObj);
			m_parts[i].transform.SetParent(gameObject.transform, false);
		}

		for(int i=0; i<NECK_PARTS_MAX; i++)
		{
			var offset = NECK_PARTS_MAX - i - 1;
			m_positions[i] = new Vector3(0, 0, 0);
			//m_offsets[i] = new Vector3(0, -offset*PARTS_HEIGHT, 0);
			m_offsets[i] = new Vector3(i*2, -offset * PARTS_HEIGHT, 0);
		}
	}

	// Update is called once per frame
	void Update () {
		grow();
		rolling();
		var neckTop = m_parts[NECK_PARTS_MAX - 1];
		var offset = m_neckTopOffsetX;

		if(offset > 0)
		{
			offset = 0;
		}

		m_headParts.transform.position = neckTop.transform.position + new Vector3(8+offset,PARTS_HEIGHT,0);
	}

	void grow()
	{
		if (m_growCount >= PARTS_HEIGHT * (NECK_PARTS_MAX - 1)) return;

		for (int i = 0; i < NECK_PARTS_MAX; i++)
		{
			m_positions[i].y += 1;
			var v = m_positions[i] + m_offsets[i];
			if (v.y < 0)
			{
				v.y = 0;
			}
			m_parts[i].transform.position = v;
		}
		m_growCount++;
	}

	void rolling()
	{
		var rolling = m_rolling;
		var omega = 60.0f;
		var scale = ROLLING_SCALE;

		for (int i = 0; i < NECK_PARTS_MAX; i++)
		{
			var x = m_positions[i].x;

			var parts = m_parts[i];
			var pos = parts.transform.position;

			float n = (float)i / (NECK_PARTS_MAX-1);

			var offset = Mathf.Sin(rolling * Mathf.Deg2Rad) * scale * n;

			x += offset;
			m_neckTopOffsetX = offset;	//	頭をのせるためのぶれ幅のみを保持

			x += m_offsets[i].x;

			pos.x = x;
			parts.transform.position = pos;

			rolling += omega;
		}

		m_rolling += 2;
	}

}
