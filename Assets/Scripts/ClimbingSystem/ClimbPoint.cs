using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] List<Neighbour> neighbours;
    [SerializeField] bool mountPoint;

    private void Awake()
    {
        var twowayNeighbours = neighbours.Where(n => n.isTooWay);
        foreach (var neighbour in twowayNeighbours)
        {
            neighbour.Point?.CreateConnection(this,-neighbour.direction,neighbour.connectionType,neighbour.isTooWay);
        }
    }

    void CreateConnection(ClimbPoint _point, Vector2 _dir, ConnectionType _conntype, bool _isTooWay = true)
    {
        var neighbour = new Neighbour()
        {
            Point = _point,
            direction = _dir,
            connectionType = _conntype,
            isTooWay = _isTooWay

        };
        neighbours.Add(neighbour);
    }
    public Neighbour GetNeibour( Vector2 dir)
    {
        Neighbour neighbour = null;

        if(dir.y !=0)
            neighbour = neighbours.FirstOrDefault(n => n.direction.y == dir.y);
        

        if(neighbour ==null && dir.x != 0)
            neighbour = neighbours.FirstOrDefault(n => n.direction.x == dir.x);
        

        return neighbour;
    }

     private void OnDrawGizmos() {
        {
            Debug.DrawRay(transform.position,transform.forward, Color.blue);
            foreach(var neighbour in neighbours)
            {
                if(neighbour.Point != null)
                {
                    Debug.DrawLine(transform.position,neighbour.Point.transform.position,(neighbour.isTooWay)? Color.green : Color.cyan);
                }
            }
        }
        
    }

    public bool MountPoint => mountPoint;
}

[System.Serializable]
public class Neighbour
{
    public ClimbPoint Point;
    public Vector2 direction;
    public ConnectionType connectionType;
    public bool isTooWay = true;
}

public enum ConnectionType { Jump, Move }
