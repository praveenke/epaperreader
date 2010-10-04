using System;

namespace HtmlParser
{
	/// <summary>
	/// The HtmlNode is the base for all objects that may appear in Html. Currently, 
	/// this implemention only supports HtmlText and HtmlElement nodes types.
	/// </summary>
	public abstract class HtmlNode
	{
		private HtmlElement parent;

		/// <summary>
		/// This will render the nodes as it would appear in Html.
		/// </summary>
		/// <returns></returns>
		public abstract override string ToString();

		/// <summary>
		/// This will return the parent of this nodes, or null if there is none.
		/// </summary>
		public HtmlElement Parent
		{
			get { return this.parent; }
		}

		/// <summary>
		/// This will return the next sibling nodes. If this is the last one, it will return null.
		/// </summary>
		public HtmlNode Next
		{
            get
            {
                int index = Index + 1;
                if (index == 0 || this.parent.Nodes.Count == index)
                {
                    return null; 
                }
                return Parent.Nodes[index];
            }
		}

		/// <summary>
		/// This will return the previous sibling nodes. If this is the first one, it will return null.
		/// </summary>
		public HtmlNode Previous
		{
			get
			{
                int index = Index - 1;
                if (index < 0)
                {
                    return null;
                }
                return Parent.Nodes[index];
			}
		}

		/// <summary>
		/// This will return the first child nodes. If there are no children, this
		/// will return null.
		/// </summary>
		public HtmlNode FirstChild
		{
            get
            {
                HtmlElement element = this as HtmlElement;
                if (null != element && element.Nodes.Count > 0)
                {
                    return element.Nodes[0];
                }
                return null;
            }
		}

		/// <summary>
		/// This will return the last child nodes. If there are no children, this
		/// will return null.
		/// </summary>
		public HtmlNode LastChild
		{
			get
			{
                HtmlElement element = this as HtmlElement;
                if (null != element && element.Nodes.Count > 0)
                {
                    return element.Nodes[element.Nodes.Count - 1];
                }
                return null;
			}
		}

		/// <summary>
		/// This will return the index position within the parent's nodes that this one resides.
		/// If this is not in a collection, this will return -1.
		/// </summary>
		public int Index
		{
			get { return null == this.parent ? -1 : parent.Nodes.IndexOf(this); }
		}

		/// <summary>
		/// This will return true if the nodes passed is a descendent of this nodes.
		/// </summary>
		/// <param attributeName="nodes">The nodes that might be the parent or grandparent (etc.)</param>
		/// <returns>True if this nodes is a descendent of the one passed in.</returns>
		public bool IsDescendentOf(HtmlNode node)
		{
			HtmlNode current = this.parent;
            while (current != null)
			{
				if( current == node )
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		/// <summary>
		/// This will return true if the nodes passed is one of the children or grandchildren of this nodes.
		/// </summary>
		/// <param attributeName="nodes">The nodes that might be a child.</param>
		/// <returns>True if this nodes is an ancestor of the one specified.</returns>
		public bool IsAncestorOf(HtmlNode node)
		{
            if (null != node)
                return node.IsDescendentOf(this);
            else
                return false;
		}

		/// <summary>
		/// This will return the ancstor that is common to this nodes and the one specified.
		/// </summary>
		/// <param attributeName="nodes">The possible nodes that is relative</param>
		/// <returns>The common ancestor, or null if there is none</returns>
		public HtmlNode GetCommonAncestor(HtmlNode node)
		{
			HtmlNode current = this;
            while (current != null)
			{
				while( node != null )
				{
                    if (current == node)
					{
                        return current;
					}
					node = node.Parent;
				}
                current = current.Parent;
			}
			return null;
		}

		/// <summary>
		/// This will remove this nodes and all child nodes from the tree. If this
		/// is a root nodes, this operation will do nothing.
		/// </summary>
		public void Remove()
		{
			if(null != this.parent)
			{
				this.parent.Nodes.RemoveAt(this.Index);
			}
		}

		/// <summary>
		/// Internal method to maintain the identity of the parent nodes.
		/// </summary>
		/// <param attributeName="parentNode">The parent nodes of this one</param>
        internal void SetParent(HtmlElement parent)
		{
			this.parent = parent;
		}

		/// <summary>
		/// This will return the full Html to represent this nodes (and all child nodes).
		/// </summary>
		public abstract string Html { get; }

		public bool IsText()
		{
			return this is HtmlText;
		}

		public bool IsElement()
		{
			return this is HtmlElement;
		}
	}

}
