using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Data.Trees
{
    public class BstNode<TKey, TValue> where TKey : IComparable<TKey>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public BstNode<TKey, TValue> Left { get; set; }
        public BstNode<TKey, TValue> Right { get; set; }

        public BstNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"[Key: {Key}]";
        }
    }

    /// <summary>
    /// Standard Binary Search Tree (BST) for organizing and retrieving service requests by ID.
    /// Provides average O(log n) search, insertion, and in-order sorted traversal.
    /// </summary>
    public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        public BstNode<TKey, TValue> Root { get; private set; }
        public int Count { get; private set; }

        public void Insert(TKey key, TValue value)
        {
            Root = InsertRec(Root, key, value);
        }

        private BstNode<TKey, TValue> InsertRec(BstNode<TKey, TValue> current, TKey key, TValue value)
        {
            if (current == null)
            {
                Count++;
                return new BstNode<TKey, TValue>(key, value);
            }

            int cmp = key.CompareTo(current.Key);
            if (cmp < 0)
            {
                current.Left = InsertRec(current.Left, key, value);
            }
            else if (cmp > 0)
            {
                current.Right = InsertRec(current.Right, key, value);
            }
            else
            {
                current.Value = value; // Update existing
            }

            return current;
        }

        /// <summary>
        /// Searches for a node by key, recording the path of visited keys and comparison count.
        /// </summary>
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

        private void InOrderRec(BstNode<TKey, TValue> node, List<TValue> list)
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

        private void PreOrderRec(BstNode<TKey, TValue> node, List<TValue> list)
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

        private void PostOrderRec(BstNode<TKey, TValue> node, List<TValue> list)
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

            var queue = new Queue<BstNode<TKey, TValue>>();
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

        public int GetHeight()
        {
            return GetHeightRec(Root);
        }

        private int GetHeightRec(BstNode<TKey, TValue> node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(GetHeightRec(node.Left), GetHeightRec(node.Right));
        }

        /// <summary>
        /// Generates an ASCII visual representation of the tree structure.
        /// </summary>
        public string GenerateAsciiTree()
        {
            if (Root == null) return "(Tree is empty)";
            var sb = new System.Text.StringBuilder();
            PrintAscii(Root, "", true, sb);
            return sb.ToString();
        }

        private void PrintAscii(BstNode<TKey, TValue> node, string indent, bool isLast, System.Text.StringBuilder sb)
        {
            sb.Append(indent);
            sb.Append(isLast ? "└── " : "├── ");
            sb.AppendLine($"[ID: {node.Key}]");

            indent += isLast ? "    " : "│   ";

            var children = new List<BstNode<TKey, TValue>>();
            if (node.Left != null) children.Add(node.Left);
            if (node.Right != null) children.Add(node.Right);

            for (int i = 0; i < children.Count; i++)
            {
                PrintAscii(children[i], indent, i == children.Count - 1, sb);
            }
        }
    }
}
