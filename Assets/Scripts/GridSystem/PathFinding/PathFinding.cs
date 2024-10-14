using System.Collections.Generic;
using UnityEngine;

namespace GridSystem.PathFinding
{
    public class PathFinding
    {
        PathFinding _instance;
        private const int MoveStraightCost = 10;
        private const int MoveDiagnolCost = 14;

        private Grid<Node> _grid;
        private List<Node> _openList;
        private List<Node> _closeList;
        public PathFinding(Grid<Node> grid)
        {
            _grid = grid;
            _instance = this;
        }
        private List<Node> CalculatePath(Node endNode)
        {
            List<Node> path = new List<Node>
            {
                endNode
            };
            Node currentNode = endNode;
            while (currentNode.CameFromNode != null)
            {
                path.Add(currentNode.CameFromNode);
                currentNode = currentNode.CameFromNode;
            }
            path.Reverse();
            return path;
        }
        public List<Node> FindPath(int startX, int startY, int endX, int endY)
        {
            Node startNode = _grid.GetNodeWithoutCoord(startX, startY);
            Node endNode = _grid.GetNodeWithoutCoord(endX, endY);
            _openList = new List<Node> { startNode };
            _closeList = new List<Node>();
            for (int i = 0; i < _grid.width; i++)
            {
                for (int j = 0; j < _grid.height; j++)
                {
                    Node node = _grid.GetNodeWithoutCoord(i, j);
                    node.GCost = int.MaxValue;
                    node.CalculateFCost();
                    node.CameFromNode = null;
                }
            }
            startNode.GCost = 0;
            startNode.HCost = CalculateDistance(startNode, endNode);
            while (_openList.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(_openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                _openList.Remove(currentNode);
                _closeList.Add(currentNode);

                foreach (Node neighbourNode in currentNode.GetNeighbourList())
                {
                    if (_closeList.Contains(neighbourNode)) continue;

                    if (!neighbourNode.IsAvailble)
                    {
                        _closeList.Add(neighbourNode);
                        continue;
                    }
                    float tentativeGCost = currentNode.GCost + CalculateDistance(neighbourNode, currentNode);
                    if(tentativeGCost < neighbourNode.GCost)
                    {
                        neighbourNode.CameFromNode = currentNode;
                        neighbourNode.GCost = tentativeGCost;
                        neighbourNode.HCost = CalculateDistance(neighbourNode, endNode);
                        neighbourNode.CalculateFCost();
                        if (!_openList.Contains(neighbourNode))
                        {
                            _openList.Add(neighbourNode);
                        }
                    }
                }
            }
            //NoPath
            return null;
        }
      
        private float CalculateDistance(Node a, Node b)
        {
            float xDist = Mathf.Abs(a.XPos - b.XPos);
            float yDist = Mathf.Abs(a.YPos - b.YPos);
            float remaning = Mathf.Abs(xDist - yDist);
            return MoveDiagnolCost * Mathf.Min(xDist, yDist) + MoveStraightCost * remaning;
        }
        private Node GetLowestFCostNode(List<Node> list)
        {
            Node lowestFCostNode = list[0];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].FCost < lowestFCostNode.FCost)
                {
                    lowestFCostNode = list[i];
                }
            }
            return lowestFCostNode;
        }
    }
}