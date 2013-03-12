/* WorldBuilder.cs
 * Copyright Grasshopper 2012
 * ----------------------------
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldBuilder : MonoBehaviour 
{
    public Transform[] wallPrefabs;
    public Transform wallEndPrefab;

    public Direction exitDirection { get; private set; }

    void Start()
    {
        BuildWalls();
    }

    // Instantiate walls?
    void BuildWalls()
    {
        int cellsPerSide = CellMaster.CellsPerSide;

        var verticalWalls = new Cell[cellsPerSide, cellsPerSide];
        for ( int x = 0; x < CellMaster.CellsPerSide - 1; x++ )
        {
            int wallType = Random.Range( 0, wallPrefabs.Length );
            for( int y = 0; y < CellMaster.CellsPerSide; y++ )
            {
                if ( !CellMaster.GetCellAt( x,y ).canGoEast )
                {
                    CreateWall( CellMaster.GetCellAt( x,y ), Direction.East, wallType );
                    verticalWalls[x,y] = CellMaster.GetCellAt( x,y );
                    
                    if ( y > 0 && verticalWalls[x,y - 1] == null )  
                        CreateWallCap ( CellMaster.GetCellAt( x,y ), true );
                }
                else
                {
                    wallType = Random.Range ( 0, wallPrefabs.Length );
                    if ( y > 0 && verticalWalls[x,y - 1] != null )
                        CreateWallCap ( CellMaster.GetCellAt( x,y ), true );
                }
            }
        }
    
        var horizontalWalls = new Cell[cellsPerSide, cellsPerSide];
        for ( int y = 0; y < cellsPerSide - 1; y++ )
        {
            int wallType = Random.Range ( 0, wallPrefabs.Length );
            for( int x = 0; x < cellsPerSide; x++ )
            {
                if ( !CellMaster.GetCellAt( x,y ).canGoNorth )
                {
                    CreateWall( CellMaster.GetCellAt( x,y ), Direction.North, wallType );
                    horizontalWalls[x,y] = CellMaster.GetCellAt( x,y );
                    
                    if ( x > 0 && horizontalWalls[x - 1,y] == null )    
                        CreateWallCap ( CellMaster.GetCellAt( x,y ), false );
                }
                else
                {
                    wallType = Random.Range ( 0, wallPrefabs.Length );
                    if ( x > 0 && horizontalWalls[x - 1,y] != null )
                        CreateWallCap ( CellMaster.GetCellAt( x,y ), false );
                }
            }
        }
    }
    
    /// <summary>
    /// Instantiates a wall between two cells.
    /// </summary>
    /// <param name='cellA'>
    /// Cell a.
    /// </param>
    /// <param name='cellB'>
    /// Cell b.
    /// </param>
    void CreateWall( Cell cellA, Direction inDirection, int prefabID )
    {
        Vector3 spawnPos = cellA.centrePosition;
        Quaternion spawnRot = Quaternion.identity;
        switch( inDirection )
        {
        case Direction.North:
            spawnPos += Vector3.forward * CellMaster.cellSize * 0.5f;
            break;
        case Direction.South:
            spawnPos += Vector3.back * CellMaster.cellSize * 0.5f;
            break;
        case Direction.East:
            spawnPos += Vector3.right * CellMaster.cellSize * 0.5f;
            spawnRot = Quaternion.AngleAxis ( 90, Vector3.up );
            break;
        case Direction.West:
            spawnPos += Vector3.left * CellMaster.cellSize * 0.5f;
            spawnRot = Quaternion.AngleAxis ( 90, Vector3.up );
            break;
        }
        
        Instantiate ( wallPrefabs[prefabID], spawnPos, spawnRot );
    }
    
    /// <summary>
    /// Creates a wall cap.
    /// </summary>
    /// <param name='onCell'>
    /// On cell.
    /// </param>
    /// <param name='southEast'>
    /// Whether cap is on south east corner of cell or north west.
    /// </param>
    void CreateWallCap( Cell onCell, bool southEast )
    {
        if ( southEast )
            Instantiate ( wallEndPrefab, onCell.centrePosition + new Vector3( 0.5f, 0, -0.5f ) * CellMaster.cellSize, Quaternion.identity );
        else  
            Instantiate ( wallEndPrefab, onCell.centrePosition + new Vector3( -0.5f, 0, 0.5f ) * CellMaster.cellSize, Quaternion.identity );
    }
}
