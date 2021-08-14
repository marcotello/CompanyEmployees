using System.Collections.Generic;
using System.Xml;

namespace Entities.LinkModels
{
    public class LinkCollectionWrapper<T> : LinkResourceBase
    {
        public List<T> Value { get; set; } = new List<T>();
        
        public LinkCollectionWrapper() {}

        public LinkCollectionWrapper(List<T> value)
        {
            Value = value;
        }

        private void WriteLinksToXml(string key, object value, XmlWriter writer)
        {
            writer.WriteStartElement(key);

            if (value.GetType() == typeof(List<Link>))
            {
                foreach (var val in value as List<Link>)
                {
                    writer.WriteStartElement(nameof(Link)); 
                    WriteLinksToXml(nameof(val.Href), val.Href, writer); 
                    WriteLinksToXml(nameof(val.Method), val.Method, writer); 
                    WriteLinksToXml(nameof(val.Rel), val.Rel, writer); writer.WriteEndElement();
                }
            }
            else
            {
                writer.WriteString(value.ToString());
            }
        }
    }
}