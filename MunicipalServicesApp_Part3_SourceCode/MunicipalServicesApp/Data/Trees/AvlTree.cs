using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Data.Trees
{
    public class AvlNode<TKey, TValue> where TKey : IComparable<TKey>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public int Height { get; set; }
        public AvlNode<TKey, TValue> Left { get; set; }
        public AvlNode<TKey, TValue> Right { get; set; }

        public AvlNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            Height = 1; // Leaf height is 1
        }

        public override string ToString()
        {
            return $"[Key: {Key}, H: {Height}]";
        }
    }

    /// <summary>
    /// Self-balancing Binary Search Tree (AVL Tree) for guaranteed O(log n)
    /// retrieval of municipal service requests.
    /// Employs strict height balancing with LL, RR, LR, and RL rotations.
    /// </summary>
    public class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        public AvlNode<TKey, TValue> Root { get; private set; }
        public int Count { get; private set; }
        public int TotalRotations { get; private set; }
        public int LlRotations { get; private set; }
        public int RrRotations { get; private set; }
        public int LrRotations { get; private set; }
        public int RlRotations { get; private set; }

        private int GetHeight(AvlNode<TKey, TValue> node)
        {
            return node == null ? 0 : node.Height;
        }

        public int GetBalanceFactor(AvlNode<TKey, TValue> node)
        {
            return node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);
        }

        private void UpdateHeight(AvlNode<TKey, TValue> node)
        {
            if (node != null)
            {
                node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            }
        }

        /// <summary>
        /// Right rotation (Left-Left case rebalancing).
        /// </summary>
        private AvlNode<TKey, TValue> RotateRight(AvlNode<TKey, TValue> y)
        {
            var x = y.Left;
            var t2 = x.Right;

            x.Right = y;
            y.Left = t2;

            UpdateHeight(y);
            UpdateHeight(x);

            TotalRotations++;
            return x;
        }

        /// <summary>
        /// Left rotation (Right-Right case rebalancing).
        /// </summary>
        private AvlNode<TKey, TValue> RotateLeft(AvlNode<TKey, TValue> x)
        {
            var y = x.Right;
            var t2 = y.Left;

            y.Left = x;
            x.Right = t2;

            UpdateHeight(x);
            UpdateHeight(y);

            TotalRotations++;
            return y;
        }

        public void Insert(TKey key, TValue value)
        {
            Root = InsertRec(Root, key, value);
        }

        private AvlNode<TKey, TValue> InsertRec(AvlNode<TKey, TValue> node, TKey key, TValue value)
        {
            if (node == null)
            {
                Count++;
                return new AvlNode<TKey, TValue>(key, value);
            }

            int cmp = key.CompareTo(node.Key);
            if (cmp < 0)
            {
                node.Left = InsertRec(node.Left, key, value);
            }
            else if (cmp > 0)
            {
                node.Right = InsertRec(node.Right, key, value);
            }
            else
            {
                node.Value = value; // Update existing
                return node;
            }

            UpdateHeight(node);
            int balance = GetBalanceFactor(node);

            // Left-Left Case
            if (balance > 1 && key.CompareTo(node.Left.Key) < 0)
            {
                LlRotations++;
                return RotateRight(node);
            }

            // Right-Right Case
            if (balance < -1 && key.CompareTo(node.Right.Key) > 0)
            {
                RrRotations++;
                return RotateLeft(node);
            }

            // Left-Right Case
            if (balance > 1 && key.CompareTo(node.Left.Key) > 0)
            {
                LrRotations++;
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Right-Left Case
            if (balance < -1 && key.CompareTo(node.Right.Key) < 0)
            {
                RlRotations++;
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        public bool Search(TKey key, out TValue value, out List<TKey> searchPath, out int comparisons)
        {
            searchPath = new List<TKey>();
            comparisons = 0;
            var current = Root;

            while (current != null)
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

        private void InOrderRec(AvlNode<TKey, TValue> node, List<TValue> list)
        {
            if (node == null) return;
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

        private void PreOrderRec(AvlNode<TKey, TValue> node, List<TValue> list)
        {
            if (node == null) return;
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

        private void PostOrderRec(AvlNode<TKey, TValue> node, List<TValue> list)
        {
            if (node == null) return;
            PostOrderRec(node.Left, list);
            PostOrderRec(node.Right, list);
            list.Add(node.Value);
        }

        public List<TValue> LevelOrderTraversal()
        {
            var list = new List<TValue>();
            if (Root == null) return list;

            var queue = new Queue<AvlNode<TKey, TValue>>();
            queue.Enqueue(Root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                list.Add(node.Value);
                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }

            return list;
        }

        public int GetTreeHeight()
        {
            return GetHeight(Root);
        }

        public string GenerateAsciiTree()
        {
            if (Root == null) return "(AVL Tree is empty)";
            var sb = new System.Text.StringBuilder();
            PrintAscii(Root, "", true, sb);
            return sb.ToString();
        }

        private void PrintAscii(AvlNode<TKey, TValue> node, string indent, bool isLast, System.Text.StringBuilder sb)
        {
            sb.Append(indent);
            sb.Append(isLast ? "└── " : "├── ");
            sb.AppendLine($"[ID: {node.Key} | BF: {GetBalanceFactor(node):+0;-0;0} | H: {node.Height}]");

            indent += isLast ? "    " : "│   ";

            var children = new List<AvlNode<TKey, TValue>>();
            if (node.Left != null) children.Add(node.Left);
            if (node.Right != null) children.Add(node.Right);

            for (int i = 0; i < children.Count; i++)
            {
                PrintAscii(children[i], indent, i == children.Count - 1, sb);
            }
        }
    }
}
