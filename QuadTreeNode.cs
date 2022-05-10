using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuadTreeNode
{
    public String hashvalue;
    private int corner;
    private Vector3 position = Vector3.zero;

    public Vector3 cornerTL;
    public Vector3 cornerTR;
    public Vector3 cornerBL;
    public Vector3 cornerBR;

    private QuadTreeNode parent;
    private QuadTreeNode topLeft;
    private QuadTreeNode topRight;
    private QuadTreeNode bottomLeft;
    private QuadTreeNode bottomRight;
    private int level = 0;
    private float radius = 0;

    public byte[] neighbours = new byte[4];

    public static int NE = 1;
    public static int NW = 0;
    public static int SE = 2;
    public static int SW = 3;

    public static int N = 0;
    public static int S = 1;
    public static int E = 2;
    public static int W = 3;

    public QuadTreeNode(QuadTreeNode parent, String hashvalue, int corner, Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, int level, float radius)
    {
        this.parent = parent;
        this.level = level;
        this.radius = radius;
        if (parent != null)
            this.hashvalue = hashvalue;
        else
            this.hashvalue = "";
        this.corner = corner;

        cornerTL = Spherize(tl);
        cornerTR = Spherize(tr);
        cornerBL = Spherize(bl);
        cornerBR = Spherize(br);

        this.position = Spherize(Vector3.Lerp(cornerTL, cornerBR, 0.5f));
    }

    public QuadTreeNode FindGreaterOrEqualNeighbours(QuadTreeNode node, int direction)
    {
        // direction : 0 -> North, 1 -> South, 2 -> East, 3 -> West
        /*
        def get_neighbor_of_greater_or_equal_size(self, direction):   
            if direction == self.Direction.N:       
                if self.parent is None: # Reached root?
                    return None
                if self.parent.children[self.Child.SW] == self: # Is 'self' SW child?
                    return self.parent.children[self.Child.NW]
                if self.parent.children[self.Child.SE] == self: # Is 'self' SE child?
                    return self.parent.children[self.Child.NE]

                node = self.parent.get_neighbor_of_greater_or_same_size(direction)
                if node is None or node.is_leaf():
                    return node

                # 'self' is guaranteed to be a north child
                return (node.children[self.Child.SW]
                        if self.parent.children[self.Child.NW] == self # Is 'self' NW child?
                        else node.children[self.Child.SE])
            else:
            # TODO: implement symmetric to NORTH case
        */
        if(direction == N)
        {
            if (node.GetParent() == null)
                return null;
            if (node.GetParent().GetBottomLeft() == node)
                return node.GetParent().GetTopLeft();
            if (node.GetParent().GetBottomRight() == node)
                return node.GetParent().GetTopRight();

            node = node.GetParent().FindGreaterOrEqualNeighbours(node.GetParent(), direction);
            if (node == null || node.IsLeaf())
                return node;

            if (node.GetParent().GetTopLeft() == node)
                return node.GetBottomLeft();
            else
                return node.GetBottomRight();
        }
        else if (direction == S)
        {
            if (node.GetParent() == null)
                return null;
            if (node.GetParent().GetTopLeft() == node)
                return node.GetParent().GetBottomLeft();
            if (node.GetParent().GetTopRight() == node)
                return node.GetParent().GetBottomRight();

            node = node.GetParent().FindGreaterOrEqualNeighbours(node.GetParent(), direction);
            if (node == null || node.IsLeaf())
                return node;

            if (node.GetParent().GetBottomLeft() == node)
                return node.GetTopLeft();
            else
                return node.GetTopRight();
        }
        else if (direction == E)
        {
            if (node.GetParent() == null)
                return null;
            if (node.GetParent().GetTopLeft() == node)
                return node.GetParent().GetTopRight();
            if (node.GetParent().GetBottomLeft() == node)
                return node.GetParent().GetBottomRight();

            node = node.GetParent().FindGreaterOrEqualNeighbours(node.GetParent(), direction);
            if (node == null || node.IsLeaf())
                return node;

            if (node.GetParent().GetBottomRight() == node)
                return node.GetBottomLeft();
            else
                return node.GetTopLeft();
        }
        else if (direction == W)
        {
            if (node.GetParent() == null)
                return null;
            if (node.GetParent().GetTopRight() == node)
                return node.GetParent().GetTopLeft();
            if (node.GetParent().GetBottomRight() == node)
                return node.GetParent().GetBottomLeft();

            node = node.GetParent().FindGreaterOrEqualNeighbours(node.GetParent(), direction);
            if (node == null || node.IsLeaf())
                return node;

            if (node.GetParent().GetBottomLeft() == node)
                return node.GetBottomRight();
            else
                return node.GetTopRight();
        }
        return null;
    }

    Vector3 Spherize(Vector3 p)
    {
        return p.normalized * radius;
    }

    public float GetRadius()
    {
        return radius;
    }
    public Vector3 GetPosition()
    {
        return position;
    }

    public QuadTreeNode GetParent()
    {
        return parent;
    }

    public QuadTreeNode GetTopLeft()
    {
        return topLeft;
    }

    public QuadTreeNode GetTopRight()
    {
        return topRight;
    }

    public QuadTreeNode GetBottomLeft()
    {
        return bottomLeft;
    }

    public QuadTreeNode GetBottomRight()
    {
        return bottomRight;
    }

    public bool IsLeaf()
    {
        return topLeft == null && topRight == null && bottomLeft == null && bottomRight == null;
    }

    public int GetSize(QuadTreeNode node)
    {
        if (!node.IsLeaf())
        {
            return GetSize(node.topLeft) + GetSize(node.topRight) + GetSize(node.bottomLeft) + GetSize(node.bottomRight);
        }
        return 1;
    }

    public int GetLevel()
    {
        return level;
    }

    private bool WithinRange(Vector3 p)
    {
        float quadSize = Vector3.Distance(cornerBL, cornerTR);
        float ratio = 2;
        if(p.magnitude < radius)
        {
            return Vector3.Distance(Spherize(p), position) - 30 < quadSize * ratio;
        }
        return Vector3.Distance(p, position) - 30 < quadSize * ratio;
    }
    public void Recalculate(Vector3 cameraPosition)
    {
        if (level < 9 && WithinRange(cameraPosition))
        {
            Split();
            if(!IsLeaf())
            {
                topLeft.Recalculate(cameraPosition);
                topRight.Recalculate(cameraPosition);
                bottomLeft.Recalculate(cameraPosition);
                bottomRight.Recalculate(cameraPosition);
            }
        }
        else
        {
            topLeft = null;
            topRight = null;
            bottomLeft = null;
            bottomRight = null;
        }
    }

    public QuadTreeNode[] GetLeafNodes(QuadTreeNode node)
    {
        List<QuadTreeNode> leafs = new List<QuadTreeNode>();
        GetLeafNodes(node, leafs);
        return leafs.ToArray();
    }
    void GetLeafNodes(QuadTreeNode node, List<QuadTreeNode> leafs)
    {
        if (node.IsLeaf())
        {
            leafs.Add(node);
        }
        else
        {
            GetLeafNodes(node.GetTopRight(), leafs);
            GetLeafNodes(node.GetTopLeft(), leafs);
            GetLeafNodes(node.GetBottomRight(), leafs);
            GetLeafNodes(node.GetBottomLeft(), leafs);
        }
    }

    public bool InViewFrustrum()
    {
        return InView(cornerTL) || InView(cornerTR) || InView(cornerBR) || InView(cornerBL);
    }
    private bool InView(Vector3 p)
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(p);
        //return screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;
        return screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1 && screenPos.z > 0;
    }

    void Split()
    {
        Vector3 topmid = Vector3.Lerp(cornerTR, cornerTL, .5f);
        Vector3 botmid = Vector3.Lerp(cornerBR, cornerBL, .5f);
        Vector3 rightmid = Vector3.Lerp(cornerBR, cornerTR, .5f);
        Vector3 leftmid = Vector3.Lerp(cornerBL, cornerTL, .5f);
        Vector3 mid = Vector3.Lerp(cornerBL, cornerTR, .5f);
        
        topLeft = new QuadTreeNode(this, hashvalue + "0", NW, cornerTL, topmid, leftmid, mid, level+1, radius);
        topRight = new QuadTreeNode(this, hashvalue + "1", NE, topmid, cornerTR, mid, rightmid, level+1, radius);
        bottomRight = new QuadTreeNode(this, hashvalue + "2", SE, mid, rightmid, botmid, cornerBR, level + 1, radius);
        bottomLeft = new QuadTreeNode(this, hashvalue + "3", SW, leftmid, mid, cornerBL, botmid, level+1, radius);
        
    }
}
