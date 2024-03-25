using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathNode
{
    public GameObject Tile { get; private set; }
    public List<PathConnection> connections = new List<PathConnection>();

    public PathNode(GameObject tile)
    {
        Tile = tile;
        connections = new List<PathConnection> ();
    }
    public void AddConnections(PathConnection pathConnection)
    {
        connections.Add(pathConnection);
    }

}

[System.Serializable]
public class PathConnection
{
    public float Cost { get; set; }
    public PathNode FromNode { get; set; }
    public PathNode ToNode { get; set;}
    public PathConnection(PathNode from, PathNode to, float cost = 1f)
    {
        FromNode = from;
        ToNode = to;
        Cost = cost;
    }
}
public class NodeRecord
{
    public PathNode Node { get; set; }
    public NodeRecord FromRecord { get; set; }
    public PathConnection Connection { get; set; }
    public float CostSoFar { get; set; }

    public NodeRecord(PathNode node = null)
    {
        Node = node;
        Connection = null;
        FromRecord = null;
        CostSoFar = 0f;

    }
 }


public class PathManager : MonoBehaviour
{
    public List<NodeRecord> openList;
    public List<NodeRecord> closeList;

    public List<PathConnection> path;

    public static PathManager Instance{get;private set;}

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            Initialize();
        }
           

    }
    private void Initialize()
    {
        openList = new List<NodeRecord>();
        closeList = new List<NodeRecord> ();
        path = new List<PathConnection>();
    }

    public List<PathNode> GetShortestPath(PathNode start, PathNode goal)
    {
        if(path.Count > 0)
        {
            GridManager.Instance.SetTileStatuses();
            path.Clear();
        }

        NodeRecord currentRecord = null;
        openList.Add(new NodeRecord(start));
        while(openList.Count > 0)
        {
            currentRecord = GetSmallestNode();
            if(currentRecord.Node == goal)
            {
                openList.Remove(currentRecord);
                closeList.Add(currentRecord);
                currentRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
                break;
            }
            List<PathConnection> connections = currentRecord.Node.connections;
            for(int i = 0;i<connections.Count;i++)
            {
                PathNode endNode = connections[i].ToNode;
                NodeRecord endNodeRecord;
                float endNodeCost = currentRecord.CostSoFar + connections[i].Cost;

                if (ContainsNode(closeList, endNode)) continue;
                else if(ContainsNode(openList,endNode))
                {
                    endNodeRecord = GetNodeRecord(openList, endNode);
                    if (endNodeRecord.CostSoFar <= endNodeCost)
                        continue;
                }
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.Node = endNode;
                }

                endNodeRecord.CostSoFar = endNodeCost;
                endNodeRecord.Connection = connections[i];
                endNodeRecord.FromRecord = currentRecord;
                if(!ContainsNode(openList,endNode))
                {
                    openList.Add(endNodeRecord);
                    endNodeRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
                }
            }
            openList.Remove(currentRecord);
            closeList.Add(currentRecord);
            currentRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
        }
        if (currentRecord == null) return null;
        if(currentRecord.Node!=goal)
        {
            Debug.Log("could not find path to goal");
        }
        else
        {
            Debug.Log("found path to goal");
            while (currentRecord.Node != start)
            {
                path.Add(currentRecord.Connection);
                currentRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.PATH);
                currentRecord = currentRecord.FromRecord;
            }
            path.Reverse();
        }
        openList.Clear();
        closeList.Clear();
        List<PathNode> pathList = new List<PathNode>();
        foreach(PathConnection connection in path)
        {
            pathList.Add(connection.ToNode);
        }
        return pathList;
    }
    public float GetTotalCost(PathNode start, PathNode goal)
    {
        if (path.Count > 0)
        {
            GridManager.Instance.SetTileStatuses();
            path.Clear();
        }
        float totalCost = 0;
        NodeRecord currentRecord = null;
        openList.Add(new NodeRecord(start));
        while (openList.Count > 0)
        {
            currentRecord = GetSmallestNode();
          
            
            if (currentRecord.Node == goal)
            {
                openList.Remove(currentRecord);
                closeList.Add(currentRecord);
                currentRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
                break;
            }
            List<PathConnection> connections = currentRecord.Node.connections;
            for (int i = 0; i < connections.Count; i++)
            {
                PathNode endNode = connections[i].ToNode;
                NodeRecord endNodeRecord;
                float endNodeCost = currentRecord.CostSoFar + connections[i].Cost;

                if (ContainsNode(closeList, endNode)) continue;
                else if (ContainsNode(openList, endNode))
                {
                    endNodeRecord = GetNodeRecord(openList, endNode);
                    if (endNodeRecord.CostSoFar <= endNodeCost)
                        continue;
                }
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.Node = endNode;
                }

                endNodeRecord.CostSoFar = endNodeCost;
                endNodeRecord.Connection = connections[i];
                endNodeRecord.FromRecord = currentRecord;
                if (!ContainsNode(openList, endNode))
                {
                    openList.Add(endNodeRecord);
                    endNodeRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
                }
            }
            openList.Remove(currentRecord);
            closeList.Add(currentRecord);
            currentRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
        }
        if (currentRecord == null) return 0;
        if (currentRecord.Node != goal)
        {
            Debug.Log("could not find path to goal");
        }
        else
        {
            Debug.Log("found path to goal");
            while (currentRecord.Node != start)
            {
                path.Add(currentRecord.Connection);
                Debug.Log(currentRecord.Node.Tile.GetComponent<TileScript>().cost);
                totalCost += currentRecord.Node.Tile.GetComponent<TileScript>().cost;
                currentRecord.Node.Tile.GetComponent<TileScript>().SetStatus(TileStatus.PATH);
                currentRecord = currentRecord.FromRecord;
            }
            path.Reverse();
        }
        openList.Clear();
        closeList.Clear();
        return totalCost;
    }
    public NodeRecord GetSmallestNode()
    {
        NodeRecord smallestNode = openList[0];

        for(int i = 1; i< openList.Count;i++)
        {
            if (openList[i].CostSoFar<smallestNode.CostSoFar)
            {
                smallestNode = openList[i];
            }
            else if (openList[i].CostSoFar == smallestNode.CostSoFar)
            {
                smallestNode = (Random.value < 0.5 ? openList[i] : smallestNode);
            }
        }
        return smallestNode;
    }

    public bool ContainsNode(List<NodeRecord> list, PathNode node)
    {
        foreach(NodeRecord record in list)
        {
            if(record.Node == node) return true;
        }
        return false;
    }

    public NodeRecord GetNodeRecord(List<NodeRecord> list,PathNode node)
    {
        foreach(NodeRecord record in list)
        {
            if(record.Node == node) return record;
        }
        return null;
    }
}
