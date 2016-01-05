using UnityEngine;

namespace Partak
{
    static public class CellUtility
	{
		static public Vector2Int GridIndexToCoordinate(int index, Vector2Int max)
		{       
			return GridIndexToCoordinate(index, max.X, max.Y);
		}

		static public Vector2Int GridIndexToCoordinate(int index, int xMax, int yMax)
		{       
			if (index >= xMax * yMax || index < 0)
			{
				return new Vector2Int(-1, -1);
			}
			Vector2Int coordinate = new Vector2Int();
			coordinate.Y = (int)(index / xMax);
			coordinate.X = index % xMax;
			return coordinate;
		}

		static public int CoordinateToGridIndex(Vector2Int coordinate, Vector2Int max)
		{       
			return CoordinateToGridIndex(coordinate.X, coordinate.Y, max.X, max.Y);
		}

		static public int CoordinateToGridIndex(int x, int y, Vector2Int max)
		{       
			return CoordinateToGridIndex(x, y, max.X, max.Y);
		}

		static public int CoordinateToGridIndex(int x, int y, int xMax, int yMax)
		{       
			if (x >= xMax || y >= yMax || x < 0 || y < 0)
			{
				return -1;
			}
			return ((y * xMax) + x);
		}

		static public int WorldPositionToGridIndex(float x, float y, Vector2Int max)
		{		
			//for some reason if it went negative build gradient thread would crash, not sure why, shouldn't be this way
			if (x < 0)
			{
				x = 0;
			}
			if (y < 0)
			{
				y = 0;
			}

			if (x > ((float)max.X - 1f) / 10f)
			{
				x = ((float)max.X - 1f) / 10f;
			}
			if (y > ((float)max.Y - 1f) / 10f)
			{
				y = ((float)max.Y - 1f) / 10f;
			}

			int gridX = Mathf.RoundToInt(x * 10f);
			int gridY = Mathf.RoundToInt(y * 10f);		

			return CoordinateToGridIndex(gridX, gridY, max);
		}

		static public int InvertDirection(int direction)
		{
			return RotateDirection(direction, 6, 12);
		}

		static public int RotateDirection(int direction, int rotation)
		{
			return RotateDirection(direction, rotation, 12);
		}
		
		static public int RotateDirection(int direction, int rotation, int drectionCount)
		{
			direction += rotation;
			if (direction > drectionCount - 1)
				direction = direction - drectionCount;
			else if (direction < 0)
				direction = drectionCount + direction;	
			return direction;
		}

		/*
		Directions:
		0 - NNE
		1 - NE
		2 - NEE
		3 - SEE
		4 - SE
		5 - SSE
		6 - SSW
		7 - SW
		8 - SWW
		9 - NWW
		10 - NW
		11 - NNW		
		*/
		static public Vector2Int DirectionToXY(int direction)
		{
			Vector2Int vector2Int = new Vector2Int();
			switch (direction)
			{
			case 0:
				//NNE
				vector2Int.X = 0;
				vector2Int.Y = 1;
				break;
				
			case 1:
				//NE
				vector2Int.X = 1;
				vector2Int.Y = 1;					
				break;
				
			case 2:
				//NEE
				vector2Int.X = 1;
				vector2Int.Y = 0;						
				break;
				
			case 3:
				//SEE
				vector2Int.X = 1;
				vector2Int.Y = 0;							
				break;
				
			case 4:
				//SE
				vector2Int.X = 1;
				vector2Int.Y = -1;						
				break;
				
			case 5:
				//SSE
				vector2Int.X = 0;
				vector2Int.Y = -1;						
				break;
				
			case 6:
				//SSW
				vector2Int.X = 0;
				vector2Int.Y = -1;						
				break;
				
			case 7:
				//SW
				vector2Int.X = -1;
				vector2Int.Y = -1;						
				break;

			case 8:
				//SWW
				vector2Int.X = -1;
				vector2Int.Y = 0;						
				break;

			case 9:
				//NWW
				vector2Int.X = -1;
				vector2Int.Y = 0;						
				break;

			case 10:
				//NW
				vector2Int.X = -1;
				vector2Int.Y = 1;						
				break;

			case 11:
				//NW
				vector2Int.X = 0;
				vector2Int.Y = 1;						
				break;
				
			default:
				vector2Int.X = 0;
				vector2Int.Y = 0;
				break;			
			}	
			return vector2Int;		
		}
	}
}