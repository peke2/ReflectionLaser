using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBall : MonoBehaviour {

	enum Parts
	{
		Ball_0,
		Ball_1,
		Ball_2,
		Ball_3,
		Max
	}

	public Vector2 m_direction = new Vector2(1,1);
	public float m_speed = 2.0f;

	GameObject[] m_laserParts = new GameObject[(int)Parts.Max];
	Sprite[] m_sprites = new Sprite[(int)Parts.Max];

	const int LaserLenMax = 16;
	//const int LaserLenMax = 24;
	GameObject[][] m_laserObjs = new GameObject[][] {
		new GameObject[LaserLenMax],
		new GameObject[LaserLenMax],
		new GameObject[LaserLenMax],
		new GameObject[LaserLenMax],
	};
	int m_laserIndex = 0;
	int m_laserIndexTop = 0;
	int m_laserCount = 0;

	Vector3 m_dir;
	Vector3 m_pos;
	bool m_isBeforeReflected;

	const float MAX_DISTANCE = 3.0f;

	// Use this for initialization
	void Start () {
		m_laserParts[0] = Resources.Load<GameObject>("LaserHead_L");

		var sprites = Resources.LoadAll<Sprite>("Textures/laser");

		m_sprites[(int)Parts.Ball_0] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "ball_0");
		m_sprites[(int)Parts.Ball_1] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "ball_1");
		m_sprites[(int)Parts.Ball_2] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "ball_2");
		m_sprites[(int)Parts.Ball_3] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "ball_3");

		m_pos = gameObject.transform.position;
		m_dir = m_direction.normalized;
		m_isBeforeReflected = false;

		//	リソースを用意
		for (int i = 0; i < (int)Parts.Max; i++)
		{
			for(int j=0; j<LaserLenMax; j++)
			{
				var obj = GameObject.Instantiate<GameObject>(m_laserParts[0]);
				m_laserObjs[i][j] = obj;
				obj.transform.SetParent(gameObject.transform, false);
				var spr = obj.GetComponent<SpriteRenderer>();
				spr.sprite = sprites[(int)Parts.Ball_0 + i];
				spr.sortingOrder = (int)Parts.Max - i;
				obj.transform.position = m_pos;
			}
		}

	}

	// Update is called once per frame
	void Update () {
		Vector3 pos, dir;
		pos = m_pos;
		dir = m_dir;
		update2(m_sprites, ref pos, ref dir, 1);

		m_pos = pos;
		m_dir = dir;

	/*	m_laserCount++;
		m_laserIndex = (m_laserIndex + 1) % LaserLenMax;

		if (m_laserCount >= LaserLenMax)
		{
			m_laserCount = LaserLenMax;
			m_laserIndexTop = (m_laserIndexTop + 1) % LaserLenMax;
		}*/
	}


	void update2(Sprite[] sprites, ref Vector3 pos, ref Vector3 dir, int order)
	{
		//	次の移動座標を算出
		var after = pos + dir * m_speed;
		Vector3 crossPos = Vector3.zero;
		Vector3 n = Vector3.zero;
		if (cross(pos, after, ref crossPos, ref n))
		{
			after = crossPos;
			dir = Vector3.Reflect(m_dir, n);
		}
		/*
		for (int i = 0; i < (int)Parts.Max; i++)
		{
			var obj = m_laserObjs[i][m_laserIndex];
			obj.transform.position = pos;
		}*/

		//	今の先頭との距離を出す
		var diff = after - pos;
		float num = diff.magnitude / MAX_DISTANCE;
		var resCount = (int)Mathf.Ceil(diff.magnitude / MAX_DISTANCE);

		if( resCount == 2 )
		{
			Debug.Log("");
		}

		//	最大間隔が何個分か求める
		//	リソースの最大数を超えない数に抑える
		resCount = Mathf.Clamp(resCount, 1, LaserLenMax);

		var d = diff.normalized;

		//	一番遠い距離からリソースと座標を割り当てる
		for(int i=resCount-1; i>=0; i--)
		{
			var p = after - d * MAX_DISTANCE * i;
			m_laserObjs[0][m_laserIndexTop].transform.position = p;
			m_laserObjs[1][m_laserIndexTop].transform.position = p;
			m_laserObjs[2][m_laserIndexTop].transform.position = p;
			m_laserObjs[3][m_laserIndexTop].transform.position = p;
			m_laserIndexTop = (m_laserIndexTop + 1) % LaserLenMax;
		}

		pos = after;
	}


	Vector3 m_wallLt = new Vector3(0, 200, 0);
	Vector3 m_wallRb = new Vector3(256, 0, 0);

	bool cross(Vector3 pos, Vector3 newPos, ref Vector3 cross, ref Vector3 n)
	{
		//左右
		//線をまたいで頂点が存在するか？
		float x;
		float y;
		var y0 = m_wallRb.y;
		var y1 = m_wallLt.y;

		x = m_wallLt.x;

		if ( Mathf.Sign((pos.x - x) * (newPos.x - x)) < 0)
		{
			var diff = newPos - pos;
			if( Mathf.Abs(diff.x) > Mathf.Epsilon )
			{
				var a = diff.y / diff.x;
				var b = pos.y - a * pos.x;

				y = a * x + b;
				if( y >= y0 && y<=y1 )
				{
					n = new Vector3(1, 0, 0);
					cross = new Vector3(x, y, 0);
					return true;
				}
			}
		}

		x = m_wallRb.x;
		if (Mathf.Sign((pos.x - x) * (newPos.x - x)) < 0)
		{
			var diff = newPos - pos;
			if (Mathf.Abs(diff.x) > Mathf.Epsilon)
			{
				var a = diff.y / diff.x;
				var b = pos.y - a * pos.x;

				y = a * x + b;
				if (y >= y0 && y <= y1)
				{
					n = new Vector3(-1, 0, 0);
					cross = new Vector3(x, y, 0);
					return true;
				}
			}
		}

		y = m_wallLt.y;
		var x0 = m_wallLt.x;
		var x1 = m_wallRb.x;

		if (Mathf.Sign((pos.y - y) * (newPos.y - y)) < 0)
		{
			var diff = newPos - pos;
			if (Mathf.Abs(diff.x) > Mathf.Epsilon)
			{
				var a = diff.y / diff.x;
				var b = pos.y - a * pos.x;

				x = (y- b)/a;
				if (x >= x0 && x <= x1)
				{
					n = new Vector3(0, -1, 0);
					cross = new Vector3(x, y, 0);
					return true;
				}
			}
			else
			{
				if( pos.x >= x0 && pos.x <= x1 )
				{
					n = new Vector3(0, -1, 0);
					cross = new Vector3(x, y, 0);
					return true;
				}
			}
		}

		y = m_wallRb.y;
		if (Mathf.Sign((pos.y - y) * (newPos.y - y)) < 0)
		{
			var diff = newPos - pos;
			if (Mathf.Abs(diff.x) > Mathf.Epsilon)
			{
				var a = diff.y / diff.x;
				var b = pos.y - a * pos.x;

				x = (y - b) / a;
				if (x >= x0 && x <= x1)
				{
					n = new Vector3(0, -1, 0);
					cross = new Vector3(x, y, 0);
					return true;
				}
			}
			else
			{
				if (pos.x >= x0 && pos.x <= x1)
				{
					n = new Vector3(0, 1, 0);
					cross = new Vector3(x, y, 0);
					return true;
				}
			}
		}

		return false;
	}

}
