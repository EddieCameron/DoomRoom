/* CellMaster.cs
 * Copyright Eddie Cameron 2012
 * ----------------------------
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellMaster : MonoBehaviour 
{
    public float wallDensity = 0.5f;

    public LayerMask pathAffectingLayers;

	public float _areaSize = 39;
	public float _cellSize = 3;

	public static float areaSize { get { return instance._areaSize; } }
	public static float cellSize { get { return instance._cellSize; } }
	public static List<List<Cell>> cellsByRoom{ get; private set; }
	public static int CellsPerSide{ get{ return cells.GetLength ( 0 ); } }
	public static Rect GetBoundary{
		get{
			return new Rect( - areaSize / 2f, -areaSize / 2f, areaSize, areaSize );
		}
	}
	
	static CellMaster instance;
	static Cell[,] cells;
	
	List<Cell> unassignedRooms = new List<Cell>();
	
	void Awake()
	{
		if ( instance )
		{
			Destroy( this );
			return;
		}
		else
		{
			instance = this;
			
			cellsByRoom = new List<List<Cell>>();
			
			int numCells = Mathf.FloorToInt ( areaSize / cellSize );
			instance._areaSize = numCells * cellSize;
			
			cells = new Cell[ numCells, numCells ];
			for ( int x = 0; x < numCells; x++ )
			{
				for ( int y = 0; y < numCells; y++ )
				{
					Cell newCell = new Cell( x, y );
					if ( x == 0 )
						newCell.canGoWest = false;
					if ( x == numCells - 1)
						newCell.canGoEast = false;
					if ( y == 0 )
						newCell.canGoSouth = false;
					if ( y == numCells - 1 )
						newCell.canGoNorth = false;
					
					cells[x,y] = newCell;
				}
			}

            GenLayout ();
		}
	}

	void Start()
	{
	}
	
	void OnDisable()
	{
		if ( instance == this )
			instance = null;
	}
	
	public static Cell GetCellAt( int x, int y )
	{
		x = Mathf.Clamp ( x, 0, CellsPerSide - 1 );
		y = Mathf.Clamp ( y, 0, CellsPerSide - 1 );
		return cells[x,y];
	}
	
	public static Cell GetCellAt( Vector3 pos )
	{
		int x = Mathf.Clamp ( Mathf.FloorToInt ( pos.x / cellSize + CellsPerSide * 0.5f ), 0, CellsPerSide - 1 );
		int y = Mathf.Clamp ( Mathf.FloorToInt( pos.z / cellSize + CellsPerSide * 0.5f ), 0, CellsPerSide - 1 );
		if ( x >= 0 && x < CellsPerSide && y >= 0 && y < CellsPerSide )
			return cells[x,y];
		
		return new Cell( -1, -1 );
	}

    public static LayerMask GetPathAffectingLayers()
    {
        return instance.pathAffectingLayers;
    }
    
    static void PrintLayout()
    {
        /// Print room layout to console
        Debug.Log ( "Room layout" );
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for ( int y = CellsPerSide - 1; y >= 0; y-- )
        {
            for ( int x = 0; x < CellsPerSide; x++ )
                sb.Append ( " " + (cells[x,y].canGoNorth ? " " : "#") + " " );
            sb.AppendLine ();
            for ( int x = 0; x < CellsPerSide; x++ )
                sb.Append ( ( cells[x,y].canGoWest ? " " : "#" )+ cells[x,y].room.ToString ( "d2" ) );
            sb.AppendLine ();
        }
        Debug.Log ( sb.ToString() );
        /// 
    }
	
	#region Room Gen
	void GenLayout()
	{
		int minRoomSize = 3;
		int maxRoomSize = 6;
		
		foreach( Cell c in cells )
			unassignedRooms.Add ( c );
		
		Cell startRoom = null;
		int curRoom = 0;
		while( unassignedRooms.Count > 0 )
		{
			// add room of random size
			int nextRoomSize = curRoom == 0 ? maxRoomSize : Random.Range ( minRoomSize, maxRoomSize + 1 );
			
			Cell centreCell = unassignedRooms[ Random.Range ( 0, unassignedRooms.Count ) ];
			if ( startRoom == null )
				startRoom =  centreCell;
			
			int startX = centreCell.x - Mathf.CeilToInt ( nextRoomSize / 2f ) + 1;
			int endX = Mathf.Min ( startX + nextRoomSize, CellsPerSide );
			startX = Mathf.Max ( 0, startX );
			
			int startY = centreCell.y - Mathf.CeilToInt ( nextRoomSize / 2f ) + 1;
			int endY = Mathf.Min ( startY + nextRoomSize, CellsPerSide );
			startY = Mathf.Max( 0, startY );
			
			var roomCells = new List<Cell>();
			
			var lastInRoom = new List<int>();	// which rows in the last column had a room on it? If no rows match, column won't be inited and room will stop. Avoids split rooms
			for( int x = startX; x < endX; x++ )
			{
				var cellsThisColumn = new List<int>();
				if ( lastInRoom.Count == 0 )
				{
					for ( int y = startY; y < endY; y++ )
						if ( cells[x,y].room == -1 )
							cellsThisColumn.Add ( y );
				}
				else
				{
					// add last column's rooms to this column if valid, then spread up and down until hits another room
					foreach( int roomRow in lastInRoom )
					{
						if ( !cellsThisColumn.Contains ( roomRow ) && cells[x,roomRow].room == -1 )
						{
							cellsThisColumn.Add ( roomRow );
							for ( int south = roomRow - 1; south >= startY; south-- )
							{
								if ( cells[x,south].room == -1 )
									cellsThisColumn.Add ( south );
								else
									break;
							}
							for ( int north = roomRow + 1; north < endY; north++ )
							{
								if ( cells[x,north].room == -1 )
									cellsThisColumn.Add ( north );
								else
									break;
							}
						}
					}
					
					// if no valid connection after room has started, stop making room
					if ( cellsThisColumn.Count == 0 )
						break;
				}
				
				// actually make rooms
				foreach( int row in cellsThisColumn )
				{
					// for each cell within room edges, add walls between neighbouring rooms (if not in another room already)
					// add each valid room to list, and if can't path to first room after all rooms done, make holes
					Cell roomCell = cells[x,row];
					if ( AddCellToRoom ( roomCell, curRoom ) )
					{
						roomCells.Add ( roomCell );
					}
				}
				lastInRoom = cellsThisColumn;
			}
			
			Debug.Log ( "Room made" );
			PrintLayout ();
			
			// try to path to start room
			if ( roomCells.Count > 0 && CellPathfinder.PathTo ( startRoom.centrePosition, roomCells[0].centrePosition ) == null )
			{
				// no path, make corridor to first cell
				Cell pathEnd = null;
				int distToTarg = int.MaxValue;
				foreach ( Cell edgeCell in roomCells )
				{
					int newDist = Mathf.Abs( edgeCell.x - startRoom.x ) + Mathf.Abs ( edgeCell.y - startRoom.y );
					if ( newDist < distToTarg )
					{
						distToTarg = newDist;
						pathEnd = edgeCell;
					}
				}
				
				while ( pathEnd.room == curRoom )
				{
					Debug.Log( "Opening path from " + pathEnd );
					int xDist = startRoom.x - pathEnd.x;
					int yDist = startRoom.y - pathEnd.y;
					if ( xDist >= Mathf.Abs ( yDist ) )
						pathEnd = OpenCellInDirection ( pathEnd, Direction.East );
					else if ( xDist <= -Mathf.Abs ( yDist ) )
						pathEnd = OpenCellInDirection( pathEnd, Direction.West );
					else if ( yDist > Mathf.Abs ( xDist ) )
						pathEnd = OpenCellInDirection( pathEnd, Direction.North );
					else if ( yDist < -Mathf.Abs( xDist ) )
						pathEnd = OpenCellInDirection( pathEnd, Direction.South );
				}		
				
				// check if can path. JUST IN CASE
				if ( CellPathfinder.PathTo ( startRoom.centrePosition, roomCells[0].centrePosition ) == null )
				{
					Debug.LogWarning ( "Still no path from room " + curRoom );
					PrintLayout ();
				}
			}
		
			curRoom++;
		}
		
		Debug.Log( "Layout complete..." );
		PrintLayout();
	}
	
	/// <summary>
	/// Opens the cell in direction.
	/// </summary>
	/// <param name='toOpen'>
	/// To open.
	/// </param>
	/// <param name='inDir'>
	/// In dir.
	/// </param>
	Cell OpenCellInDirection( Cell toOpen, Direction inDir )
	{
		Debug.Log ( "Opening cell " + toOpen + " in direction " + inDir );
		Cell nextCell = toOpen.GetCellInDirection ( inDir );
						
		if ( !toOpen.CanGoInDirection( inDir ) )
		{
			toOpen.SetOpenInDirection( inDir, true );
			if ( nextCell.room == -1 )
				Debug.LogWarning ( "Wall made to uninitialised cell" );
			nextCell.SetOpenInDirection ( Cell.ReverseDirection( inDir ), true );
			// made connection to another room. Stop.
		}
		else
		{
			// add uninited cell towards startCell to current room
			if ( nextCell.room != -1 )
			{
				Debug.LogWarning ( "No path to start, but can reach another room???" );
			}
			else
			{
				AddCellToRoom ( nextCell, toOpen.room );
			}
		}
		return nextCell;
	}
		
	/// <summary>
	/// Adds the cell to given room.
	/// </summary>
	/// <returns>
	/// Whether the cell was added
	/// </returns>
	/// <param name='newCell'>
	/// Cell to add
	/// </param>
	/// <param name='inRoom'>
	/// Room number to add to
	/// </param>
	bool AddCellToRoom( Cell newCell, int inRoom )
	{
		// Add walls between this and cells that have been set to other rooms
		if ( newCell.room == -1 )
		{
			newCell.room = inRoom;
			if ( newCell.x > 0 && newCell.GetCellInDirection ( Direction.West ).room != -1 && newCell.GetCellInDirection ( Direction.West ).room != inRoom && Random.value < wallDensity )
				newCell.canGoWest = newCell.GetCellInDirection( Direction.West ).canGoEast = false;
			if ( newCell.x < CellsPerSide - 1 && newCell.GetCellInDirection ( Direction.East ).room != -1 && newCell.GetCellInDirection ( Direction.East ).room != inRoom && Random.value < wallDensity )
				newCell.canGoEast = newCell.GetCellInDirection ( Direction.East ).canGoWest = false;
			if ( newCell.y > 0 && newCell.GetCellInDirection ( Direction.South ).room != -1 && newCell.GetCellInDirection ( Direction.South ).room != inRoom && Random.value < wallDensity )
				newCell.canGoSouth = newCell.GetCellInDirection ( Direction.South ).canGoNorth = false;
			if ( newCell.y < CellsPerSide - 1 && newCell.GetCellInDirection ( Direction.North ).room != -1 && newCell.GetCellInDirection ( Direction.North ).room != inRoom && Random.value < wallDensity )
				newCell.canGoNorth = newCell.GetCellInDirection ( Direction.North ).canGoSouth = false;	
			
			unassignedRooms.Remove ( newCell );
			
			while ( inRoom >= cellsByRoom.Count )
				cellsByRoom.Add ( new List<Cell>() );
			cellsByRoom[inRoom].Add( newCell );
			
			return true;
		}
		
		return false;
	}
	#endregion
		
	static Direction RandomDirection()
	{
		float a = Random.value;
		if ( a < 0.25f )
			return Direction.North;
		if ( a < 0.5f )
			return Direction.South;
		if ( a < 0.75f )
			return Direction.East;
		return Direction.West;
	}
}
