using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Data.Trees
{
    public enum NodeColor
    {
        Red,
        Black
    }

    public class RbNode<TKey, TValue> where TKey : IComparable<TKey>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public NodeColor Color { get; set; }
        public RbNode<TKey, TValue> Left { get; set; }
        public RbNode<TKey, TValue> Right { get; set; }
        public RbNode<TKey, TValue> Parent { get; set; }

        public RbNode(TKey key, TValue value, NodeColor color)
        {
            Key = key;
            Value = value;
            Color = color;
        }

        public override string ToString()
        {
            return $"[{Key} ({Color})]";
        }
    }

    /// <summary>
    /// Self-balancing Red-Black Tree implementation for municipal service requests.
    /// Guarantees O(log n) worst-case search and insert while adhering to all
    /// five classic Red-Black invariants.
    /// </summary>
    public class RedBlackTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private readonly RbNode<TKey, TValue> _nil;
        public RbNode<TKey, TValue> Root { get; private set; }
        public int Count { get; private set; }
        public int TotalRotations { get; private set; }
        public int ColorFlips { get; private set; }

        public RedBlackTree()
        {
            _nil = new RbNode<TKey, TValue>(default(TKey), default(TValue), NodeColor.Black);
            _nil.Left = _nil;
            _nil.Right = _nil;
            Root = _nil;
        }

        private void RotateLeft(RbNode<TKey, TValue> x)
        {
            TotalRotations++;
            var y = x.Right;
            x.Right = y.Left;
            if (y.Left != _nil) y.Left.Parent = x;
            y.Parent = x.Parent;

            if (x.Parent == _nil)
                Root = y;
            else if (x == x.Parent.Left)
                x.Parent.Left = y;
            else
                x.Parent.Right = y;

            y.Left = x;
            x.Parent = y;
        }

        private void RotateRight(RbNode<TKey, TValue> y)
        {
            TotalRotations++;
            var x = y.Left;
            y.Left = x.Right;
            if (x.Right != _nil) x.Right.Parent = y;
            x.Parent = y.Parent;

            if (y.Parent == _nil)
                Root = x;
            else if (y == y.Parent.Right)
                y.Parent.Right = x;
            else
                y.Parent.Left = x;

            x.Right = y;
            y.Parent = x;
        }

        public void Insert(TKey key, TValue value)
        {
            var z = new RbNode<TKey, TValue>(key, value, NodeColor.Red)
            {
                Left = _nil,
                Right = _nil,
                Parent = _nil
            };

            var y = _nil;
            var x = Root;

            while (x != _nil)
            {
                y = x;
                int cmp = z.Key.CompareTo(x.Key);
                if (cmp < 0)
                {
                    x = x.Left;
                }
                else if (cmp > 0)
                {
                    x = x.Right;
                }
                else
                {
                    x.Value = value; // Update existing value
                    return;
                }
            }

            z.Parent = y;
            if (y == _nil)
                Root = z;
            else if (z.Key.CompareTo(y.Key) < 0)
                y.Left = z;
            else
                y.Right = z;

            Count++;
            InsertFixup(z);
        }

        private void InsertFixup(RbNode<TKey, TValue> z)
        {
            while (z.Parent.Color == NodeColor.Red)
            {
                if (z.Parent == z.Parent.Parent.Left)
                {
                    var y = z.Parent.Parent.Right; // Uncle
                    if (y.Color == NodeColor.Red)
                    {
                        // Case 1: Uncle is Red -> Recolor
                        ColorFlips++;
                        z.Parent.Color = NodeColor.Black;
                        y.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        // Case 2: Uncle is Black and z is Right child -> Rotate Left
                        if (z == z.Parent.Right)
                        {
                            z = z.Parent;
                            RotateLeft(z);
                        }
                        // Case 3: Uncle is Black and z is Left child -> Recolor & Rotate Right
                        z.Parent.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        RotateRight(z.Parent.Parent);
                    }
                }
                else
                {
                    var y = z.Parent.Parent.Left; // Uncle
                    if (y.Color == NodeColor.Red)
                    {
                        // Case 1: Uncle is Red -> Recolor
                        ColorFlips++;
                        z.Parent.Color = NodeColor.Black;
                        y.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        // Case 2: Uncle is Black and z is Left child -> Rotate Right
                        if (z == z.Parent.Left)
                        {
                            z = z.Parent;
                            RotateRight(z);
                        }
                        // Case 3: Uncle is Black and z is Right child -> Recolor & Rotate Left
                        z.Parent.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        RotateLeft(z.Parent.Parent);
                    }
                }
            }

            Root.Color = NodeColor.Black; // Invariant 2: Root is always black
        }

        public bool Search(TKey key, out TValue value, out List<TKey> searchPath, out int comparisons)
        {
            searchPath = new List<TKey>();
            comparisons = 0;
            var current = Root;

            while (current != _nil)
            {
                comparisons++;
                searchPath.Add(current.Key);

                int cmp = key.CompareTo(current.Key);
                if (cmp == 0)
                {
                    value = current.Value;
                    return true;
                }
                current = (cmp < 0) ? current.Left : current.Right;
            }

            value = default(TValue);
            return false;
        }

        public bool Contains(TKey key)
        {
            return Search(key, out _, out _, out _);
        }

        public List<TValue> InOrderTraversal()
        {
            var list = new List<TValue>();
            InOrderRec(Root, list);
            return list;
        }

        private void InOrderRec(RbNode<TKey, TValue> node, List<TValue> list)
        {
            if (node == _nil) return;
            InOrderRec(node.Left, list);
            list.Add(node.Value);
            InOrderRec(node.Right, list);
        }

        public List<TValue> PreOrderTraversal()
        {
            var list = new List<TValue>();
            PreOrderRec(Root, list);
            return list;
        }

        private void PreOrderRec(RbNode<TKey, TValue> node, List<TValue> list)
        {
            if (node == _nil) return;
            list.Add(node.Value);
            PreOrderRec(node.Left, list);
            PreOrderRec(node.Right, list);
        }

        public List<TValue> PostOrderTraversal()
        {
            var list = new List<TValue>();
            PostOrderRec(Root, list);
            return list;
        }

        private void PostOrderRec(RbNode<TKey, TValue> node, List<TValue> list)
        {
            if (node == _nil) return;
            PostOrderRec(node.Left, list);
            PostOrderRec(node.Right, list);
            list.Add(node.Value);
        }

        public List<TValue> LevelOrderTraversal()
        {
            var list = new List<TValue>();
            if (Root == _nil) return list;

            var queue = new Queue<RbNode<TKey, TValue>>();
            queue.Enqueue(Root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                list.Add(node.Value);
                if (node.Left != _nil) queue.Enqueue(node.Left);
                if (node.Right != _nil) queue.Enqueue(node.Right);
            }

            return list;
        }

        public int GetBlackHeight()
        {
            int blackHeight = 0;
            var current = Root;
            while (current != _nil)
            {
                if (current.Color == NodeColor.Black) blackHeight++;
                current = current.Left;
            }
            return blackHeight;
        }

        public int GetTreeHeight()
        {
            return GetHeightRec(Root);
        }

        private int GetHeightRec(RbNode<TKey, TValue> node)
        {
            if (node == _nil) return 0;
            return 1 + Math.Max(GetHeightRec(node.Left), GetHeightRec(node.Right));
        }

        /// <summary>
        /// Validates that the 5 classic Red-Black properties hold.
        /// </summary>
        public bool VerifyProperties(out string report)
        {
            if (Root == _nil)
            {
                report = "Tree is empty (vacuously valid Red-Black tree).";
                return true;
            }

            if (Root.Color != NodeColor.Black)
            {
                report = "Violation of Property 2: Root is not Black.";
                return false;
            }

            int expectedBlackCount = -1;
            bool noConsecutiveReds = CheckRedPropertyAndBlackHeight(Root, 0, ref expectedBlackCount, out string violation);
            if (!noConsecutiveReds)
            {
                report = violation;
                return false;
            }

            report = $"All 5 Red-Black properties successfully validated. Root is Black; no consecutive red nodes exist; and all {Count} paths have a uniform Black Height of {expectedBlackCount}.";
            return true;
        }

        private bool CheckRedPropertyAndBlackHeight(RbNode<TKey, TValue> node, int currentBlackCount, ref int expectedBlackCount, out string violation)
        {
            violation = null;
            if (node == _nil)
            {
                currentBlackCount++; // NIL is black
                if (expectedBlackCount == -1)
                {
                    expectedBlackCount = currentBlackCount;
                }
                else if (currentBlackCount != expectedBlackCount)
                {
                    violation = $"Violation of Property 5: Path has black count {currentBlackCount}, expected {expectedBlackCount}.";
                    return false;
                }
                return true;
            }

            if (node.Color == NodeColor.Black)
            {
                currentBlackCount++;
            }
            else
            {
                // Property 4: If node is Red, children must be Black
                if (node.Left.Color == NodeColor.Red || node.Right.Color == NodeColor.Red)
                {
                    violation = $"Violation of Property 4: Red node {node.Key} has a red child.";
                    return false;
                }
            }

            if (!CheckRedPropertyAndBlackHeight(node.Left, currentBlackCount, ref expectedBlackCount, out violation))
                return false;

            return CheckRedPropertyAndBlackHeight(node.Right, currentBlackCount, ref expectedBlackCount, out violation);
        }

        public string GenerateAsciiTree()
        {
            if (Root == _nil) return "(Red-Black Tree is empty)";
            var sb = new System.Text.StringBuilder();
            PrintAscii(Root, "", true, sb);
            return sb.ToString();
        }

        private void PrintAscii(RbNode<TKey, TValue> node, string indent, bool isLast, System.Text.StringBuilder sb)
        {
            sb.Append(indent);
            sb.Append(isLast ? "└── " : "├── ");
            string colorTag = node.Color == NodeColor.Red ? "[RED]" : "[BLK]";
            sb.AppendLine($"[ID: {node.Key} {colorTag}]");

            indent += isLast ? "    " : "│   ";

            var children = new List<RbNode<TKey, TValue>>();
            if (node.Left != _nil) children.Add(node.Left);
            if (node.Right != _nil) children.Add(node.Right);

            for (int i = 0; i < children.Count; i++)
            {
                PrintAscii(children[i], indent, i == children.Count - 1, sb);
            }
        }
    }
}
