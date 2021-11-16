using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HangfireService.Models
{
		[XmlRoot(ElementName = "StatelessServiceType", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class StatelessServiceType
		{
			[XmlAttribute(AttributeName = "ServiceTypeName")]
			public string ServiceTypeName { get; set; }
		}

		[XmlRoot(ElementName = "ServiceTypes", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class ServiceTypes
		{
			[XmlElement(ElementName = "StatelessServiceType", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public StatelessServiceType StatelessServiceType { get; set; }
		}

		[XmlRoot(ElementName = "ExeHost", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class ExeHost
		{
			[XmlElement(ElementName = "Program", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public string Program { get; set; }
			[XmlElement(ElementName = "WorkingFolder", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public string WorkingFolder { get; set; }
		}

		[XmlRoot(ElementName = "EntryPoint", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class EntryPoint
		{
			[XmlElement(ElementName = "ExeHost", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public ExeHost ExeHost { get; set; }
		}

		[XmlRoot(ElementName = "CodePackage", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class CodePackage
		{
			[XmlElement(ElementName = "EntryPoint", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public EntryPoint EntryPoint { get; set; }
			[XmlAttribute(AttributeName = "Name")]
			public string Name { get; set; }
			[XmlAttribute(AttributeName = "Version")]
			public string Version { get; set; }
		}

		[XmlRoot(ElementName = "ConfigPackage", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class ConfigPackage
		{
			[XmlAttribute(AttributeName = "Name")]
			public string Name { get; set; }
			[XmlAttribute(AttributeName = "Version")]
			public string Version { get; set; }
		}

		[XmlRoot(ElementName = "Endpoint", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class Endpoint
		{
			[XmlAttribute(AttributeName = "Name")]
			public string Name { get; set; }
			[XmlAttribute(AttributeName = "Protocol")]
			public string Protocol { get; set; }
		}

		[XmlRoot(ElementName = "Endpoints", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class Endpoints
		{
			[XmlElement(ElementName = "Endpoint", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public List<Endpoint> Endpoint { get; set; }
		}

		[XmlRoot(ElementName = "Resources", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class Resources
		{
			[XmlElement(ElementName = "Endpoints", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public Endpoints Endpoints { get; set; }
		}

		[XmlRoot(ElementName = "ServiceManifest", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
		public class ServiceManifest
		{
			[XmlElement(ElementName = "ServiceTypes", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public ServiceTypes ServiceTypes { get; set; }
			[XmlElement(ElementName = "CodePackage", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public CodePackage CodePackage { get; set; }
			[XmlElement(ElementName = "ConfigPackage", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public ConfigPackage ConfigPackage { get; set; }
			[XmlElement(ElementName = "Resources", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
			public Resources Resources { get; set; }
			[XmlAttribute(AttributeName = "Name")]
			public string Name { get; set; }
			[XmlAttribute(AttributeName = "Version")]
			public string Version { get; set; }
			[XmlAttribute(AttributeName = "xmlns")]
			public string Xmlns { get; set; }
			[XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
			public string Xsd { get; set; }
			[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
			public string Xsi { get; set; }

            public string ToXmlString()
            {
                using (var ms = new MemoryStream())
                {
                    using (var r = new StreamReader(ms))
                    {
                        var ser = new XmlSerializer(GetType());
                        ser.Serialize(XmlWriter.Create(ms), this);
                        ms.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        return r.ReadToEnd();
                    }
                }
            }
		}

	}