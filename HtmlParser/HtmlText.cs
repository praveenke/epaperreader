using System;
using System.Web;

namespace HtmlParser
{
	/// <summary>
	/// The HtmlText nodes represents a simple piece of text from the document.
	/// </summary>
	public class HtmlText: HtmlNode
	{
		private string text;

		/// <summary>
		/// This constructs a new nodes with the given text content.
		/// </summary>
		/// <param attributeName="text"></param>
		internal HtmlText(string text)
		{
			this.text = text;
		}

		/// <summary>
		/// This is the text associated with this nodes.
		/// </summary>
		public string Text
		{
			get { return this.text; }
			set { this.text = value; }
		}

		/// <summary>
		/// This will return the text for outputting inside an Html document.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.text;
		}

		/// <summary>
		/// This will return the Html to represent this text object.
		/// </summary>
		public override string Html
		{
            get { return this.text;  }
}

	}
}
