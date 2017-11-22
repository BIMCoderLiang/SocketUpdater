using System.Xml;

namespace UpdaterShare.Utility
{
    public static class XmlUtils
    {
        /// <summary>
        /// Get Value By Key
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="rootNodeName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string GetValueByKey(string xmlFilePath, string rootNodeName, string keyName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            XmlNode xmlRootNode = xmlDoc.SelectSingleNode(rootNodeName);
            XmlNodeList xnl = xmlRootNode?.ChildNodes;
            if (xnl?.Count > 0)
            {
                foreach (XmlElement xe in xnl)
                {
                    foreach (XmlAttribute attr in xe.Attributes)
                    {
                        if (attr.Name.Equals(keyName))
                        {
                            return attr.InnerXml;
                        }
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Insert Node
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeName"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XmlElement InsertNode(XmlDocument xmlDoc, string nodeName, string keyName, string value)
        {
            XmlElement xn = xmlDoc.CreateElement(nodeName);
            xn.SetAttribute(keyName, value);
            return xn;
        }
    }
}
