using UnityEngine;

[System.Serializable]
public struct Vector2Int
{
	public int X { get { return _x; } set { _x = value; } }
	[SerializeField]
	private int _x;
	
	public int Y { get { return _y; } set { _y = value; } }
	[SerializeField]
	private int _y;
	
	public Vector2Int(int x, int y)
	{
		_x = x;
		_y = y;
	}

	public int Multiplied()
	{
		return X * Y;	
	}
	
	public override string ToString()
	{
		return X.ToString() + ", " + Y.ToString();
	}
	
	public static Vector2Int operator +(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.X + b.X, a.Y + b.Y);
	}

	public static Vector2Int operator /(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.X / b.X, a.Y / b.Y);
	}

	public static Vector2Int operator /(Vector2Int a, int b)
	{
		return new Vector2Int(a.X / b, a.Y / b);
	}
}