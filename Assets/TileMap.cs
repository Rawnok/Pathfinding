using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[Serializable]
public class Node
{
    public List<Node> neighbours;
    public int x;
    public int y;

    public Node ()
    {
        neighbours = new List<Node> ();
    }

    public Vector3 GetVector3 (int z = 0)
    {
        return new Vector3 ( x, y, z );
    }

    public float DistanceTo (Node n)
    {
        return Vector2.Distance (
            new Vector2 ( x, y ), new Vector2 ( n.x, n.y ) );
    }
}//node



public class TileMap : MonoBehaviour {

    public GameObject unitObject;

    public TileType[] tileTypes;

    int[,] tiles;

    int mapSizeX = 10;
    int mapSizeY = 10;

    Node[,] graph;
    Dictionary<Node, ClickableTile> visuals;
    private WaitForSeconds waitSec = new WaitForSeconds ( 0.1f );

    public bool useAStar = false;

    private void Start ()
    {
        GenerateMapData ();
        GeneratePathFindingGraph ();
        MoveAtOnce ( new Vector2 ( 5, 5 ) );
    }

    private void GeneratePathFindingGraph ()
    {
        //initialize the array
        graph = new Node[mapSizeX, mapSizeY];
        visuals = new Dictionary<Node, ClickableTile> ();

        //initialize each node on the array
        for ( int x = 0; x < mapSizeX; x++ )
        {
            for ( int y = 0; y < mapSizeY; y++ )
            {
                graph[x, y] = new Node ();
                graph[x, y].x = x;
                graph[x, y].y = y;

                // instantiate visuals
                TileType tt = tileTypes[tiles[x, y]];

                GameObject go = Instantiate ( tt.tileVisualPrefab, new Vector3 ( x, y, 0 ), Quaternion.identity );
                go.transform.SetParent ( this.transform );

                ClickableTile ct = go.GetComponent<ClickableTile> ();
                ct.tileX = x;
                ct.tileY = y;
                ct.mapObject = this;

                visuals.Add ( graph[x, y], ct );
            }
        }

        for ( int x = 0; x < mapSizeX; x++ )
        {
            for ( int y = 0; y < mapSizeY; y++ )
            {
                // 4 way connected map
                //if (x > 0)
                //    graph[x, y].neighbours.Add ( graph[x - 1, y] );

                //if (x < ( mapSizeX - 1 ) )
                //    graph[x, y].neighbours.Add ( graph[x + 1, y] );

                //if ( y > 0 )
                //    graph[x, y].neighbours.Add ( graph[x, y-1] );

                //if ( y < ( mapSizeY - 1 ) )
                //    graph[x, y].neighbours.Add ( graph[x, y+1] );


                // 8 way tiles, diagonal movements

                //try left
                if ( x > 0 )
                {
                    graph[x, y].neighbours.Add ( graph[x - 1, y] );
                    if ( y > 0 )
                    {
                        graph[x, y].neighbours.Add ( graph[x - 1, y-1] );
                    }

                    if (y < mapSizeY - 1)
                    {
                        graph[x, y].neighbours.Add ( graph[x - 1, y + 1] );
                    }
                }


                //try right
                if ( x < ( mapSizeX - 1 ) )
                {
                    graph[x, y].neighbours.Add ( graph[x + 1, y] );
                    if ( y > 0 )
                    {
                        graph[x, y].neighbours.Add ( graph[x + 1, y - 1] );
                    }

                    if ( y < mapSizeY - 1 )
                    {
                        graph[x, y].neighbours.Add ( graph[x + 1, y + 1] );
                    }
                }


                if ( y > 0 )
                {
                    graph[x, y].neighbours.Add ( graph[x, y - 1] );
                }


                if ( y < ( mapSizeY - 1 ) )
                {
                    graph[x, y].neighbours.Add ( graph[x, y + 1] );
                }
                    

            }
        }
    }

    private void GenerateMapData ()
    {
        tiles = new int[mapSizeX, mapSizeY];

        for ( int x = 0; x < mapSizeX; x++ )
        {
            for ( int y = 0; y < mapSizeY; y++ )
            {
                tiles[x, y] = 0;
            }
        }

        // Mountains
        tiles[4, 4] = 1;
        tiles[5, 4] = 1;
        tiles[6, 4] = 1;
        tiles[7, 4] = 1;
        tiles[8, 4] = 1;

        //tiles[4, 5] = 2;
        //tiles[4, 6] = 2;
        //tiles[8, 5] = 2;
        //tiles[8, 6] = 2;

        //swamp
        //tiles[2, 0] = 1;
        //tiles[3, 0] = 1;
        //tiles[2, 1] = 1;
        //tiles[2, 1] = 1;
        //tiles[2, 2] = 1;
        //tiles[2, 2] = 1;
        //tiles[2, 3] = 1;
    }
    
    private Vector3 TileCordToWorldCord (Vector2 address)
    {
        // here we can change the world cordination if we have a special kind of tiling system 
        return new Vector3 ( address.x, address.y, 0 ); 
    }

    private bool IsWalkable (Node n1)
    {
        TileType tt = tileTypes[tiles[n1.x, n1.y]];
        return tt.movementCost < 999;
    }

    private float GetMovementCost (Node n1)
    {
        TileType tt = tileTypes[tiles[n1.x, n1.y]];
        return tt.movementCost;
    }

    private float GetMovementCost ( Node n1, Node n2 )
    {
        TileType tt = tileTypes[tiles[n1.x, n1.y]];

        float cost = tt.movementCost;
        if ( n1.x != n2.x && n1.y != n2.y )
        {
            //we are moving diagonally
            cost += 0.4f;
        }
        return cost;
    }

    private float GetGValueForNodes (Node n1, Node n2)
    {
        TileType tt = tileTypes[tiles[n1.x, n1.y]];

        float cost = tt.movementCost;
        if ( n1.x != n2.x && n1.y != n2.y )
        {
            //we are moving diagonally
            cost += 0.4f;
        }
        return cost;
    }

    public IEnumerator MoveSelectedUnitByAStar (int x, int y)
    {
        Node targetNode = graph[x, y];
        if ( GetMovementCost ( targetNode ) >= 999 )
        {
            //we cannot move in this destination
            yield break;
        }


        Unit uObj = unitObject.GetComponent<Unit> ();
        int startX = uObj.tileX;
        int startY = uObj.tileY;

        Node startNode = graph[startX, startY];

        List<Node> openNodes = new List<Node> ();
        List<Node> closedNodes = new List<Node> ();

        
        Dictionary<Node, float> hCosts = new Dictionary<Node, float> ();
        Dictionary<Node, float> dist = new Dictionary<Node, float> ();
        Dictionary<Node, Node> prevPath = new Dictionary<Node, Node> ();
        
        dist[startNode] = 0;
        prevPath[startNode] = null;

        //Calculating the heuristics
        foreach ( Node node in graph )
        {
            float h = Vector3.Distance ( node.GetVector3 (), targetNode.GetVector3 () );
            hCosts.Add ( node, h );

            if ( node != startNode )
            {
                dist.Add ( node, Mathf.Infinity );
                prevPath[node] = null;
            }
            //visuals[node].SetText ( hCosts[node] );
            //yield return null;
        }
        
        //initialize starting node as open
        openNodes.Add ( startNode );

        while ( true )
        {
            //Debug.Log ("calculating start");
            Node u = null;
            float fCostCurrentLowest = 0;

            //finding the lowest possible fcost in open nodes
            foreach ( Node possibleNode in openNodes )
            {
                if ( u == null )
                {
                    u = possibleNode;
                    fCostCurrentLowest = hCosts[u] + dist[u];
                    visuals[u].SetText ( fCostCurrentLowest );
                    continue;
                }

                float fc = hCosts[possibleNode] + dist[possibleNode];
                visuals[possibleNode].SetText ( fc );

                //yield return waitSec;
                //Debug.Log ( "choosing new node to traverse" );

                if ( fc < fCostCurrentLowest )
                {
                    u = possibleNode;
                    fCostCurrentLowest = hCosts[u] + dist[u];
                }
            }

            if (u == targetNode)
            {
                Debug.Log ("found destination");
                break;
            }
            else if ( IsWalkable(u) == false )
            {
                continue;
            }

            visuals[u].SetTileMat ( ClickableTile.MAT_TYPE.VISITED );
            visuals[u].SetText ( dist[u] + hCosts[u] );

            yield return waitSec;
            //remove the current traversing node from the open list
            openNodes.Remove ( u );
            //add current traversing node to the closed list
            closedNodes.Add ( u );

            foreach ( Node node in u.neighbours )
            {
                // is this node newly found node?
                if ( IsWalkable(node) && 
                    !openNodes.Contains(node) && 
                    !closedNodes.Contains(node))
                {
                    openNodes.Add (node);
                    dist[node] = dist[u] + GetMovementCost ( u, node );
                    prevPath[node] = u;
                }
            }

            //update the g values (dist) again in the open nodes who are neighbour of u
            foreach ( Node node in openNodes )
            {
                if ( !u.neighbours.Contains ( node ) )
                    continue;

                float newGCost = GetMovementCost ( u, node ) + dist[u];
                if (newGCost < dist[node])
                {
                    dist[node] = newGCost;
                    ///TODO update parent
                    prevPath[node] = u;
                    visuals[node].SetText ( "U." + newGCost );
                    yield return waitSec;
                }
            }

            //Debug.Log ( "calculating end" );

            if (openNodes.Count <= 0)
            {
                Debug.LogError ("Open nodes count ended");
                break;
            }
            //yield return new WaitForSeconds (1);
            //break;
        }

        yield return new WaitForSeconds (0.5f);

        List<Node> currNodes = new List<Node> ();
        Node cNode = targetNode;

        while ( cNode != null )
        {
            visuals[cNode].SetTileMat ( ClickableTile.MAT_TYPE.CHOSEN );
            currNodes.Add ( cNode );
            cNode = prevPath[cNode];
        }

        currNodes.Reverse ();
        uObj.currentPath = currNodes;

        foreach ( var item in dist.Keys )
        {
            visuals[item].SetText ( dist[item] + hCosts[item] );
        }
    }

    public IEnumerator MoveSelectedUnitTo (int x, int y)
    {
        //Dijkstras Algorithm

        //MoveAtOnce ( address );

        Node target = graph[x, y];

        if ( GetMovementCost ( target ) >= 999 )
        {
            Debug.Log ( "invalid path" );
            yield break;
        }

        foreach ( var item in visuals.Values )
        {
            item.SetText (".");
        }

        //Dijkstra Algorithm
        Unit unit = unitObject.GetComponent<Unit> ();
        unit.currentPath = null;

        Dictionary<Node, float> dist = new Dictionary<Node, float> ();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node> ();

        List<Node> unvisitedNodes = new List<Node> ();
        
        //Debug.Log ( "chosen destination " + x + ", " + y );

        Node source = graph[
            unit.tileX,
            unit.tileY];

        dist[source] = 0;
        prev[source] = null;
        
        //initialize
        // at start,every node is at infinity distance
        foreach ( var node in graph )
        {
            if ( node != source )
            {
                dist[node] = Mathf.Infinity;
                //Debug.Log ("setting infinity");
                //yield return waitSec; 
                prev[node] = null;
            }

            if (IsWalkable(node))
                unvisitedNodes.Add ( node );
        }

        while ( unvisitedNodes.Count > 0 )
        {
            Node u = null ;//unvisitedNodes.OrderBy ( n => dist[n] ).First ();

            foreach ( var possibleU in unvisitedNodes )
            {
                if ( u == null || dist[possibleU] < dist[u] )
                {
                    u = possibleU;
                }
            }

            visuals[u].SetText ("x");
            visuals[u].SetTileMat ( ClickableTile.MAT_TYPE.VISITED );
            yield return waitSec;

            if ( u == target )
            {
                break; // exit  the while loop
            }

            unvisitedNodes.Remove ( u );

            foreach ( Node v in u.neighbours )
            {
                //float alt = dist[u] + u.DistanceTo (v);
                float alt = dist[u] + GetMovementCost ( u, v );

                if ( alt < dist[v] )
                {
                    dist[v] = alt;
                    prev[v] = u;

                    visuals[v].SetText ( alt );
                    yield return waitSec;
                }
            }
        }

        // there is no target or we found shortest path
        if ( prev[target] == null )
        {
            // no route between our source and target
            Debug.LogError ( "NO valid way found" );
            yield return null;
        }
        else
        {

            yield return new WaitForSeconds (0.5f);
            foreach ( var item in dist.Keys )
            {
                visuals[item].SetText ( dist[item] );
            }

            //visuals[target].SetText ("GOAL");

            List<Node> currentPath = new List<Node> ();

            Node curr = target;
            while ( curr != null )
            {
                visuals[curr].SetTileMat (ClickableTile.MAT_TYPE.CHOSEN);
                currentPath.Add ( curr );
                curr = prev[curr];
            }

            currentPath.Reverse ();

            unit.currentPath = currentPath;
        }
    }

    private void MoveAtOnce ( Vector2 address )
    {
        unitObject.GetComponent<Unit> ().tileX = (int) address.x;
        unitObject.GetComponent<Unit> ().tileY = (int) address.y;

        unitObject.transform.position = TileCordToWorldCord ( address );
    }
}
