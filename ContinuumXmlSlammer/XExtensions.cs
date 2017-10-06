using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using System.Linq;

namespace ContinuumXmlSlammer
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Credit for GetAbsoluteXPath: 
    ///   Chaviero answer:
    ///   https://stackoverflow.com/questions/451950/get-the-xpath-to-an-xelement
    /// Credit for ShallowValue:
    ///   Microsoft Stooge:
    ///   https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/how-to-retrieve-the-shallow-value-of-an-element
    /// </remarks>
    public static class XExtensions
    {
        /// <summary>
        /// Get the absolute XPath to a given XElement, including the namespace.
        /// (e.g. "/a:people/b:person[6]/c:name[1]/d:last[1]").
        /// </summary>
        public static string GetAbsoluteXPath(this XElement element, string delimiter="/")
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Func<XElement, string> relativeXPath = e =>
            {
                int index = e.IndexPosition();

                var currentNamespace = e.Name.Namespace;

                string name;
                if (String.IsNullOrEmpty(currentNamespace.ToString()))
                {
                    name = e.Name.LocalName;
                }
                else
                {
                    //name = "*[local-name()='" + e.Name.LocalName + "']";
                    string namespacePrefix = e.GetPrefixOfNamespace(currentNamespace);
                    name = namespacePrefix + ":" + e.Name.LocalName;
                }

                // If the element is the root or has no sibling elements, no index is required
                return ((index == -1) || (index == -2)) ?
                    delimiter + name :
                    delimiter + name + delimiter + index.ToString();
                    
            };

            var ancestors = from e in element.Ancestors() select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) + relativeXPath(element);
        }

        /// <summary>
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned or -2 if element has no sibling elements.
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        public static int IndexPosition(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element.Parent == null)
            {
                // Element is root
                return -1;
            }

            if (element.Parent.Elements(element.Name).Count() == 1)
            {
                // Element has no sibling elements
                return -2;
            }

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException ("element has been removed from its parent.");
        }

        /// <summary>
        /// Returns the value of just this element, not the concatenated value of all child text elements.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string ShallowValue(this XElement element)
        {
            return element
                   .Nodes()
                   .OfType<XText>()
                   .Aggregate(new StringBuilder(),
                              (s, c) => s.Append(c),
                              s => s.ToString());
        }

    } // end of class XExtensions

} // end of namespace NZ01
