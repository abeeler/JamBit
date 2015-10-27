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
        public IEnumerator<TreeNode> SelectedNodes { get { return _selectedNodes.GetEnumerator(); } }

        public MultipleSelectTreeView()
        {
            InitializeComponent();
            HideSelection = false;
            this.ForeColor = new TreeNode().ForeColor;
            this.BackColor = new TreeNode().BackColor;
        }

        protected override void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseDoubleClick(e);
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
                        SelectNodes(e.Node);
                }
                else
                {
                    ClearSelection();
                    base.OnNodeMouseClick(e);
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
                }
            }
        }

        public void ClearSelection()
        {
            DeselectNodes(_selectedNodes.ToArray<TreeNode>());
        }
    }
}
