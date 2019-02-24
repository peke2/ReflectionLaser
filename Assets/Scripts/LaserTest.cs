using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTest : MonoBehaviour {

	enum Parts
	{
		Head,
		Body,
		Tail,
		Max
	}

	GameObject[] m_laserParts = new GameObject[(int)Parts.Max];
	Sprite[] m_sprites = new Sprite[(int)Parts.Max];

	const int LaserLenMax = 16;
	GameObject[] m_laser = new GameObject[LaserLenMax];
	int m_laserIndex = 0;
	int m_laserIndexTop = 0;
	int m_laserCount = 0;

	Vector3 m_dir;
	Vector3 m_pos;
	float m_speed;
	bool m_isBeforeReflected;

	// Use this for initialization
	void Start () {
		m_laserParts[(int)Parts.Head] = Resources.Load<GameObject>("LaserHead_L");
		m_laserParts[(int)Parts.Body] = Resources.Load<GameObject>("LaserBody_L");
		m_laserParts[(int)Parts.Tail] = Resources.Load<GameObject>("LaserTail_L");

		var sprites = Resources.LoadAll<Sprite>("Textures/laser");

		m_sprites[(int)Parts.Head] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "head");
		m_sprites[(int)Parts.Body] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "body");
		m_sprites[(int)Parts.Tail] = System.Array.Find<Sprite>(sprites, sprite => sprite.name == "tail");

		m_pos = new Vector3(10, 10, 0);
		m_dir = new Vector3(-1, 1, 0).normalized;
		m_speed = 4;
		m_isBeforeReflected = false;
	}

	// Update is called once per frame
	void Update () {

		GameObject obj;
		if( m_laser[m_laserIndex] == null )
		{
			obj = GameObject.Instantiate<GameObject>(m_laserParts[(int)Parts.Head]);
			m_laser[m_laserIndex] = obj;
			obj.transform.SetParent(gameObject.transform, false);
		}
		else
		{
			obj = m_laser[m_laserIndex];
			var spr = obj.GetComponent<SpriteRenderer>();
			spr.sprite = m_sprites[(int)Parts.Head];
		}

		var angle = Vector3.Angle(new Vector3(1, 0, 0), m_dir);
		var o = Vector3.Cross(new Vector3(1, 0, 0), m_dir);
		if (o.z < 0)
		{
			angle = -angle;
		}
		obj.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0,0,1));

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

		//	描画が破綻しないようにオーダーを先頭から順番に割り振る
		for(int i=0; i<LaserLenMax; i++)
		{
			var index = (m_laserIndex + i) % LaserLenMax;
			if (!m_laser[index]) continue;
			var spr = m_laser[index].GetComponent<SpriteRenderer>();
			spr.sortingOrder = i;
		}

		obj.transform.position = m_pos;

		m_laserCount++;
		m_laserIndex = (m_laserIndex + 1) % LaserLenMax;
		
		if( m_laserCount >= LaserLenMax )
		{
			m_laserCount = LaserLenMax;
			m_laserIndexTop = (m_laserIndexTop + 1) % LaserLenMax;
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
