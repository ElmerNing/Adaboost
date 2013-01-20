using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace MinAdaBoost
{
    /// <summary>
    /// Encapsulates an SVM Model
    /// </summary>
    public class Model
    {
        private int _numberOfClasses;
        private BinaryClassifier[] _binaryClassifiers;

        internal Model()
        {
        }

        /// <summary>
        /// Number of classes in the model.
        /// </summary>
        public int NumberOfClasses
        {
            get { return _numberOfClasses; }
            set { _numberOfClasses = value; }
        }

        /// <summary>
        /// A set of binary classifier
        /// </summary>
        public BinaryClassifier[] BinaryClassifiers
        {
            get { return _binaryClassifiers; }
            set { _binaryClassifiers = value; }
        }

        /// <summary>
        /// Number of support vectors per class.
        /// </summary>
        public static void Write(Stream stream, Model model)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //XmlNode declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            //xmlDoc.AppendChild(declaration);
            XmlElement modelRoot = xmlDoc.CreateElement("model");
            modelRoot.SetAttribute("NumberOfClasses", model.NumberOfClasses.ToString());
            xmlDoc.AppendChild(modelRoot);

            for(int i = 0; i<model.BinaryClassifiers.Length; i++)
            {
                XmlElement binaryClassifierNode = xmlDoc.CreateElement("BinaryClassifier");
                model.BinaryClassifiers[i].SerializeToXml(ref binaryClassifierNode);
                modelRoot.AppendChild(binaryClassifierNode);
            }
            xmlDoc.Save(stream);
        }

        /// <summary>
        /// Reads a Model from the provided file.
        /// </summary>
        /// <param name="filename">The name of the file containing the Model</param>
        /// <returns>the Model</returns>
        public static void Write(string filename, Model model)
        {
            FileStream stream = File.Open(filename, FileMode.Create);
            try
            {
                Write(stream, model);
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Reads a Model from the provided stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the Model.</param>
        /// <returns>the Model</returns>
        public static Model Read(Stream stream)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(stream);
            Model model = new Model();
            XmlNode modelNode = xmldoc.FirstChild;
            XmlNodeList classifyNodes = modelNode.ChildNodes;
            model.NumberOfClasses = int.Parse(modelNode.Attributes["NumberOfClasses"].Value);
            if (    (model.NumberOfClasses == 2 && classifyNodes.Count != 1) &&
                    (classifyNodes.Count != model.NumberOfClasses) )
                throw new Exception(Messege.ReadModelFail);

            model.BinaryClassifiers = new BinaryClassifier[classifyNodes.Count];
            for (int i = 0; i < classifyNodes.Count; i++ )
            {
                XmlElement binaryClassifierNode = (XmlElement)classifyNodes.Item(i);
                BinaryClassifier binaryClassifer = BinaryClassifier.DeserializeFromXML(binaryClassifierNode); 
                model.BinaryClassifiers[i] = binaryClassifer;
            }
            return model;
        }

        /// <summary>
        /// Reads a Model from the provided file.
        /// </summary>
        /// <param name="filename">The name of the file containing the Model</param>
        /// <returns>the Model</returns>
        public static Model Read(string filename)
        {
            FileStream input = File.OpenRead(filename);
            try
            {
                return Read(input);
            }
            finally
            {
                input.Close();
            }
        }

    }
}
