using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Shot : MonoBehaviour {

	protected Vector2 m_position;
	protected Vector2 m_direction;
	protected List<Vector2> m_positions;

	protected float m_speed = 1;

	protected GameObject m_stageObj;

	protected Vector2 m_size = new Vector2(4, 4);

	protected const float STAGE_CELL_SIZE = 8;

	private void Awake()
	{
		createLineMaterial();
	}

	// Use this for initialization
	void Start() {
		m_stageObj = GameObject.Find("Tilemap");
	}

	// Update is called once per frame
	void Update() {
		//move();
		move0();
	}

	public void setInfo(Vector2 pos, Vector2 dir, float speed)
	{
		//gameObject.transform.Translate(pos.x, pos.y, 0);
		m_position = pos;
		m_direction = dir.normalized;
		m_speed = speed;

		gameObject.transform.position = m_position;
	}


	protected void move0()
	{
		var tileMap = m_stageObj.GetComponent<Tilemap>();
		if (tileMap == null)
		{
			return;
		}

		var mv = m_direction * m_speed;
		var pos = m_position + mv;
		int bx = (int)(pos.x / STAGE_CELL_SIZE);
		int by = (int)(pos.y / STAGE_CELL_SIZE);
		Vector2 blockBasePos0 = new Vector2(bx * STAGE_CELL_SIZE, by * STAGE_CELL_SIZE);
		Vector2 blockBasePos1 = blockBasePos0 + new Vector2(STAGE_CELL_SIZE, STAGE_CELL_SIZE);

		int ax = (int)(m_position.x / STAGE_CELL_SIZE);
		int ay = (int)(m_position.y / STAGE_CELL_SIZE);

		int x0 = Mathf.Min(ax, bx);
		int x1 = Mathf.Max(ax, bx);
		int y0 = Mathf.Min(ay, by);
		int y1 = Mathf.Max(ay, by);

		bool isHit = false;
		float minLength = float.MaxValue;
		Vector2 resultPos = Vector2.zero;
		Vector2 resultDir = Vector2.zero;

		for (int y=y0; y<=y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				Vector2 infoPos, infoDir;
				if( move1(tileMap, x, y, out infoPos, out infoDir) )
				{
					float len = Vector2.Distance(m_position, infoPos);

					var str = string.Format("x={0} y={1}", x,y);
					Debug.Log(str);

					str = string.Format("pos=[{0},{1}] -> [{2},{3}]", m_position.x, m_position.y, infoPos.x, infoPos.y);
					Debug.Log(str);

					str = string.Format("dir=[{0},{1}] -> [{2},{3}]", m_direction.x, m_direction.y, infoDir.x, infoDir.y);
					Debug.Log(str);

					//	反射先にタイルが無いことも判定する
					var tilePos = infoPos / STAGE_CELL_SIZE;
					Vector3Int tp = new Vector3Int( (int)tilePos.x, (int)tilePos.y, 0);
					var tile = tileMap.GetTile(tp);

					if ( len < minLength && tile == null)
					{
						isHit = true;

						minLength = len;
						resultPos = infoPos;
						resultDir = infoDir;
					}
				}
			}

		}

		if( isHit )
		{
			m_position = resultPos;
			m_direction = resultDir;
		}
		else
		{
			m_position = pos;
		}

		gameObject.transform.position = m_position;
	}

	bool move1(Tilemap tileMap, int bx, int by, out Vector2 infoPos, out Vector2 infoDir)
	{
		var mv = m_direction * m_speed;
		var pos = m_position + mv;
		Vector2 blockBasePos0 = new Vector2(bx * STAGE_CELL_SIZE, by * STAGE_CELL_SIZE);
		Vector2 blockBasePos1 = blockBasePos0 + new Vector2(STAGE_CELL_SIZE, STAGE_CELL_SIZE);

		Vector3Int tp = new Vector3Int(bx, by, 0);
		var tile = tileMap.GetTile(tp);

		infoPos = Vector2.zero;
		infoDir = Vector2.zero;

		bool cross = false;

		if( tile == null )
		{
			return false;
		}

		//	入射方向と逆にして、直感的にどちらから来たかわかるようにする
		var xn = new Vector2(1, 0);
		var yn = new Vector2(0, 1);

		//	どちらの方向から入ってきたかの判断
		var dx = Vector2.Dot(-mv, xn);
		var dy = Vector2.Dot(-mv, yn);

		Vector2 p0, p1;
		p0 = m_position;
		p1 = pos;
		//	左右の直線(壁)をx=0とみなす
		if (dx > 0)
		{
			//	右
			p0.x = p0.x - blockBasePos1.x;
			p1.x = p1.x - blockBasePos1.x;
		}
		else if (dx < 0)
		{
			//	左
			p0.x = p0.x - blockBasePos0.x;
			p1.x = p1.x - blockBasePos0.x;
		}

		float a = p1.x - p0.x;
		float b;
		float x, y;
		Vector2 n;
		//	傾きがあること(垂直は交わらない)
		//	始点と終点が壁をまたいでいること
		if (Mathf.Abs(a) > 0.0005f && p0.x*p1.x<0)
		{
			var str = string.Format("p0.x={0},p1.x={1} / dx={2}", p0.x, p1.x, dx);
			Debug.Log(str);

			//	入射線の傾き
			a = (p1.y - p0.y) / a;
			//	x = 0 の時の y 座標 = 左右の直線との交点
			b = p0.y - a * p0.x;

			if (b >= blockBasePos0.y && b < blockBasePos1.y)
			{
				if (dx > 0)
				{
					//	右側
					//x = blockBasePos0.x + STAGE_CELL_SIZE - 1;
					x = blockBasePos0.x + STAGE_CELL_SIZE;

					n = new Vector2(1, 0);
					infoDir = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
				}
				else
				{
					//	左側
					//x = blockBasePos0.x;
					x = blockBasePos0.x - 1;

					n = new Vector2(-1, 0);
					infoDir = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
				}
				y = b;
				//m_position.x = x;
				//m_position.y = y;
				infoPos.x = x;
				infoPos.y = y;
				//cross = true;
				return true;
			}
		}


		p0 = m_position;
		p1 = pos;
		//	上下の直線(壁)を y=0 とみなす
		if (dy > 0)
		{
			//	上
			p0.y = p0.y - blockBasePos1.y;
			p1.y = p1.y - blockBasePos1.y;
		}
		else if (dy < 0)
		{
			//	下
			p0.y = p0.y - blockBasePos0.y;
			p1.y = p1.y - blockBasePos0.y;
		}

		//	傾き0は上下の直線と平行なので事前に確認(そもそも0除算になる)
		//	始点と終点が壁をまたいでいること
		a = p1.x - p0.x;
		if ( p0.y * p1.y < 0)
		{
			if (Mathf.Abs(a) > 0.0005f)
			{
				a = (p1.y - p0.y) / a;
				b = p0.y - a * p0.x;
				if (Mathf.Abs(a) > 0.0005f)
				{
					//	y=0の時のx座標
					float x0;
					x0 = -b / a;

					if (x0 >= blockBasePos0.x && x0 < blockBasePos1.x)
					{
						var str = string.Format("p0.y={0},p1.y={1} / dy={2}", p0.y, p1.y, dy);
						Debug.Log(str);

						x = x0;
						if (dy > 0)
						{
							//	上側
							//y = blockBasePos0.y + STAGE_CELL_SIZE - 1;
							y = blockBasePos0.y + STAGE_CELL_SIZE;

							n = new Vector2(0, 1);
							infoDir = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
						}
						else
						{
							//	下側
							//y = blockBasePos0.y;
							y = blockBasePos0.y-1;

							n = new Vector2(0, -1);
							infoDir = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
						}
						infoPos.x = x;
						infoPos.y = y;
						cross = true;
					}
				}
			}
			else
			{
				//	垂直
				if (p0.x >= blockBasePos0.x && p0.x < blockBasePos1.x)
				{
					var str = string.Format("[v] p0.y={0},p1.y={1} / dy={2}", p0.y, p1.y, dy);
					Debug.Log(str);

					x = p0.x;
					if (dy > 0)
					{
						//	上側
						//y = blockBasePos0.y + STAGE_CELL_SIZE - 1;
						y = blockBasePos0.y + STAGE_CELL_SIZE;

						n = new Vector2(0, 1);
						infoDir = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
					}
					else
					{
						//	下側
						//y = blockBasePos0.y;
						y = blockBasePos0.y - 1;

						n = new Vector2(0, -1);
						infoDir = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
					}
					infoPos.x = x;
					infoPos.y = y;
					cross = true;
				}
			}
		}

		//gameObject.transform.position = new Vector3(m_position.x, m_position.y, 0);
		return cross;
	}

	void move()
	{
		var mv = m_direction * m_speed;
		var pos = m_position + mv;
		int bx = (int)(pos.x / STAGE_CELL_SIZE);
		int by = (int)(pos.y / STAGE_CELL_SIZE);
		Vector2 blockBasePos0 = new Vector2(bx * STAGE_CELL_SIZE, by * STAGE_CELL_SIZE);
		Vector2 blockBasePos1 = blockBasePos0 + new Vector2(STAGE_CELL_SIZE, STAGE_CELL_SIZE);

		//	ブロック内で当たるものがあるか？
		var tileMap = m_stageObj.GetComponent<Tilemap>();
		if(tileMap == null)
		{
			return;
		}

		Vector3Int tp = new Vector3Int(bx, by, 0);
		var tile = tileMap.GetTile(tp);
		//Tilemap tile = null;

		if(tile != null)
		{
			//	入射方向と逆にして、直感的にどちらから来たかわかるようにする
			var xn = new Vector2(1, 0);
			var yn = new Vector2(0, 1);

			//	どちらの方向から入ってきたかの判断
			var dx = Vector2.Dot(-mv, xn);
			var dy = Vector2.Dot(-mv, yn);

			Vector2 p0, p1;
			p0 = m_position;
			p1 = pos;
			//	左右の直線(壁)をx=0とみなす
			if(dx > 0)
			{
				//	右
				p0.x = p0.x - blockBasePos1.x;
				p1.x = p1.x - blockBasePos1.x;
			}
			else if(dx < 0)
			{
				//	左
				p0.x = p0.x - blockBasePos0.x;
				p1.x = p1.x - blockBasePos0.x;
			}

			float a = p1.x - p0.x;
			float b;
			float x, y;
			Vector2 n;
			//	傾きがあること(垂直は交わらない)
			if(Mathf.Abs(a) > 0.0005f)
			{
				//	入射線の傾き
				a = (p1.y - p0.y) / a;
				//	x = 0 の時の y 座標 = 左右の直線との交点
				b = p0.y - a * p0.x;

				if(b >= blockBasePos0.y && b < blockBasePos1.y)
				{
					if(dx > 0)
					{
						//	右側
						x = blockBasePos0.x + STAGE_CELL_SIZE;

						n = new Vector2(1, 0);
						m_direction = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
					}
					else
					{
						//	左側
						x = blockBasePos0.x-1;

						n = new Vector2(-1, 0);
						m_direction = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
					}
					y = b;
					m_position.x = x;
					m_position.y = y;
				}
			}


			p0 = m_position;
			p1 = pos;
			//	上下の直線(壁)を y=0 とみなす
			if(dy > 0)
			{
				//	上
				p0.y = p0.y - blockBasePos1.y;
				p1.y = p1.y - blockBasePos1.y;
			}
			else if(dy < 0)
			{
				//	下
				p0.y = p0.y - blockBasePos0.y;
				p1.y = p1.y - blockBasePos0.y;
			}

			//	傾き0は上下の直線と平行なので事前に確認(そもそも0除算になる)
			a = p1.x - p0.x;
			if(Mathf.Abs(a) > 0.0005f)
			{
				a = (p1.y - p0.y) / a;
				b = p0.y - a * p0.x;
				if(Mathf.Abs(a) > 0.0005f)
				{
					float x0;
					x0 = - b / a;

					if(x0 >= blockBasePos0.x && x0 < blockBasePos1.x)
					{
						x = x0;
						if(dy > 0)
						{
							//	上側
							y = blockBasePos0.y + STAGE_CELL_SIZE - 1;

							n = new Vector2(0, 1);
							m_direction = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
						}
						else
						{
							//	下側
							y = blockBasePos0.y;

							n = new Vector2(0, -1);
							m_direction = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
						}
						m_position.x = x;
						m_position.y = y;
					}
				}
			}
			else
			{
				//	垂直
				if(p0.x >= blockBasePos0.x && p0.x < blockBasePos1.x)
				{
					x = p0.x;
					if(dy > 0)
					{
						//	上側
						y = blockBasePos0.y + STAGE_CELL_SIZE - 1;

						n = new Vector2(0, 1);
						m_direction = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
					}
					else
					{
						//	下側
						y = blockBasePos0.y;

						n = new Vector2(0, -1);
						m_direction = m_direction - 2 * Vector2.Dot(n, m_direction) * n;
					}
					m_position.x = x;
					m_position.y = y;
				}
			}
		}
		else
		{
			m_position = pos;
		}

		
		gameObject.transform.position = new Vector3(m_position.x, m_position.y, 0);

	}

	static Material lineMaterial;
	static void createLineMaterial()
	{
		if(lineMaterial)
		{
			return;
		}

		Shader shader = Shader.Find("Hidden/Internal-Colored");
		lineMaterial = new Material(shader);
		lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
		lineMaterial.SetInt("_ZWrite", 0);
	}

	private void OnRenderObject()
	{
		lineMaterial.SetPass(0);
		GL.PushMatrix();
		//GL.MultMatrix(transform.localToWorldMatrix);
		GL.MultMatrix(Matrix4x4.identity);
		GL.Begin(GL.LINES);
		GL.Color(new Color(1, 0, 0, 1));
		GL.Vertex3(m_position.x, m_position.y, 0);
		GL.Vertex3(m_position.x + m_direction.x*8, m_position.y + m_direction.y*8, 0);
		GL.End();
		GL.PopMatrix();

	}

}
