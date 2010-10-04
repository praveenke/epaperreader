using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace HtmlParser
{
    /// <summary>
    /// Main Html parser class.
    /// Bascially, this class will build a tree containing HtmlNode elements.
    /// </summary>
    internal class HtmlParser
    {
        #region private enums
        private enum ParserStatus
        {
            TagStart,
            TagEnd,
            Tag,
            Text,
            Attribute,
            AttributeValue,
            SgmlComment,
            Comment,
            InsideSingleQuotes,
            InsideDoubleQuotes,
        }
        #endregion

        private HtmlParser()
        {
        }

        #region The main parser

        /// <summary>
        /// This will parse a string containing Html and will produce a domain tree.
        /// </summary>
        public static HtmlNodeCollection Parse(Stream stream)
        {            
            return Parse(new StreamReader(stream));
        }

        public static HtmlNodeCollection Parse(StreamReader reader)
        {
            Queue<string> tokens;

            tokens = HtmlParser.GetTokens(reader);

            return BuildNodeCollection(tokens);

        }

        /// <summary>
        /// This will move all the nodes from the specified index to the new parent.
        /// </summary>
        private static void MoveNodesDown(ref HtmlNodeCollection nodes, int index, HtmlElement newParent)
        {
            int count = nodes.Count;

            for (int i = index; i < count; i++)
            {
                ((HtmlElement)newParent).Nodes.Add(nodes[i]);
                nodes[i].SetParent(newParent);
            }

            for (int i = index; i < count; i++)
            {
                nodes.RemoveAt(index);
            }
            newParent.IsExplicitlyTerminated = true;
        }

        /// <summary>
        /// This will find the corresponding opening tag for the named one. This is identified as
        /// the most recently read nodes with the same attributeName, but with no child nodes.
        /// </summary>
        private static int FindTagOpenNodeIndex(HtmlNodeCollection nodes, string name)
        {
            for (int index = nodes.Count - 1; index >= 0; index--)
            {
                HtmlElement element = nodes[index] as HtmlElement;
                if (null != element && (string.Compare(element.Name, name, true, CultureInfo.InvariantCulture) == 0) && element.Nodes.Count == 0 && !element.IsTerminated)
                {
                    return index;
                }
            }
            return -1;
        }

        #endregion

        #region Parse tokens
        private static HtmlNodeCollection BuildNodeCollection(Queue<string> tokens)
        {
            HtmlNodeCollection nodes = new HtmlNodeCollection(null);
            HtmlElement element = null;
            string current;

            while (tokens.Count > 0)
            {
                current = tokens.Dequeue();
                switch (current)
                {
                    case ("<"):
                        // Read open tag

                        if (tokens.Count == 0)
                            break;

                        current = tokens.Dequeue();
                        element = new HtmlElement(current);

                        // read the attributes and values
                        while (tokens.Count > 0 && (current = tokens.Dequeue()) != ">" && current != "/>")
                        {
                            string attribute_name = current;
                            if (tokens.Count > 0 && tokens.Peek() == "=")
                            {
                                tokens.Dequeue();
                                current = (tokens.Count > 0) ? tokens.Dequeue() : null;
                                HtmlAttribute attribute = new HtmlAttribute(attribute_name, HttpUtility.HtmlDecode(current));
                                element.Attributes.Add(attribute);
                            }
                            else //if (tokens.Count == 0)
                            {
                                // Null-attributeValue attribute
                                HtmlAttribute attribute = new HtmlAttribute(attribute_name);
                                element.Attributes.Add(attribute);
                            }
                        }
                        nodes.Add(element);

                        if (current == "/>")
                        {
                            element.IsTerminated = true;
                            element = null; //could not have any sub elements
                        }
                        else if (current == ">")
                        {
                            continue;
                        }
                        break;
                    case (">"):
                        continue;
                    case ("</"):
                        // Read close tag

                        if (tokens.Count == 0)
                            break;

                        current = tokens.Dequeue();

                        int open_index = FindTagOpenNodeIndex(nodes, current);
                        if (open_index != -1)
                        {
                            MoveNodesDown(ref nodes, open_index + 1, (HtmlElement)nodes[open_index]);
                        }

                        // Skip to the end of this tag
                        while (tokens.Count > 0 && (current = tokens.Dequeue()) != ">")
                        {
                            //shouldn't happen
                        }
                        element = null;
                        break;
                    default:
                        HtmlText node = new HtmlText(current);
                        nodes.Add(node);
                        break;
                }
            }
            return nodes;

        }
        #endregion

        #region Html tokenizer
        private static Queue<string> GetTokens(StreamReader reader)
        {
            StringBuilder builder = new StringBuilder();
            Queue<string> tokens = new Queue<string>();
            int actualChar;
            Stack<ParserStatus> parserState = new Stack<ParserStatus>();

            ParserStatus status = ParserStatus.Text;
            parserState.Push(ParserStatus.Text);
            bool inScript = false;
            string current;

            actualChar = reader.Read();
            while (actualChar > -1)
            {

                switch (status)
                {
                    case ParserStatus.InsideDoubleQuotes:
                        builder.Append((char)actualChar);
                        if (actualChar == '"') // end of quotation reached
                        {
                            status = parserState.Pop(); //get back to the latest parser state
                            if (status == ParserStatus.AttributeValue)
                            {
                                tokens.Enqueue(builder.ToString());
                                builder = new StringBuilder();
                                status = ParserStatus.Attribute;
                            }
                        }
                        break;
                    case ParserStatus.InsideSingleQuotes:
                        builder.Append((char)actualChar);
                        if (actualChar == '\'') // end of quotation reached
                        {
                            status = parserState.Pop(); //get back to the latest parser state
                            if (status == ParserStatus.AttributeValue)
                            {
                                tokens.Enqueue(builder.ToString());
                                builder = new StringBuilder();
                                status = ParserStatus.Attribute;
                            }
                        }
                        break;
                    case ParserStatus.Text:
                        if (inScript)
                        {
                            if (actualChar == '<' && reader.Peek() == '/')
                            {
                                reader.Read();
                                StringBuilder stack = new StringBuilder();
                                const string script = "script";

                                for (int i = script.Length; i>0; i--)
                                {
                                    actualChar = reader.Read();
                                    if (char.ToLower((char)actualChar, CultureInfo.InvariantCulture) != script[script.Length - i])
                                    {
                                        builder.Append("</");
                                        stack.Append((char)actualChar);
                                        builder.Append(stack.ToString());
                                        inScript = false;
                                        break;
                                    }
                                    stack.Append((char)actualChar);
                                }
                                if (inScript)
                                {
                                    //if we are here, this was a script block ending
                                    if (builder.Length > 0)
                                    {
                                        tokens.Enqueue(builder.ToString());
                                    }
                                    tokens.Enqueue("</");
                                    builder = new StringBuilder(script);
                                    status = ParserStatus.Tag;
                                    inScript = false;
                                }
                                else
                                {
                                    inScript = true;
                                }
                            }
                            else
                            {
                                builder.Append((char)actualChar);
                                break;
                            }
                            break;
                        }
                        else
                        {
                            switch (actualChar)
                            {
                                case '<':
                                    if (builder.Length > 0)
                                    {
                                        tokens.Enqueue(builder.ToString());
                                    }
                                    status = ParserStatus.TagStart;
                                    if (reader.Peek() == '/')
                                    {
                                        reader.Read();
                                        tokens.Enqueue("</");
                                    }
                                    else
                                    {
                                        tokens.Enqueue("<");
                                    }
                                    builder = new StringBuilder();
                                    break;
                                default:
                                    builder.Append((char)actualChar);
                                    break;
                            }
                        }
                        break;
                    case ParserStatus.TagStart:
                        switch (actualChar)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                break;  // read further
                            case '!':
                                status = ParserStatus.SgmlComment;
                                builder.Append((char)actualChar);
                                break;
                            case '>':
                                throw new InvalidDataException();
                            default:
                                status = ParserStatus.Tag;
                                builder = new StringBuilder();
                                builder.Append((char)actualChar);
                                break;
                        }
                        break;
                    case ParserStatus.Comment:
                    case ParserStatus.SgmlComment:
                        switch (actualChar)
                        {
                            case '>':
                                if (status == ParserStatus.SgmlComment)
                                {
                                    // end of SGML SgmlComment
                                    tokens.Enqueue(builder.ToString());
                                    tokens.Enqueue(">");
                                    builder = new StringBuilder();
                                    status = ParserStatus.Text;
                                }
                                else
                                {
                                    builder.Append((char)actualChar);
                                }
                                break;
                            case '-':
                                if (status == ParserStatus.SgmlComment && reader.Peek() == '-')
                                {
                                    reader.Read();
                                    status = ParserStatus.Comment;
                                    builder.Append('-');
                                }
                                else if (status == ParserStatus.Comment && reader.Peek() == '-')
                                {
                                    reader.Read();
                                    status = ParserStatus.SgmlComment;
                                    builder.Append('-');
                                }
                                builder.Append((char)actualChar);
                                break;
                            default:
                                builder.Append((char)actualChar);
                                break;
                        }
                        break;
                    case ParserStatus.Tag:
                        switch (actualChar)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                current = builder.ToString();
                                status = ParserStatus.Attribute;
                                tokens.Enqueue(current);
                                builder = new StringBuilder();
                                //tag attributeName ends here. check whether its a script
                                if ("script".Equals(current.ToLower(CultureInfo.InvariantCulture)))
                                {
                                    inScript = true;
                                }
                                break;
                            case '>':
                                tokens.Enqueue(builder.ToString());
                                tokens.Enqueue(">");
                                builder = new StringBuilder();
                                status = ParserStatus.Text;
                                break;
                            case '/':
                                tokens.Enqueue(builder.ToString());
                                builder = new StringBuilder();
                                builder.Append((char)actualChar);
                                status = ParserStatus.TagEnd;
                                break;
                            default:
                                builder.Append((char)actualChar);
                                break;
                        }
                        break;
                    case ParserStatus.TagEnd:
                        if (actualChar == '>')
                        {
                            builder.Append((char)actualChar);
                            tokens.Enqueue(builder.ToString());
                            builder = new StringBuilder();
                            status = ParserStatus.Text;
                        }
                        break;
                    case ParserStatus.Attribute:
                        switch (actualChar)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                status = ParserStatus.Attribute;
                                if (builder.Length > 0)
                                {
                                    tokens.Enqueue(builder.ToString());
                                    builder = new StringBuilder();
                                }
                                break;
                            case '>':
                                if (builder.Length > 0)
                                {
                                    tokens.Enqueue(builder.ToString());
                                }
                                tokens.Enqueue(">");
                                builder = new StringBuilder();
                                status = ParserStatus.Text;
                                break;
                            case '/':
                                if (builder.Length > 0)
                                {
                                    tokens.Enqueue(builder.ToString());
                                }
                                builder = new StringBuilder();
                                builder.Append((char)actualChar);
                                status = ParserStatus.TagEnd;
                                break;
                            case '=':
                                tokens.Enqueue(builder.ToString());
                                tokens.Enqueue("=");
                                builder = new StringBuilder();
                                status = ParserStatus.AttributeValue;
                                break;
                            default:
                                builder.Append((char)actualChar);
                                break;
                        }
                        break;
                    case ParserStatus.AttributeValue:
                        switch (actualChar)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                if (builder.Length > 0)
                                {
                                    tokens.Enqueue(builder.ToString());
                                }
                                builder = new StringBuilder();
                                status = ParserStatus.Attribute;
                                break;
                            case '>':
                                if (builder.Length > 0)
                                {
                                    tokens.Enqueue(builder.ToString());
                                }
                                tokens.Enqueue(">");
                                builder = new StringBuilder();
                                status = ParserStatus.Text;
                                break;
                            case '/':
                                if (reader.Peek() == '>')
                                {
                                    if (builder.Length > 0)
                                    {
                                        tokens.Enqueue(builder.ToString());
                                    }
                                    builder = new StringBuilder();
                                    builder.Append((char)actualChar);
                                    status = ParserStatus.TagEnd;
                                }
                                else
                                {
                                    builder.Append((char)actualChar);
                                }
                                break;
                            case '\'':
                                builder.Append((char)actualChar);
                                parserState.Push(status);
                                status = ParserStatus.InsideSingleQuotes;
                                break;
                            case '"':
                                builder.Append((char)actualChar);
                                parserState.Push(status);
                                status = ParserStatus.InsideDoubleQuotes;
                                break;
                            default:
                                builder.Append((char)actualChar);
                                break;
                        }
                        break;
                    default:
                        break;
                }
                actualChar = reader.Read();
            }

            return tokens;

        }

        #endregion
    }
}
