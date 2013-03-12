/* Cell.cs
 * Copyright Eddie Cameron 2012
 * ----------------------------
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell
{
    public int x,y;
    public int room = -1;
    
    Vector3 _centrePosition;
    public Vector3 centrePosition{
        get{
            if ( _centrePosition.y != 0 )
                _centrePosition = new Vector3( CellMaster.cellSize * ( x + 0.5f - CellMaster.CellsPerSide * 0.5f ), 0, CellMaster.cellSize * ( y + 0.5f - CellMaster.CellsPerSide * 0.5f ) );
            
            return _centrePosition;
        }
    }
    
    public bool canGoNorth;
    public bool canGoEast;
    public bool canGoSouth;
    public bool canGoWest;
    
    public Cell()
    {
        x = y = -1;
        _centrePosition = -Vector3.one;
        canGoNorth = canGoSouth = canGoEast = canGoWest = true;
    }
    
    public Cell( int x, int y )
    {
        this.x = x;
        this.y = y;
        
        _centrePosition = new Vector3( CellMaster.cellSize * ( x + 0.5f - CellMaster.CellsPerSide * 0.5f ), 0, CellMaster.cellSize * ( y + 0.5f - CellMaster.CellsPerSide * 0.5f ) );
        canGoNorth = canGoSouth = canGoEast = canGoWest = true;
    }
    
    public bool CanGoInDirection( Direction dir )
    {
        switch( dir )
        {
        case Direction.North:
            return canGoNorth;
        case Direction.South:
            return canGoSouth;
        case Direction.East:
            return canGoEast;
        case Direction.West:
            return canGoWest;
        default:
            Debug.LogWarning( "Invalid direction " + dir );
            return false;
        }
    }
    
    public void SetOpenInDirection( Direction dir, bool toOpen )
    {
        switch( dir )
        {
        case Direction.North:
            canGoNorth = toOpen;
            break;
        case Direction.South:
            canGoSouth = toOpen;
            break;
        case Direction.East:
            canGoEast = toOpen;
            break;
        case Direction.West:
            canGoWest = toOpen;
            break;
        default:
            Debug.LogWarning( "Invalid direction " + dir );
            break;
        }
    }
    
    public Cell GetCellInDirection( Direction dir )
    {
        switch( dir )
        {
        case Direction.North:
            if( y < CellMaster.CellsPerSide - 1 )
                return CellMaster.GetCellAt( x, y + 1 );
            break;
        case Direction.South:
            if( y > 0 )
                return CellMaster.GetCellAt( x, y - 1 );
            break;
        case Direction.East:
            if( x < CellMaster.CellsPerSide - 1 )
                return CellMaster.GetCellAt( x + 1, y );
            break;
        case Direction.West:
            if( x > 0 )
                return CellMaster.GetCellAt( x - 1, y );
            break;
        default:
            Debug.LogWarning ( "Invalid direction " + dir );
            break;
        }
        return null;
    }
    
    public static Direction ReverseDirection( Direction dir )
    {
        switch( dir )
        {
        case Direction.North:
            return Direction.South;
        case Direction.South:
            return Direction.North;
        case Direction.East:
            return Direction.West;
        case Direction.West:
            return Direction.East;
        default:
            Debug.LogWarning( "Invalid direction " + dir );
            return Direction.None;
        }
    }
    
    public override bool Equals (object obj)
    {
        Cell otherCell = obj as Cell;
        return otherCell != null && Equals ( this, otherCell );
    }
    
    public bool Equals( Cell other )
    {
        return other != null && Equals ( this, other );
    }
    
    public static bool Equals( Cell a, Cell b )
    {
        return a.x == b.x && a.y == b.y;
    }
    
    public override int GetHashCode()
    {
        return x.GetHashCode () + ( CellMaster.CellsPerSide + y ).GetHashCode();
    }
    
    public override string ToString()
    {
        return string.Format("[Cell: x={0}, y={1}]", x, y );
    }
}

public enum Direction
{
    None,
    North,
    South,
    East,
    West
}