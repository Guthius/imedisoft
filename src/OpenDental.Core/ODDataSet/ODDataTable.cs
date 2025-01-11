using System.Collections.Generic;
using System.Xml;

namespace OpenDentBusiness
{
    public class ODDataTable
    {
        public readonly string Name;
        public readonly List<SortedList<string, string>> Rows;

        public ODDataTable()
        {
            Rows = new List<SortedList<string, string>>();
            Name = "";
        }

        public ODDataTable(string xmlData)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlData);
            Rows = new List<SortedList<string, string>>();
            Name = "";
            var xmlNodeListRows = xmlDocument.DocumentElement.ChildNodes;
            for (var i = 0; i < xmlNodeListRows.Count; i++)
            {
                var odDataRow = new SortedList<string, string>();
                if (Name == "")
                {
                    Name = xmlNodeListRows[i].Name;
                }

                foreach (XmlNode xmlNodeCell in xmlNodeListRows[i].ChildNodes)
                {
                    odDataRow.Add(xmlNodeCell.Name, xmlNodeCell.InnerXml);
                }

                Rows.Add(odDataRow);
            }
        }
    }
}