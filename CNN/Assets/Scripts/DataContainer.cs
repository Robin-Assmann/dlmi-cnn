using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

[XmlRoot ("Data")]
public class DataContainer {

	[XmlArrayItem("Parameters")]
	public Data Parameters;

	//Serialisierung der Daten aus dem XML Dokument
	public static DataContainer Load(string path){
		var tp_ser = new XmlSerializer (typeof(DataContainer));
		using (var stream = new FileStream (path, FileMode.Open)) {
			return tp_ser.Deserialize (stream) as DataContainer;
		}
	}
}
