using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Data.Trees
{
    /// <summary>
    /// Node in an N-ary general hierarchical tree.
    /// Used for organizing municipal service requests by Directorate, Category, Suburb/Ward, and Status.
    /// </summary>
    public class GeneralTreeNode<T>
    {
        public string Title { get; set; }
        public T Data { get; set; }
        public List<GeneralTreeNode<T>> Children { get; set; }
        public GeneralTreeNode<T> Parent { get; set; }

        public GeneralTreeNode(string title, T data = default(T))
        {
            Title = title;
            Data = data;
            Children = new List<GeneralTreeNode<T>>();
        }

        public GeneralTreeNode<T> AddChild(string title, T data = default(T))
        {
            var child = new GeneralTreeNode<T>(title, data) { Parent = this };
            Children.Add(child);
            return child;
        }

        public int CountDescendants()
        {
            int count = Children.Count;
            foreach (var child in Children)
            {
                count += child.CountDescendants();
            }
            return count;
        }

        public override string ToString()
        {
            return $"{Title} ({Children.Count} children)";
        }
    }

    /// <summary>
    /// Represents the overall municipal administrative hierarchy as an N-ary Tree.
    /// Provides methods to populate a Windows Forms TreeView control directly.
    /// </summary>
    public class GeneralTree<T>
    {
        public GeneralTreeNode<T> Root { get; set; }

        public GeneralTree(string rootTitle, T rootData = default(T))
        {
            Root = new GeneralTreeNode<T>(rootTitle, rootData);
        }

        /// <summary>
        /// Converts this GeneralTree structure into WinForms TreeNode objects for visual navigation.
        /// </summary>
        public System.Windows.Forms.TreeNode ToWinFormsTreeNode()
        {
            return ConvertNode(Root);
        }

        private System.Windows.Forms.TreeNode ConvertNode(GeneralTreeNode<T> node)
        {
            var winNode = new System.Windows.Forms.TreeNode(node.Title)
            {
                Tag = node.Data
            };

            foreach (var child in node.Children)
            {
                winNode.Nodes.Add(ConvertNode(child));
            }

            return winNode;
        }
    }
}
