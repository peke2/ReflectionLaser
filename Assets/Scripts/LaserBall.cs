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
#if false
		GameObject obj, objCenter;
		if( m_laser[m_laserIndex] == null )
		{
			obj = GameObject.Instantiate<GameObject>(m_laserParts[(int)Parts.Head]);
			m_laser[m_laserIndex] = obj;
			obj.transform.SetParent(gameObject.transform, false);

			objCenter = GameObject.Instantiate<GameObject>(m_laserParts[(int)Parts.Head]);
			m_laserCenter[m_laserIndex] = objCenter;
			objCenter.transform.SetParent(gameObject.transform, false);
		}

		obj = m_laser[m_laserIndex];
		var spr = obj.GetComponent<SpriteRenderer>();
		spr.sprite = m_sprites[(int)Parts.Head];

		objCenter = m_laserCenter[m_laserIndex];
		spr = objCenter.GetComponent<SpriteRenderer>();
		spr.sprite = m_sprites[(int)Parts.CenterHead];

		var angle = Vector3.Angle(new Vector3(1, 0, 0), m_dir);
		var o = Vector3.Cross(new Vector3(1, 0, 0), m_dir);
		if (o.z < 0)
		{
			angle = -angle;
		}
		obj.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0,0,1));
		objCenter.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));

		bool isReflected = false;
		var after = m_pos + m_dir * m_speed;
		Vector3 crossPos = Vector3.zero;
		Vector3 n = Vector3.zero;
		if (cross(m_pos, after, ref crossPos, ref n))
		{
			m_pos = crossPos;
			m_dir = Vector3.Reflect(m_dir, n);
			isReflected = true;
		}
		else
		{
			m_pos = after;
		}

		//	1つ前の種類をボディに変更
		var prevIndex = m_laserIndex - 1;
		if (prevIndex < 0)
		{
			prevIndex = LaserLenMax - 1;
		}

		var prevObj = m_laser[prevIndex];
		if (prevObj != null && !m_isBeforeReflected)
		{
			var prevSpr = prevObj.GetComponent<SpriteRenderer>();
			prevSpr.sprite = m_sprites[(int)Parts.Body];
		}

		var prevObjCenter = m_laserCenter[prevIndex];
		if (prevObjCenter != null && !m_isBeforeReflected)
		{
			var prevSpr = prevObjCenter.GetComponent<SpriteRenderer>();
			prevSpr.sprite = m_sprites[(int)Parts.CenterBody];
		}

		//	反射後の1つ手前の絵を変えないようにするため状態を残す
		m_isBeforeReflected = isReflected;

		//	最後尾はテイルに変更
		var lastIndex = (m_laserIndex + 1) % LaserLenMax;
		var lastObj = m_laser[lastIndex];
		if (lastObj != null)
		{
			var lastSpr = lastObj.GetComponent<SpriteRenderer>();
			lastSpr.sprite = m_sprites[(int)Parts.Tail];
		}

		var lastObjCenter = m_laserCenter[lastIndex];
		if (lastObjCenter != null)
		{
			var lastSpr = lastObjCenter.GetComponent<SpriteRenderer>();
			lastSpr.sprite = m_sprites[(int)Parts.CenterTail];
		}

		//	描画が破綻しないようにオーダーを先頭から順番に割り振る
		for (int i=0; i<LaserLenMax; i++)
		{
			var index = (m_laserIndex + i) % LaserLenMax;
			if (!m_laser[index]) continue;
			spr = m_laser[index].GetComponent<SpriteRenderer>();
			spr.sortingOrder = i;

			spr = m_laserCenter[index].GetComponent<SpriteRenderer>();
			spr.sortingOrder = i+LaserLenMax;
		}

		obj.transform.position = m_pos;
		objCenter.transform.position = m_pos;

		m_laserCount++;
		m_laserIndex = (m_laserIndex + 1) % LaserLenMax;
		
		if( m_laserCount >= LaserLenMax )
		{
			m_laserCount = LaserLenMax;
			m_laserIndexTop = (m_laserIndexTop + 1) % LaserLenMax;
		}
#else
		Vector3 pos, dir;
		pos = m_pos;
		dir = m_dir;
		update2(m_sprites, ref pos, ref dir, 1);

		m_pos = pos;
		m_dir = dir;

		/*m_laserCount++;
		m_laserIndex = (m_laserIndex + 1) % LaserLenMax;

		if (m_laserCount >= LaserLenMax)
		{
			m_laserCount = LaserLenMax;
			m_laserIndexTop = (m_laserIndexTop + 1) % LaserLenMax;
		}*/

#endif
	}


	void update2(Sprite[] sprites, ref Vector3 pos, ref Vector3 dir, int order)
	{
		var after = pos + dir * m_speed;
		Vector3 crossPos = Vector3.zero;
		Vector3 n = Vector3.zero;
		if (cross(pos, after, ref crossPos, ref n))
		{
			pos = crossPos;
			dir = Vector3.Reflect(m_dir, n);
		}
		else
		{
			pos = after;
		}

		//	描画が破綻しないようにオーダーを先頭から順番に割り振る
		/*for (int i = 0; i < LaserLenMax; i++)
		{
			var index = (m_laserIndex + i) % LaserLenMax;
			if (!laserObjs[index]) continue;
			spr = laserObjs[index].GetComponent<SpriteRenderer>();
			spr.sortingOrder = order;
		}*/
		/*
		for (int i = 0; i < (int)Parts.Max; i++)
		{
			GameObject obj;
			if (m_laserObjs[i][m_laserIndex] == null)
			{
				obj = GameObject.Instantiate<GameObject>(m_laserParts[0]);
				m_laserObjs[i][m_laserIndex] = obj;
				obj.transform.SetParent(gameObject.transform, false);
				var spr = obj.GetComponent<SpriteRenderer>();
				spr.sprite = sprites[(int)Parts.Ball_0+i];
				spr.sortingOrder = (int)Parts.Max - i;
			}
			else
			{
				obj = m_laserObjs[i][m_laserIndex];
			}
			obj.transform.position = pos;
		}*/


		var p0 = m_laserObjs[0][0].transform.position;
		Vector3 p;
		if( (pos - p0).magnitude > MAX_DISTANCE)
		{
			p = pos - (pos - p0).normalized * MAX_DISTANCE;
		}
		else
		{
			p = p0;
		}

		for (int i = 0; i < (int)Parts.Max; i++)
		{
			m_laserObjs[i][0].transform.position = pos;
		}

		for (int i = 0; i < (int)Parts.Max; i++)
		{
			m_laserObjs[i][1].transform.position = p;
		}

		for (int i = 0; i < (int)Parts.Max; i++)
		{
			for (int j = LaserLenMax-1; j > 1 ; j--)
			{
				//var p0 = m_laserObjs[i][j].transform.position;
				var p1 = m_laserObjs[i][j-1].transform.position;
				var p2 = m_laserObjs[i][j-2].transform.position;

				if ( (p2 - p1).magnitude > MAX_DISTANCE)
				{
					p = p2 - (p2 - p1).normalized * MAX_DISTANCE;
				}
				else
				{
					p = p1;
				}
				m_laserObjs[i][j].transform.position = p;
			}
		}

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
