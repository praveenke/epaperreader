using System;
using System.Text;

namespace HtmlParser
{
	/// <summary>
	/// The HtmlElement object represents any Html element. An element has a attributeName
	/// and zero or more attributes.
	/// </summary>
	public class HtmlElement: HtmlNode
    {
        #region private member fields
        private string name;
        private HtmlNodeCollection nodes;
        private HtmlAttributeCollection attributes;
        private bool terminated;
        private bool explicitlyTerminated;
        #endregion

        /// <summary>
		/// This constructs a new Html element with the specified tag attributeName.
		/// </summary>
		public HtmlElement(string name)
		{
			this.nodes = new HtmlNodeCollection(this);
			this.attributes = new HtmlAttributeCollection(this);
			this.name = name;
		}

		/// <summary>
		/// This is the tag attributeName of the element. e.g. BR, BODY, TABLE etc.
		/// </summary>
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// This is the collection of all child nodes of this one. If this nodes is actually
		/// a text nodes, this will throw an InvalidOperationException exception.
		/// </summary>
		public HtmlNodeCollection Nodes
		{
			get
			{
				if(IsText())
				{
					throw new InvalidOperationException("An HtmlText nodes does not have child nodes");
				}
				return this.nodes;
			}
		}

		/// <summary>
		/// This is the collection of attributes associated with this element.
		/// </summary>
		public HtmlAttributeCollection Attributes
		{
			get { return this.attributes; }
		}

		/// <summary>
		/// This flag indicates that the element is explicitly closed using the "<attributeName/>" method.
		/// </summary>
		internal bool IsTerminated
		{
			get { return Nodes.Count == 0 && (this.terminated || this.explicitlyTerminated); }
			set { this.terminated = value; }
		}

        /// <summary>
        /// This flag indicates that the element is explicitly closed using the "</tagName>" method.
        /// </summary>
        internal bool IsExplicitlyTerminated
        {
            get { return this.explicitlyTerminated; }
            set { this.explicitlyTerminated = value; }
        }

		/// <summary>
		/// This will return the Html representation of this element.
		/// </summary>
		public override string ToString()
		{
            StringBuilder html = new StringBuilder();
            html.Append("<" + name);
			foreach( HtmlAttribute attribute in this.attributes)
			{
                html.Append(" " + attribute.ToString());
			}
            html.Append(">");
            return html.ToString();
		}

		public string Text
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach(HtmlNode node in this.nodes)
				{
                    HtmlText textNode = node as HtmlText;
					if(null != textNode)
					{
						stringBuilder.Append(textNode.Text );
					}
				}
				return stringBuilder.ToString();
			}
		}

		/// <summary>
		/// This will return the Html for this element and all subnodes.
		/// </summary>
		public override string Html
		{
			get
			{
				StringBuilder html = new StringBuilder();
				html.Append("<" + name);
				foreach(HtmlAttribute attribute in this.attributes)
				{
					html.Append(" " + attribute.Html);
				}
				if(this.nodes.Count > 0)
				{
					html.Append(">");
                    foreach (HtmlNode node in this.nodes)
					{
                        html.Append(node.Html);
                    }
					html.Append("</" + this.name + ">");
                }
				else
				{
					if(this.explicitlyTerminated)
					{
						html.Append("></" + this.name + ">");
                    }
					else if(this.terminated)
					{
						html.Append(" />");
                    }
					else
					{
						html.Append(">");
                    }
                }
				return html.ToString();
			}
		}
	}
}
