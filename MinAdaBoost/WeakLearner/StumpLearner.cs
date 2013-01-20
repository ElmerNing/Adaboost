using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace MinAdaBoost
{
    /// <summary>
    /// WeakLearner with stump.
    /// </summary>
    public class StumpLearner : WeakLearner
    {
        /// <summary>
        /// Stump WeakLearner.
        /// </summary>
        private Stump _stump = new Stump();
        /// <summary>
        /// find a optimal stump in this dim
        /// </summary>
        /// <param name="sortedDim">sorted X[][dim]</param>
        /// <param name="sortedWight">sorted weight with</param>
        /// <param name="N">length of sortedDim and sortedWeight</param>
        /// <param name="dim">current dim</param>
        /// <returns>the stump of min Pm in this dim</returns>
        private Stump OptimalOneDim(SortedNode[] sortedDim, double[] sortedWeight, int N, int dim)
        {
            //integral of (yn*wn)  for speed improvement 
            double[] integralPos = new double[N];
            double[] integralNeg = new double[N];
            int integralCount = 0;
            double[] valueCollection = new double[N];
            int valueCollectionCount = 0;
            for (int n = 0; n < N; )
            {
                int offset = 0;
                while (n + offset < N && sortedDim[n].Value == sortedDim[n + offset].Value)
                {
                    if (offset == 0) 
                    {
                        integralCount++;
                    }
                    if (sortedDim[n + offset].Y > 0)
                        integralPos[integralCount - 1] += sortedWeight[n+offset];
                    else
                        integralNeg[integralCount - 1] += sortedWeight[n + offset];
                    offset++;
                }
                valueCollectionCount++;
                valueCollection[valueCollectionCount - 1] = sortedDim[n].Value;
                n += offset;
            }
            for (int v = 1; v < integralCount; v++)
            {
                integralPos[v] += integralPos[v - 1];
                integralNeg[v] += integralNeg[v - 1];
            }

            // calculate error rates of (Count-1) split
            double[] errorPos = new double[integralCount - 1];
            double[] errorNeg = new double[integralCount - 1];
            for (int v = 0; v < integralCount - 1; v++)
            {
                errorPos[v] = integralPos[v] + integralNeg[integralCount - 1] - integralNeg[v];
                errorNeg[v] = integralNeg[v] + integralPos[integralCount - 1] - integralPos[v];
            }

            //find the min error rate as Pm and return the best stump of this dim
            int minPosIndex = 0, minNegIndex = 0;
            for (int v = 1; v < errorPos.Length; v++)
            {
                if (errorPos[0] >= errorPos[v])
                {
                    minPosIndex = v;
                    errorPos[0] = errorPos[v]; 
                }
                if (errorNeg[0] >= errorNeg[v])
                {
                    minNegIndex = v;
                    errorNeg[0] = errorNeg[v];
                }
            }

            double sign = 1, threshold = 0, Pm = 0.49;
            if (errorNeg.Length < 1 || errorPos.Length < 1)
            {
                sign = -1;
                threshold = valueCollection[0];
                Pm = 1;
            }
            else if (errorNeg[0] > errorPos[0])
            {
                sign = 1;
                threshold = 0.5*(valueCollection[minPosIndex] + valueCollection[minPosIndex+1]);
                Pm = errorPos[0];
            }
            else
            {
                threshold = 0.5 * (valueCollection[minNegIndex] + valueCollection[minNegIndex + 1]);
                sign = -1;
                Pm = errorNeg[0];
            }
            return new Stump(dim, threshold, sign, Pm);
        }

        #region interface WeakLearner

        private double _alpha;
        public double Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        public void InitLearningOptions(string[] args)
        {
//             OrigData data = new OrigData(prob)
//             return null;
        }

        public TrainData CreateTrainData(Problem prob)
        {
            SortedData data = new SortedData();
            data.GenTrainData(prob);
            if (data.IsSparse == true)
            {
                string mesg = /*this.GetType().ToString()+*/"StumpLearner do not support sparse data!";
                throw new Exception(mesg);
            }
            return data;
        }

        public double Train(TrainData data, double[] weight)
        {
            SortedData sortdata = data as SortedData;
            int N = sortdata.N, MaxDim = sortdata.MaxDim;
            List<Stump> stumpArray = new List<Stump>(MaxDim);
            double[] sortedWeight = new double[sortdata.N];
            for (int dim = 1; dim <= MaxDim; dim++ )
            {
                SortedNode[] sortedDim = sortdata[dim];
                //sort the weight as sortedDim
                for (int n = 0; n < N; n++)
                    sortedWeight[n] = weight[sortedDim[n].N];
                //find the best Stump in a dim.
                Stump stump = OptimalOneDim(sortedDim, sortedWeight, N, dim);
                stumpArray.Add(stump);
            }
            stumpArray.Sort();
            _stump = stumpArray[0];
            return stumpArray[0].Pm;
        }

        public double Classify(Node[] vx)
        {
            if (_stump == null)
                throw new Exception(Messege.WeakLearnerNull);
            return _stump.Classify(vx);
        }

        public void SerializeToXml(ref XmlElement weakLearnerNode)
        {
            weakLearnerNode.SetAttribute("Alpha", _alpha.ToString());

            XmlElement dimNode = weakLearnerNode.OwnerDocument.CreateElement("Dim");
            XmlElement thrNode = weakLearnerNode.OwnerDocument.CreateElement("Thr");
            XmlElement signNode = weakLearnerNode.OwnerDocument.CreateElement("Sign");
            dimNode.InnerText = _stump.Dim.ToString();
            thrNode.InnerText = _stump.Thr.ToString();
            signNode.InnerText = _stump.Sign.ToString();
            weakLearnerNode.AppendChild(dimNode);
            weakLearnerNode.AppendChild(thrNode);
            weakLearnerNode.AppendChild(signNode);
        }

        public void DeserializeFromXml(XmlElement weakLearnerNode)
        {
            _alpha = double.Parse(weakLearnerNode.Attributes["Alpha"].Value);
            
            foreach (XmlNode node in weakLearnerNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Dim":
                        _stump.Dim = int.Parse(node.InnerText);
                        break;
                    case "Thr":
                        _stump.Thr = double.Parse(node.InnerText);
                        break;
                    case "Sign":
                        _stump.Sign = double.Parse(node.InnerText);
                        break;
                }
            }
        }
        #endregion

        #region Definition of Class Stump

        /// <summary>
        /// stump data structure
        /// </summary>
        class Stump : IComparable<Stump>
        {
            /// <summary>
            /// dim of stump.
            /// </summary>
            public int Dim = 1;
            /// <summary>
            /// threshold of stump.
            /// </summary>
            public double Thr = 0;
            /// <summary>
            /// When X[][Dim] > threshold, we hypothesize y = Sign. 1 or -1.
            /// </summary>
            public double Sign = 1;
            /// <summary>
            /// the sum of sortedWeight[n] where n is the misclassified index.
            /// use for train.
            /// </summary>
            public double Pm = 0.49;
            /// <summary>
            /// Constructor.
            /// </summary>
            public Stump()
            {
                Dim = 1;
                Thr = 0;
                Sign = 1;
                Pm = 0.49;
            }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="dim"></param>
            /// <param name="threshold"></param>
            /// <param name="sign"></param>
            /// <param name="pm"></param>
            /// <returns></returns>
            public Stump(int dim, double threshold, double sign, double pm)
            {
                Dim = dim;
                Thr = threshold;
                Sign = sign;
                Pm = pm;
            }
            /// <summary>
            /// Classify.
            /// </summary>
            /// <param name="vx">one sample</param>
            /// <returns>classify result</returns>
            public double Classify(Node[] vx)
            {
                double x = vx[Dim-1].Value;
                if (x > Thr)
                {
                    return Sign;
                }
                else
                    return -Sign;
                /*foreach (Node x in vx)
                {
                    if (x.Dim == Dim)
                    {
                        if (x.Value > Thr)
                        {
                            return Sign;
                        }
                        else
                            return -Sign;
                    }
                }*/
                return 0;
            }

            #region interface IComparable<Stump>
            public int CompareTo(Stump other)
            {
                if (other == null)
                {
                    return -1;
                }
                return Pm.CompareTo(other.Pm);
            }
            #endregion
        }

        #endregion
    }
}
