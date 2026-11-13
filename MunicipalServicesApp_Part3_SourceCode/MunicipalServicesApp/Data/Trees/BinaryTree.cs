using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Data.Trees
{
    public class BinaryTreeNode<T>
    {
        public T Data { get; set; }
        public BinaryTreeNode<T> Left { get; set; }
        public BinaryTreeNode<T> Right { get; set; }

        public BinaryTreeNode(T data)
        {
            Data = data;
        }

        public override string ToString()
        {
            return Data != null ? Data.ToString() : "(null)";
        }
    }

    /// <summary>
    /// Basic binary tree implementation providing universal traversal algorithms
    /// (Pre-order, In-order, Post-order, and Level-order / Breadth-first)
    /// for inspecting and processing hierarchical municipal service structures.
    /// </summary>
    public class BinaryTree<T>
    {
        public BinaryTreeNode<T> Root { get; set; }

        public BinaryTree() { }

        public BinaryTree(BinaryTreeNode<T> root)
        {
            Root = root;
        }

        public List<T> PreOrderTraversal()
        {
            var result = new List<T>();
            PreOrderRec(Root, result);
            return result;
        }

        private void PreOrderRec(BinaryTreeNode<T> node, List<T> result)
        {
            if (node == null) return;
            result.Add(node.Data);
            PreOrderRec(node.Left, result);
            PreOrderRec(node.Right, result);
        }

        public List<T> InOrderTraversal()
        {
            var result = new List<T>();
            InOrderRec(Root, result);
            return result;
        }

        private void InOrderRec(BinaryTreeNode<T> node, List<T> result)
        {
            if (node == null) return;
            InOrderRec(node.Left, result);
            result.Add(node.Data);
            InOrderRec(node.Right, result);
        }

        public List<T> PostOrderTraversal()
        {
            var result = new List<T>();
            PostOrderRec(Root, result);
            return result;
        }

        private void PostOrderRec(BinaryTreeNode<T> node, List<T> result)
        {
            if (node == null) return;
            PostOrderRec(node.Left, result);
            PostOrderRec(node.Right, result);
            result.Add(node.Data);
        }

        public List<T> LevelOrderTraversal()
        {
            var result = new List<T>();
            if (Root == null) return result;

            var queue = new Queue<BinaryTreeNode<T>>();
            queue.Enqueue(Root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current.Data);
                if (current.Left != null) queue.Enqueue(current.Left);
                if (current.Right != null) queue.Enqueue(current.Right);
            }

            return result;
        }

        public int GetHeight()
        {
            return GetHeightRec(Root);
        }

        private int GetHeightRec(BinaryTreeNode<T> node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(GetHeightRec(node.Left), GetHeightRec(node.Right));
        }

        public int CountNodes()
        {
            return CountRec(Root);
        }

        private int CountRec(BinaryTreeNode<T> node)
        {
            if (node == null) return 0;
            return 1 + CountRec(node.Left) + CountRec(node.Right);
        }
    }
}
