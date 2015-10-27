using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerControlsLibrary
{
    public partial class MultipleSelectTreeView : TreeView
    {
        private List<TreeNode> _selectedNodes = new List<TreeNode>();
        public IEnumerable<TreeNode> SelectedNodes { get { return _selectedNodes; } }
        public int SelectedCount { get { return _selectedNodes.Count; } }
        public TreeNode LastSelectedNode { get; set; }

        public MultipleSelectTreeView()
        {
            InitializeComponent();
            HideSelection = false;
            this.ForeColor = new TreeNode().ForeColor;
            this.BackColor = new TreeNode().BackColor;
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            base.OnBeforeSelect(e);

            base.OnBeforeSelect(e);
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys == Keys.Control)
                {
                    if (_selectedNodes.Contains(e.Node))
                        DeselectNodes(e.Node);
                    else
                    {
                        SelectNodes(e.Node);
                        LastSelectedNode = e.Node;
                    }
                }
                else if (ModifierKeys == Keys.Shift && LastSelectedNode != null)
                {
                    if (e.Node == LastSelectedNode)
                        return;

                    if (e.Node.Parent == LastSelectedNode.Parent)
                    {                        
                        if (LastSelectedNode.Index < e.Node.Index)
                            while (LastSelectedNode != e.Node)
                            {
                                LastSelectedNode = LastSelectedNode.NextVisibleNode;
                                SelectNodes(LastSelectedNode);
                            }
                        else
                            while (LastSelectedNode != e.Node)
                            {
                                LastSelectedNode = LastSelectedNode.PrevVisibleNode;
                                SelectNodes(LastSelectedNode);
                            }
                    }
                }
                else
                {
                    ClearSelection();
                    SelectNodes(e.Node);
                    LastSelectedNode = e.Node;
                }
            }
        }

        public void SelectNodes(params TreeNode[] nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
                _selectedNodes.Add(node);
            }
        }

        public void DeselectNodes(params TreeNode[] nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (_selectedNodes.Contains(node))
                {
                    node.BackColor = this.BackColor;
                    node.ForeColor = this.ForeColor;
                    _selectedNodes.Remove(node);

                    if (node == LastSelectedNode)
                        LastSelectedNode = null;
                }
            }
        }

        public void ClearSelection()
        {
            DeselectNodes(_selectedNodes.ToArray<TreeNode>());
        }
    }
}
