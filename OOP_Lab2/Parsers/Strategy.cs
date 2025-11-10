using OOP_Lab2.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace OOP_Lab2.Parsers
{
    public interface IParser
    {
        public string selectedValue1 { get; set; }//Name
        public string selectedValue2 { get; set; }//Faculty
        public string selectedValue3 { get; set; }//Department
        public string selectedValue4 { get; set; }//Position
        public string selectedValue5 { get; set; }//Mail

        Dictionary<string, ObservableCollection<string>> Parse(string xmlPath);

        public void searchResult(string selectedValue1, string selectedValue2, string selectedValue3, string selectedValue4, string selectedValue5);
    }

    class SaxParser : IParser
    {
        public string selectedValue1 { get; set; }//Name
        public string selectedValue2 { get; set; }//Faculty
        public string selectedValue3 { get; set; }//Department
        public string selectedValue4 { get; set; }//Position
        public string selectedValue5 { get; set; }//Mail

        private readonly MainViewModel _vm;
        private string xmlPath;
        private XmlTextReader xmlReader;
        private string mainNodeName;

        private Dictionary<string, ObservableCollection<string>> attributeValues = new Dictionary<string, ObservableCollection<string>>();
        public SaxParser(MainViewModel vm)
        {
            _vm = vm;
            selectedValue1 = _vm.SelectedValue1;
            selectedValue2 = _vm.SelectedValue2;
            selectedValue3 = _vm.SelectedValue3;
            selectedValue4 = _vm.SelectedValue4;
            selectedValue5 = _vm.SelectedValue5;
        }
        public Dictionary<string, ObservableCollection<string>> Parse(string xmlPath)
        {
            this.xmlPath = xmlPath;
            xmlReader = new XmlTextReader(xmlPath);

            bool rootFound = false;
            bool firstAfterRootFound = false;

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (rootFound == false)
                    {
                        rootFound = true;
                        continue;
                    }

                    if (firstAfterRootFound == false)
                    {
                        mainNodeName = xmlReader.Name;
                        firstAfterRootFound = true;
                    }

                    if (xmlReader.Name == mainNodeName && xmlReader.HasAttributes)
                    {
                        while (xmlReader.MoveToNextAttribute())
                        {
                            if (!attributeValues.ContainsKey(xmlReader.Name))
                            {
                                attributeValues[xmlReader.Name] = new ObservableCollection<string>();
                            }

                            if (!attributeValues[xmlReader.Name].Contains(xmlReader.Value))
                            {
                                attributeValues[xmlReader.Name].Add(xmlReader.Value);
                            }
                        }
                    }
                }
            }
            return attributeValues;
        }

        public void searchResult(string selectedValue1, string selectedValue2, string selectedValue3, string selectedValue4, string selectedValue5)
        {
            var keys = attributeValues.Keys.ToList();
            var selectedValues = new List<string> { selectedValue1, selectedValue2, selectedValue3, selectedValue4, selectedValue5 };

            StringBuilder sb = new StringBuilder();
            xmlReader = new XmlTextReader(xmlPath);

            bool insideMainNode = false;
            StringBuilder currentNodeText = new StringBuilder();
            Dictionary<string, string> currentAttributes = new Dictionary<string, string>();

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == mainNodeName)
                {
                    insideMainNode = true;
                    currentAttributes.Clear();
                    currentNodeText.Clear();

                    if (xmlReader.HasAttributes)
                    {
                        while (xmlReader.MoveToNextAttribute())
                        {
                            currentAttributes[xmlReader.Name] = xmlReader.Value;
                        }
                        xmlReader.MoveToElement();
                    }

                    bool matches = true;

                    for (int i = 0; i < selectedValues.Count; i++)
                    {
                        if (selectedValues[i] != null)
                        {
                            if (!currentAttributes.ContainsKey(keys[i]) || currentAttributes[keys[i]] != selectedValues[i])
                            {
                                matches = false;
                                break;
                            }
                        }
                    }

                    if (matches)
                    {
                        getInfo(xmlReader, currentAttributes);
                        insideMainNode = false;
                        _vm.TextToView = sb.ToString();
                    }
                }

                void getInfo(XmlReader reader, Dictionary<string, string> currentAttributes)
                {
                    sb.AppendLine($"{reader.Name}:");

                    foreach (var attr in currentAttributes)
                    {
                        sb.AppendLine($"{attr.Key}: {attr.Value}");
                    }

                    if (!reader.IsEmptyElement)
                    {
                        int currentDepth = reader.Depth;

                        while (reader.Read() && reader.Depth > currentDepth)
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                var subAttrs = new Dictionary<string, string>();

                                if (reader.HasAttributes)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        subAttrs[reader.Name] = reader.Value;
                                    }                                    
                                    reader.MoveToElement();
                                }

                                if (reader.IsEmptyElement)
                                {
                                    sb.Append($"{reader.Name}: (empty)");
                                }
                                else
                                {
                                    getInfo(reader, subAttrs);
                                }
                            }
                            else if (reader.NodeType == XmlNodeType.Text)
                            {
                                string value = reader.Value.Trim();

                                if (!string.IsNullOrEmpty(value))
                                {
                                    sb.Append($" {value}\n");
                                }                               
                            }
                        }
                    }
                }
            }
            if (_vm.TextToView == null)
            {
                Shell.Current.DisplayAlert("Filter", "No attributes match the selected criteria.", "OK");
            }
        }
    }

    class DomParser : IParser
    {
        public string selectedValue1 { get; set; }//Name
        public string selectedValue2 { get; set; }//Faculty
        public string selectedValue3 { get; set; }//Department
        public string selectedValue4 { get; set; }//Position
        public string selectedValue5 { get; set; }//Mail

        private readonly MainViewModel _vm;
        private XmlDocument xmlDoc = new XmlDocument();
        private string mainNodeName;

        private Dictionary<string, ObservableCollection<string>> attributeValues = new Dictionary<string, ObservableCollection<string>>();

        public DomParser(MainViewModel vm)
        {
            _vm = vm;
            selectedValue1 = _vm.SelectedValue1;
            selectedValue2 = _vm.SelectedValue2;
            selectedValue3 = _vm.SelectedValue3;
            selectedValue4 = _vm.SelectedValue4;
            selectedValue5 = _vm.SelectedValue5;
        }

        public Dictionary<string, ObservableCollection<string>> Parse(string xmlPath)
        {
            xmlDoc.Load(xmlPath);
            XmlNode root = xmlDoc.DocumentElement;
            XmlNode mainNode = root.FirstChild;
            mainNodeName = mainNode.Name;

            var elements = xmlDoc.GetElementsByTagName(mainNodeName);

            foreach (XmlNode el in elements)
            {
                foreach (XmlAttribute attr in el.Attributes)
                {
                    if (!attributeValues.ContainsKey(attr.Name))
                    {
                        attributeValues[attr.Name] = new ObservableCollection<string>();
                    }

                    if (!attributeValues[attr.Name].Contains(attr.Value))
                    {
                        attributeValues[attr.Name].Add(attr.Value);
                    }
                }
            }
            return attributeValues;
        }

        public void searchResult(string selectedValue1, string selectedValue2, string selectedValue3, string selectedValue4, string selectedValue5)
        {
            var keys = attributeValues.Keys.ToList();
            var elmts = xmlDoc.GetElementsByTagName(mainNodeName);

            var filteredList = new List<XmlNode>();

            foreach (XmlNode el in elmts)
            {
                bool matches = true;

                if (selectedValue1 != null)
                {
                    var attr = el.Attributes[keys[0]];
                    if (attr == null || attr.Value != selectedValue1)
                    {
                        matches = false;
                    }  
                }

                if (selectedValue2 != null)
                {
                    var attr = el.Attributes[keys[1]];
                    if (attr == null || attr.Value != selectedValue2)
                    {
                        matches = false;
                    }
                }

                if (selectedValue3 != null)
                {
                    var attr = el.Attributes[keys[2]];
                    if (attr == null || attr.Value != selectedValue3)
                    {
                        matches = false;
                    }
                }

                if (selectedValue4 != null)
                {
                    var attr = el.Attributes[keys[3]];
                    if (attr == null || attr.Value != selectedValue4)
                    {
                        matches = false;
                    }
                }

                if (selectedValue5 != null)
                {
                    var attr = el.Attributes[keys[4]];
                    if (attr == null || attr.Value != selectedValue5)
                    {
                        matches = false;
                    }
                }

                if (matches)
                {
                    filteredList.Add(el);
                }
            }

            if (filteredList.Count == 0)
            {
                Shell.Current.DisplayAlert("Filter", "No teachers match the selected criteria.", "OK");
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var el in filteredList)
            {
                getInfo(el, sb);
            }

            _vm.TextToView = sb.ToString();
        }

        void getInfo(XmlNode element, StringBuilder sb)
        {
            if (element.Name == "#text")
            {
                return;
            }       

            sb.Append($"\n{element.Name}:");

            if (element.Attributes != null && element.Attributes.Count > 0)
            {
                foreach (XmlAttribute attr in element.Attributes)
                {
                    sb.Append($"\n {attr.Name}: {attr.Value}");
                }
            }

            if (element.HasChildNodes)
            {
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Text)
                    {
                        sb.Append($"\n{child.InnerText}");
                    }
                    else
                    {
                        getInfo(child, sb);
                    }
                }
            }
        }
    }

    class LinqToXmlParser : IParser
    {
        public string selectedValue1 { get; set; }//Ім'я
        public string selectedValue2 { get; set; }//Факультет
        public string selectedValue3 { get; set; }//Кафедра
        public string selectedValue4 { get; set; }//Посада
        public string selectedValue5 { get; set; }//Пошта

        private readonly MainViewModel _vm;
        private XDocument xmlDoc = new XDocument();
        private string mainNodeName;

        private Dictionary<string, ObservableCollection<string>> attributeValues = new Dictionary<string, ObservableCollection<string>>();

        public LinqToXmlParser(MainViewModel vm)
        {
            _vm = vm;
            selectedValue1 = _vm.SelectedValue1;
            selectedValue2 = _vm.SelectedValue2;
            selectedValue3 = _vm.SelectedValue3;
            selectedValue4 = _vm.SelectedValue4;
            selectedValue5 = _vm.SelectedValue5;
        }

        private void searchMainNodeName(string xmlPath)
        {
            xmlDoc = XDocument.Load(xmlPath);
            XElement mainNode = xmlDoc.Root.Elements().FirstOrDefault();
            mainNodeName = mainNode.Name.LocalName;
        }

        public Dictionary<string, ObservableCollection<string>> Parse(string xmlPath)
        {
            searchMainNodeName(xmlPath);
            var teachers = xmlDoc.Root.Elements(mainNodeName);

            foreach (var el in teachers)
            {
                foreach (XAttribute attr in el.Attributes())
                {
                    if (!attributeValues.ContainsKey(attr.Name.LocalName))
                    {
                        attributeValues[attr.Name.LocalName] = new ObservableCollection<string>();
                    }

                    if (!attributeValues[attr.Name.LocalName].Contains(attr.Value))
                    {
                        attributeValues[attr.Name.LocalName].Add(attr.Value);
                    }
                }
            }
            return attributeValues;
        }

        public void searchResult(string selectedValue1, string selectedValue2, string selectedValue3, string selectedValue4, string selectedValue5)
        {
            var keys = attributeValues.Keys.ToList();
            var filter = xmlDoc.Descendants(mainNodeName);

            if (selectedValue1 != null)
            {
                filter = (from mainNode in filter
                          where mainNode.Attribute((keys[0])) != null &&
                               mainNode.Attribute((keys[0])).Value == selectedValue1
                          select mainNode);
            }

            if (selectedValue2 != null)
            {
                filter = (from mainNode in filter
                          where mainNode.Attribute((keys[1])) != null &&
                                mainNode.Attribute((keys[1])).Value == selectedValue2
                          select mainNode);
            }

            if (selectedValue3 != null)
            {
                filter = (from mainNode in filter
                          where mainNode.Attribute((keys[2])) != null &&
                                mainNode.Attribute((keys[2])).Value == selectedValue3
                          select mainNode);
            }

            if (selectedValue4 != null)
            {
                filter = (from mainNode in filter
                          where mainNode.Attribute((keys[3])) != null &&
                                mainNode.Attribute((keys[3])).Value == selectedValue4
                          select mainNode);
            }

            if (selectedValue5 != null)
            {
                filter = (from mainNode in filter
                          where mainNode.Attribute((keys[4])) != null &&
                                mainNode.Attribute((keys[4])).Value == selectedValue5
                          select mainNode);
            }

            StringBuilder sb = new StringBuilder();

            foreach (var el in filter)
            {
                getInfo(el, sb);
            }
            _vm.TextToView = sb.ToString();
            if (_vm.TextToView == null)
            {
                Shell.Current.DisplayAlert("Filter", "No attributes match the selected criteria.", "OK");
            }
        }

        void getInfo(XElement element, StringBuilder sb)
        {
            sb.AppendLine($"{element.Name}:");

            if (element.HasAttributes)
            {
                foreach (var attr in element.Attributes())
                {
                    sb.AppendLine($"{attr.Name}: {attr.Value}");
                }
            }

            if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
            {
                sb.AppendLine($"{element.Value.Trim()}");
            }

            foreach (var el in element.Elements())
            {
                getInfo(el, sb);
            }
        }
    }

    class Parser
    {
        private IParser _strategy;

        public Parser(IParser strategy)
        {
            _strategy = strategy;
        }

        public void setStrategy(IParser strategy)
        {
            _strategy = strategy;
        }

        public Dictionary<string, ObservableCollection<string>> doParse(string xmlPath)
        {
            return _strategy.Parse(xmlPath);
        }
        public void doSearchResult(string selectedValue1, string selectedValue2, string selectedValue3, string selectedValue4, string selectedValue5)
        {
            _strategy.searchResult(selectedValue1, selectedValue2, selectedValue3, selectedValue4, selectedValue5);
        }
    }
}




