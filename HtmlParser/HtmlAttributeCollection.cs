using System;
using System.Collections.Generic;
using System.Globalization;

namespace HtmlParser
{
    /// <summary>
    /// This is a collection of attributes. Typically, this is associated with a particular
    /// element. This collection is searchable by both the index and the attributeName of the attribute.
    /// </summary>
    public class HtmlAttributeCollection : List<HtmlAttribute>
    {
        HtmlElement element;

        #region .ctor
        /// <summary>
        /// This will create an empty collection of attributes.
        /// </summary>
        /// <param attributeName="element"></param>
        internal HtmlAttributeCollection(HtmlElement element)
        {
            this.element = element;
        }
        #endregion

        public bool Contains(string name)
        {
            return this.IndexOf(name) > -1;
        }

        /// <summary>
        /// This will return the index of the attribute with the specified attributeName. If it is
        /// not found, this method will return -1.
        /// </summary>
        public int IndexOf(string name)
        {
            for (int index = 0; index < this.Count; index++)
            {
                if (string.Compare(name, this[index].Name, true, CultureInfo.InvariantCulture) == 0)
                {
                    return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// This overload allows you to have direct access to an attribute by providing
        /// its attributeName. If the attribute does not exist, null is returned.
        /// </summary>
        public HtmlAttribute this[string name]
        {
            get
            {
                int index = this.IndexOf(name);
                return index > -1 ? this[index] : null;
            }
        }

        /// <summary>
        /// get  the parent HtmlElement
        /// </summary>
        public HtmlElement HtmlElement
        {
            get { return this.element; }
        }
    }
}
