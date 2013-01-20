using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace MinAdaBoost
{
    /// <summary>
    /// Encapsulates an binary classifier.
    /// </summary>
    public class BinaryClassifier
    {
        private double _posLabel = double.NaN;
        private double _negLabel = double.NaN;
        private StrongLeaner _strongLearner = null;

        private BinaryClassifier(double posLabel, double negLabel, StrongLeaner strongLearner)
        {
            _posLabel = posLabel;
            _negLabel = negLabel;
            _strongLearner = strongLearner;
        }

        private static Problem CreatBinaryProblem(Problem prob, double posLabel, double negLabel)
        {
            List<Node[]> X_pos = new List<Node[]>();
            List<Node[]> X_neg = new List<Node[]>();
            for (int n = 0; n < prob.N; n++)
            {
                if (prob.Y[n] == posLabel)
                    X_pos.Add(prob.X[n]);
                else if (prob.Y[n] == negLabel || double.IsNaN(negLabel))
                    X_neg.Add(prob.X[n]);
            }

            double[] Y = new double[X_pos.Count + X_neg.Count];
            for (int i = 0; i < X_pos.Count; i++)
                Y[i] = 1;
            for (int i = X_pos.Count; i < Y.Length; i++)
                Y[i] = -1;

            X_pos.AddRange(X_neg);
            Node[][] X = X_pos.ToArray();

            return new Problem(Y.Length, Y, X, prob.MaxDim);
        }

        /// <summary>
        /// Positive label.
        /// </summary>
        public double PosLabel
        {
            get { return _posLabel; }
            private set { _posLabel = value;  }
        }

        /// <summary>
        /// Negative label.
        /// </summary>
        public double NegLabel
        {
            get { return _negLabel; }
            private set { _negLabel = value;  }
        }

        /// <summary>
        /// Train posLabel vs negLabel. 
        /// when negLabel is NaN, Train posLabel vs other.
        /// </summary>
        /// <param name="prob">The training data</param>
        /// <param name="arg">The training argument</param>
        /// <param name="posLabel">positive label</param>
        /// <param name="negLabel">negative label</param>
        /// <returns>BinaryClassifier</returns>
        public static BinaryClassifier Train(Problem prob, TrainingArg arg, double posLabel, double negLabel = double.NaN)
        {
            Problem binaryProb = CreatBinaryProblem(prob, posLabel, negLabel);
            Assembly asm = Assembly.GetAssembly(typeof(StrongLeaner));
            StrongLeaner strongLearner = (StrongLeaner)asm.CreateInstance(typeof(StrongLeaner).Namespace + "." + arg.StrongLearnerName);
            strongLearner.Train(binaryProb, arg.WeakLearnerName, arg.WeakLearnerArgs, arg.Iterations);

            return new BinaryClassifier(posLabel, negLabel, strongLearner);
        }

        /// <summary>
        /// Classify.
        /// </summary>
        /// <param name="vx">The vector which to classify</param>
        /// <returns>PosLabel or NegLabel </returns>
        public double Classify(Node[] vx)
        {
            if (_strongLearner == null)
                throw new Exception(Messege.StrongLearnerNull);

            double y = _strongLearner.Classify(vx);
            if (y > 0)
            {
                return _posLabel;
            }
            else
            {
                return _negLabel;
            }
        }

        /// <summary>
        /// Serialize to an xml node.
        /// </summary>
        /// <param name="binaryClassifierNode">an xml node</param>
        /// <returns></returns>
        public void SerializeToXml(ref XmlElement binaryClassifierNode)
        {
            XmlElement posLabelNote = binaryClassifierNode.OwnerDocument.CreateElement("PosLabel");
            XmlElement negLabelNote = binaryClassifierNode.OwnerDocument.CreateElement("NegLabel");
            XmlElement learnerNote = binaryClassifierNode.OwnerDocument.CreateElement("StrongLearner");

            posLabelNote.InnerText = _posLabel.ToString();
            negLabelNote.InnerText = _negLabel.ToString();
            learnerNote.SetAttribute("type", _strongLearner.GetType().Name);
            _strongLearner.SerializeToXml(ref learnerNote);
            
            if (!double.IsNaN(_posLabel))
                binaryClassifierNode.AppendChild(posLabelNote);
            if (!double.IsNaN(_negLabel))
                binaryClassifierNode.AppendChild(negLabelNote);
            binaryClassifierNode.AppendChild(learnerNote);
        }

        /// <summary>
        /// Deserialize from an xml node.
        /// </summary>
        /// <param name="binaryClassifierNode">an xml node</param>
        /// <returns>BinaryClassifier</returns>
        public static BinaryClassifier DeserializeFromXML(XmlElement binaryClassifierNode)
        {
            double posLabel = double.NaN;
            double negLabel = double.NaN;
            StrongLeaner strongLearner = null;
           foreach (XmlElement node in binaryClassifierNode.ChildNodes)
           {
               switch (node.Name)
               {
                   case "PosLabel":
                       posLabel = double.Parse(node.InnerText);
                       break;
                   case "NegLabel":
                       negLabel = double.Parse(node.InnerText);
                       break;
                   case "StrongLearner":
                       Assembly asm = Assembly.GetAssembly(typeof(StrongLeaner));
                       strongLearner = (StrongLeaner)asm.CreateInstance(typeof(StrongLeaner).Namespace + "." + node.Attributes["type"].Value);
                       strongLearner.DerializeFromXML(node);
                       break;
               }
           }
           return new BinaryClassifier(posLabel, negLabel, strongLearner);
        }
    }
}
