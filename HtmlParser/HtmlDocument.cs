using System;
using System.IO;
using System.Text;
using System.Collections;

namespace HtmlParser
{
	/// <summary>
	/// This is the basic Html document object used to represent a sequence of Html.
	/// </summary>
	public class HtmlDocument
	{
		HtmlNodeCollection nodes = new HtmlNodeCollection(null);

		/// <summary>
		/// This will create a new document object by parsing the Html specified.
		/// </summary>
		/// <param attributeName="html">The Html to parse.</param>
        private HtmlDocument(Stream stream)
		{
			this.nodes = HtmlParser.Parse(stream);
		}

        private HtmlDocument(StreamReader reader)
        {
            this.nodes = HtmlParser.Parse(reader);
        }
        
        /// <summary>
		/// This is the collection of nodes used to represent this document.
		/// </summary>
		public HtmlNodeCollection Nodes
		{
			get { return this.nodes; }
		}

		/// <summary>
		/// This will create a new document object by parsing the Html specified.
		/// </summary>
        public static HtmlDocument Create(Stream stream)
		{
            return new HtmlDocument(stream);
		}

        public static HtmlDocument Create(StreamReader reader)
        {
            return new HtmlDocument(reader);
        }
        
        /// <summary>
		/// This will return the Html used to represent this document.
		/// </summary>
		public string Html
		{
			get
			{
				StringBuilder html = new StringBuilder();
				foreach(HtmlNode node in Nodes)
				{
					html.Append(node.Html);
				}
				return html.ToString();
			}
		}
	}
}
