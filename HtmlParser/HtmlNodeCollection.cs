using System;
using System.Collections.Generic;
using System.Globalization;

namespace HtmlParser
{
    /// <summary>
    /// This object represents a collection of HtmlNodes, which can be either HtmlText
    /// or HtmlElement objects. The order in which the nodes occur directly corresponds
    /// to the order in which they appear in the original Html document.
    /// </summary>
    public class HtmlNodeCollection : List<HtmlNode>
    {
        private HtmlElement parent;

        /// <summary>
        /// A collection is usually associated with a parent nodes (an HtmlElement, actually)
        /// but you can pass null to implement an abstracted collection.
        /// </summary>
        internal HtmlNodeCollection(HtmlElement parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// This will add a node to the collection.
        /// </summary>
        public new int Add(HtmlNode node)
        {
            if (null == node)
                throw new ArgumentNullException("node");
            if (null != this.parent)
                node.SetParent(parent);
            base.Add(node);
            return this.IndexOf(node);
        }

        /// <summary>
        /// This will insert a nodes at the given position
        /// </summary>
        public new void Insert(int index, HtmlNode node)
        {
            if (null == node)
                throw new ArgumentNullException("node");
            if (null != this.parent)
                node.SetParent(this.parent);
            base.Insert(index, node);
        }

        /// <summary>
        /// This property allows you to change the nodes at a particular position in the
        /// collection.
        /// </summary>
        public new HtmlNode this[int index]
        {
            get { return base[index]; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");
                if (null != parent)
                    value.SetParent(this.parent);
                base[index] = value;
            }
        }

        /// <summary>
        /// This allows you to directly access the first element in this collection with the given attributeName.
        /// If the nodes does not exist, this will return null.
        /// </summary>
        public HtmlNode this[string name]
        {
            get
            {
                HtmlNodeCollection results = GetByName(name, false);
                if (results.Count > 0)
                {
                    return results[0];
                }
                return null;
            }
        }

        /// <summary>
        /// This will search through this collection of nodes for all elements with the
        /// specified attributeName. 
        /// </summary>
        public HtmlNodeCollection GetByName(string name)
        {
            return GetByName(name, true);
        }

        /// <summary>
        /// This will search through this collection of nodes for all elements with the
        /// specified attributeName. Recursively search is possible too. This search is 
        /// guaranteed to return nodes in the order in which they are found in the document.
        /// </summary>
        public HtmlNodeCollection GetByName(string name, bool recursive)
        {
            HtmlNodeCollection results = new HtmlNodeCollection(null);
            foreach (HtmlNode node in this)
            {
                HtmlElement element = node as HtmlElement;
                if (null != element)
                {
                    if (string.Compare(element.Name, name, true, CultureInfo.InvariantCulture) == 0)
                    {
                        results.Add(node);
                    }
                    if (recursive)
                    {
                        foreach (HtmlNode matchedChild in element.Nodes.GetByName(name, true))
                        {
                            results.Add(matchedChild);
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// This will search through this collection of nodes for all elements with the an
        /// attribute with the given attributeName. 
        /// </summary>
        public HtmlNodeCollection FindByAttributeName(string attributeName)
        {
            return FindByAttributeName(attributeName, true);
        }

        /// <summary>
        /// This will search through this collection of nodes for all elements with the an
        /// attribute with the given attributeName. 
        /// </summary>
        public HtmlNodeCollection FindByAttributeName(string attributeName, bool recursive)
        {
            HtmlNodeCollection results = new HtmlNodeCollection(null);
            foreach (HtmlNode node in this)
            {
                HtmlElement element = node as HtmlElement;
                if (null != element)
                {
                    if (element.Attributes.Contains(attributeName))
                    {
                        results.Add(node);
                    }
                    if (recursive)
                    {
                        foreach (HtmlNode matchedChild in element.Nodes.FindByAttributeName(attributeName, true))
                        {
                            results.Add(matchedChild);
                        }
                    }
                }
            }
            return results;
        }

        public HtmlNodeCollection FindByAttributeNameValue(string attributeName, string attributeValue)
        {
            return FindByAttributeNameValue(attributeName, attributeValue, true);
        }

        public HtmlNodeCollection FindByAttributeNameValue(string attributeName, string attributeValue, bool recursive)
        {
            HtmlNodeCollection results = new HtmlNodeCollection(null);
            foreach (HtmlNode node in this)
            {
                HtmlElement element = node as HtmlElement;
                if (null != element)
                {

                    HtmlAttribute attribute = element.Attributes[attributeName];
                    {
                        if (attribute != null && string.Compare(attribute.Value, attributeValue, true, CultureInfo.InvariantCulture) == 0)
                        {
                            results.Add(node);
                        }
                    }
                    if (recursive)
                    {
                        foreach (HtmlNode matchedChild in element.Nodes.FindByAttributeNameValue(attributeName, attributeValue, true))
                        {
                            results.Add(matchedChild);
                        }
                    }
                }
            }
            return results;
        }
    }
}
