using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

namespace MinAdaBoost
{
    /// <summary>
    /// Decesion tree weakLearner.
    /// "MULTIBOOST documentation", Chapter B.4. in http://www.multiboost.org.
    /// </summary>
    class TreeLearner : WeakLearner
    {
        /// <summary>
        /// root node of tree.
        /// </summary>
        private TreeNode _treeRoot;
        /// <summary>
        /// max split number.
        /// </summary>
        private int _maxSplit = 3;
        /// <summary>
        /// find a optimal stump in sub dataset
        /// </summary>
        /// <param name="sortdata">sorted X[][dim]</param>
        /// <param name="flag">flag of the sub dataset</param>
        /// <param name="datasetFlags">flags of the whole dataset</param>
        /// <param name="weight">current weight</param>
        /// <returns>the stump of min Pm</returns>
        Stump OptimalOneNode(SortedData sortdata, int flag, int[] datasetFlags, double[] weight)
        {
            int N = sortdata.N, MaxDim = sortdata.MaxDim;
            List<Stump> stumpArray = new List<Stump>(MaxDim);
            double[] sortedWeight = new double[sortdata.N];
            object _lock = new object();
            for (int dim = 1; dim <= MaxDim; dim++)
            //Parallel.For(1, MaxDim + 1, dim =>
            {
                SortedNode[] sortedDim = sortdata[dim];
                for (int n = 0; n < N; n++)
                    sortedWeight[n] = weight[sortedDim[n].N];
                Stump stump = OptimalOneDim(sortedDim, flag, datasetFlags, sortedWeight, N, dim);
                stumpArray.Add(stump);
            }//);
            stumpArray.Sort();
            Stump beststump = null;
            do
            {
                beststump = stumpArray[0];
                stumpArray.RemoveAt(0);
            } while (beststump == null && stumpArray.Count != 0);
            return beststump;
        }
        /// <summary>
        /// find a optimal stump in this dim (sub dataset)
        /// </summary>
        /// <param name="sortedDim">sorted X[][dim]</param>
        /// <param name="flag">flag of the sub samples</param>
        /// <param name="datasetFlags">flags of the whole dataset</param>
        /// <param name="sortedWight">sorted weight with</param>
        /// <param name="N">length of sortedDim and sortedWeight</param>
        /// <param name="dim">current dim</param>
        /// <returns>the stump of min Pm in this dim</returns>
        private Stump OptimalOneDim(SortedNode[] sortedDim, int flag, int[] datasetFlags, double[] sortedWeight, int N, int dim)
        {
            //integral of (yn*wn)  for speed improvement 
            double[] integralPos = new double[N];
            double[] integralNeg = new double[N];
            int integralCount = 0;
            double[] valueCollection = new double[N];
            int valueCollectionCount = 0;
            for (int n = 0; n < N; )
            {
                if (datasetFlags[sortedDim[n].N] != flag)
                {
                    n++;
                    continue;
                }

                int offset = 0;
                while (n + offset < N && sortedDim[n].Value == sortedDim[n + offset].Value)
                {
                    if (offset == 0)
                    {
                        integralCount++;
                    }
                    int nx = n + offset;
                    if (datasetFlags[sortedDim[nx].N] == flag)
                    {
                        if (sortedDim[n + offset].Y > 0)
                            integralPos[integralCount - 1] += sortedWeight[nx];
                        else
                            integralNeg[integralCount - 1] += sortedWeight[nx];
                    }
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
            double errorPosMin = 1, errorNegMin = 1;
            for (int v = 1; v < errorPos.Length; v++)
            {
                if (errorPosMin >= errorPos[v])
                {
                    minPosIndex = v;
                    errorPosMin = errorPos[v];
                }
                if (errorNegMin >= errorNeg[v])
                {
                    minNegIndex = v;
                    errorNegMin = errorNeg[v];
                }
            }

            double sign = 1, threshold = 0, Pm = 0.49, Pl = 0.49, Pr = 0.49;
            if (errorNeg.Length < 1 || errorPos.Length < 1)
            {
                sign = -1;
                threshold = valueCollection[0];
                Pm = 1;
                return null;
            }
            else if (errorNegMin > errorPosMin)
            {
                sign = 1;
                threshold = 0.5 * (valueCollection[minPosIndex] + valueCollection[minPosIndex + 1]);
                Pm = errorPosMin;
                Pl = integralPos[minPosIndex];
                Pr = integralNeg[integralCount - 1] - integralNeg[minPosIndex];
            }
            else
            {
                threshold = 0.5 * (valueCollection[minNegIndex] + valueCollection[minNegIndex + 1]);
                sign = -1;
                Pm = errorNegMin;
                Pl = integralNeg[minNegIndex];
                Pr = integralPos[integralCount - 1] - integralPos[minNegIndex];
            }
            return new Stump(dim, threshold, sign, Pm, Pl, Pr);
        }
//         private Stump OptimalOneDim2(SortedNode[] sortedDim,  int flag, int[] datasetFlags, double[] sortedWeight, int N, int dim)
//         {
//             //integral of (yn*wn)  for speed improvement 
//             List<double> integralPos = new List<double>(N);
//             List<double> integralNeg = new List<double>(N);
//             List<double> valueCollection = new List<double>(N);
//             for (int n = 0; n < N; )
//             {
//                 if (datasetFlags[sortedDim[n].N] != flag)
//                 {
//                     n++;
//                     continue;
//                 }
// 
//                 int offset = 0;
//                 while (n + offset < N && sortedDim[n].Value == sortedDim[n + offset].Value)
//                 {
//                     if (offset == 0)
//                     {
//                         integralPos.Add(0);
//                         integralNeg.Add(0);
//                     }
//                     int nx = n + offset;
//                     if (datasetFlags[sortedDim[nx].N] == flag)
//                     {
//                         if (sortedDim[n + offset].Y > 0)
//                             integralPos[integralPos.Count - 1] += sortedWeight[nx];
//                         else
//                             integralNeg[integralNeg.Count - 1] += sortedWeight[nx];
//                     }
//                     offset++;
//                 }
//                 valueCollection.Add(sortedDim[n].Value);
//                 n += offset;
//             }
//             for (int v = 1; v < integralNeg.Count; v++)
//             {
//                 integralPos[v] += integralPos[v - 1];
//                 integralNeg[v] += integralNeg[v - 1];
//             }
// 
//             // calculate error rates of (Count-1) split
//             double[] errorPos = new double[integralPos.Count - 1];
//             double[] errorNeg = new double[integralNeg.Count - 1];
//             for (int v = 0; v < integralNeg.Count - 1; v++)
//             {
//                 errorPos[v] = integralPos[v] + integralNeg[integralNeg.Count - 1] - integralNeg[v];
//                 errorNeg[v] = integralNeg[v] + integralPos[integralPos.Count - 1] - integralPos[v];
//             }
// 
//             //find the min error rate as Pm and return the best stump of this dim
//             int minPosIndex = 0, minNegIndex = 0;
//             double errorPosMin = 1, errorNegMin = 1;
//             for (int v = 1; v < errorPos.Length; v++)
//             {
//                 if (errorPosMin >= errorPos[v])
//                 {
//                     minPosIndex = v;
//                     errorPosMin = errorPos[v];
//                 }
//                 if (errorNegMin >= errorNeg[v])
//                 {
//                     minNegIndex = v;
//                     errorNegMin = errorNeg[v];
//                 }
//             }
// 
//             double sign = 1, threshold = 0, Pm = 0.49, Pl = 0.49, Pr = 0.49;
//             if (errorNeg.Length < 1 || errorPos.Length < 1)
//             {
//                 sign = -1;
//                 threshold = valueCollection[0];
//                 Pm = 1;
//                 return null;
//             }
//             else if (errorNegMin > errorPosMin)
//             {
//                 sign = 1;
//                 threshold = 0.5 * (valueCollection[minPosIndex] + valueCollection[minPosIndex + 1]);
//                 Pm = errorPosMin;
//                 Pl = integralPos[minPosIndex];
//                 Pr = integralNeg[integralNeg.Count - 1] - integralNeg[minPosIndex];
//             }
//             else
//             {
//                 threshold = 0.5 * (valueCollection[minNegIndex] + valueCollection[minNegIndex + 1]);
//                 sign = -1;
//                 Pm = errorNegMin;
//                 Pl = integralNeg[minNegIndex];
//                 Pr = integralPos[integralPos.Count - 1] - integralPos[minNegIndex];
//             }
//             return new Stump(dim, threshold, sign, Pm, Pl, Pr);
//         }
        /// <summary>
        /// Cut the sub dataset of treeNode into 2 branches.
        /// each branch sign with leftFlag and rightFlag.
        /// </summary>
        /// <param name="treeNode">the treeNode to cut</param>
        /// <param name="sortdata">sorted X[][dim]</param>
        /// <param name="leftFlag">flag of the left branch</param>
        /// <param name="rightFlag">flag of the right branch</param>
        /// <param name="datasetFlag">flags of the whole dataset</param>
        /// <returns></returns>
        private void CutDataSet(TreeNode treeNode, SortedData sortdata, int leftFlag, int rightFlag, ref int[] datasetFlag)
        {
            int N = sortdata.N;
            SortedNode[] sortedDim = sortdata[treeNode.InnerStump.Dim];
            double Thr = treeNode.InnerStump.Thr;
            for (int n = 0; n < N;  n++)
            {
                //SortedNode node = sortedDim[n];
                if (datasetFlag[sortedDim[n].N] == treeNode.Flag)
                {
                    if (sortedDim[n].Value > Thr)
                        datasetFlag[sortedDim[n].N] = rightFlag;
                    else
                        datasetFlag[sortedDim[n].N] = leftFlag;
                }
            }
        }
        /// <summary>
        /// Serialize a tree node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="treeXML"></param>
        private void SerializeTree(TreeNode node, ref XmlElement treeXML)
        {
//             XmlElement dimNode = treeXML.OwnerDocument.CreateElement("Dim");
//             XmlElement thrNode = treeXML.OwnerDocument.CreateElement("Thr");
//             XmlElement signNode = treeXML.OwnerDocument.CreateElement("Sign");
//             dimNode.InnerText = _treeRoot.InnerStump.Dim.ToString();
//             thrNode.InnerText = _treeRoot.InnerStump.Thr.ToString();
//             signNode.InnerText = _treeRoot.InnerStump.Sign.ToString();
//             treeXML.AppendChild(dimNode);
//             treeXML.AppendChild(thrNode);
//             treeXML.AppendChild(signNode);
            treeXML.SetAttribute("Dim", node.InnerStump.Dim.ToString());
            treeXML.SetAttribute("Thr", node.InnerStump.Thr.ToString());
            treeXML.SetAttribute("Sign", node.InnerStump.Sign.ToString());
            if (node.Left != null)
            {
                XmlElement leftTreeXML = treeXML.OwnerDocument.CreateElement("Left");
                SerializeTree(node.Left, ref leftTreeXML);
                treeXML.AppendChild(leftTreeXML);
            }

            if (node.Right != null)
            {
                XmlElement rightTreeXML = treeXML.OwnerDocument.CreateElement("Right");
                SerializeTree(node.Right, ref rightTreeXML);
                treeXML.AppendChild(rightTreeXML);
            }
        }
        /// <summary>
        /// Deserialize a treenode
        /// </summary>
        /// <param name="treeXML"></param>
        /// <returns></returns>
        private TreeNode DeSerializeTree(XmlElement treeXML)
        {
            Stump innerStump = new Stump();
            innerStump.Dim = int.Parse(treeXML.Attributes["Dim"].Value);
            innerStump.Thr = double.Parse(treeXML.Attributes["Thr"].Value);
            innerStump.Sign = double.Parse(treeXML.Attributes["Sign"].Value);

            TreeNode treeNode = new TreeNode();
            treeNode.InnerStump = innerStump;
            foreach (XmlNode node in treeXML.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Left":
                        treeNode.Left = DeSerializeTree((XmlElement)node);
                        treeNode.Left.Parent = treeNode;
                        break;
                    case "Right":
                        treeNode.Right = DeSerializeTree((XmlElement)node);
                        treeNode.Right.Parent = treeNode;
                        break;
                }
            }
            return treeNode;
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
            if (args == null || args.Length <= 0)
                return;
            else
                _maxSplit = int.Parse(args[0]); 
        }

        public TrainData CreateTrainData(Problem prob)
        {
            SortedData data = new SortedData();
            data.GenTrainData(prob);
            if (data.IsSparse == true)
            {
                string mesg = "TreeLearner do not support sparse data!";
                throw new Exception(mesg);
            }
            return data;
        }

        public double Train(TrainData data, double[] weight)
        {
            SortedData sortdata = data as SortedData;
            int N = sortdata.N, MaxDim = sortdata.MaxDim;

            int[] datasetFlags = new int[sortdata.N];
            int flag = 0;

            Stump stump = OptimalOneNode(sortdata, flag, datasetFlags, weight);
            if (stump == null)
                throw new Exception(Messege.CouldNotClassify);

            TreeNode treeNode = new TreeNode();
            treeNode.InnerStump = stump;
            treeNode.Parent = null;
            treeNode.Flag = flag;
            treeNode.Delta = 0.5-treeNode.InnerStump.Pm;

            List<TreeNode> priorityQueue = new List<TreeNode>();
            priorityQueue.Add(treeNode);
            double Pm = stump.Pm;
            for (int splitIndex = 0; splitIndex < _maxSplit; splitIndex++ )
            {
                do 
                {
                    treeNode = priorityQueue[0];
                    priorityQueue.RemoveAt(0);
                } while (treeNode == null && priorityQueue.Count != 0);

                if (treeNode == null)
                    break;

                if (treeNode.Parent == null)
                    _treeRoot = treeNode;
                else{
                    if (treeNode.Flag % 2 != 0)
                        treeNode.Parent.Left = treeNode;
                    else
                        treeNode.Parent.Right = treeNode;
                    Pm = Pm - treeNode.Delta;
                }

                int leftFlag = ++flag;
                int rightFlag = ++flag;
                CutDataSet(treeNode, sortdata, leftFlag, rightFlag, ref datasetFlags);

                TreeNode leftNode = null,rightNode = null;
                if (treeNode.InnerStump.Pl > double.Epsilon)
                {
                    stump = OptimalOneNode(sortdata, leftFlag, datasetFlags, weight);
                    if (stump != null)
                    {
                        leftNode = new TreeNode();
                        leftNode.InnerStump = stump;
                        leftNode.Parent = treeNode;
                        leftNode.Flag = leftFlag;
                        leftNode.Delta = treeNode.InnerStump.Pl - stump.Pm;
                    }
                }
                if (treeNode.InnerStump.Pr > double.Epsilon)
                {
                    stump = OptimalOneNode(sortdata, rightFlag, datasetFlags, weight);
                    if (stump != null)
                    {
                        rightNode = new TreeNode();
                        rightNode.InnerStump = stump;
                        rightNode.Parent = treeNode;
                        rightNode.Flag = rightFlag;
                        rightNode.Delta = treeNode.InnerStump.Pr - stump.Pm;
                    }
                }

                priorityQueue.Add(leftNode);
                priorityQueue.Add(rightNode);
                priorityQueue.Sort();
            }
            return Pm;
        }

        public double Classify(Node[] vx)
        {
            return _treeRoot.Classify(vx);
        }

        public void SerializeToXml(ref XmlElement weakLearnerNode)
        {
            weakLearnerNode.SetAttribute("Alpha", _alpha.ToString());
            XmlElement treeXML = weakLearnerNode.OwnerDocument.CreateElement("TreeRoot");
            SerializeTree(_treeRoot, ref treeXML);
            weakLearnerNode.AppendChild(treeXML);
        }

        public void DeserializeFromXml(XmlElement weakLearnerNode)
        {
            _alpha = double.Parse(weakLearnerNode.Attributes["Alpha"].Value);
            foreach (XmlNode node in weakLearnerNode.ChildNodes)
            {
                if (node.Name == "TreeRoot")
                {
                    _treeRoot = DeSerializeTree((XmlElement)node);
                    break;
                }
            }
        }
        #endregion

        #region Definition of Class Tree
        class TreeNode : IComparable<TreeNode>
        {
            public Stump InnerStump = null; 
            public TreeNode Parent = null;
            public TreeNode Left = null;
            public TreeNode Right = null;

            //////////////////////////////////////////////////////////////////////////
            public int Flag = 0;
            public double Delta = 0;

            /// <summary>
            /// Classify.
            /// </summary>
            /// <param name="vx">one sample</param>
            /// <returns>classify result</returns>
            public double Classify(Node[] vx)
            {
                //foreach (Node x in vx)
                {
                    Node x = vx[InnerStump.Dim - 1];
                    if (x.Dim == InnerStump.Dim)
                    {
                        if (x.Value > InnerStump.Thr)
                        {
                            if (Right == null)
                                return InnerStump.Classify(vx);
                            else
                                return Right.Classify(vx);
                        }
                        else
                        {
                            if (Left == null)
                                return InnerStump.Classify(vx);
                            else
                                return Left.Classify(vx);
                        }
                    }
                }
                return 0;
            }
            #region interface IComparable<Stump>
            public int CompareTo(TreeNode other)
            {
                if (other == null)
                {
                    return -1;
                }
                return -Delta.CompareTo(other.Delta);
            }
            #endregion
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
            public double Pm = 1;
            /// <summary>
            /// the sum of sortedWeight[n] where n is the misclassified index and X[n][Dim] > Thr.
            /// </summary>
            public double Pr = 1;
            /// <summary>
            /// the sum of sortedWeight[n] where n is the misclassified index and  Thr >= X[n][Dim].
            /// </summary>
            public double Pl = 1;
            /// <summary>
            /// Constructor.
            /// </summary>
            public Stump()
            {
            }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="dim"></param>
            /// <param name="threshold"></param>
            /// <param name="sign"></param>
            /// <param name="pm"></param>
            /// <returns></returns>
            public Stump(int dim, double threshold, double sign, double pm, double pl, double pr)
            {
                Dim = dim;
                Thr = threshold;
                Sign = sign;
                Pm = pm;
                Pl = pl;
                Pr = pr;
            }

            /// <summary>
            /// Classify.
            /// </summary>
            /// <param name="vx">one sample</param>
            /// <returns>classify result</returns>
            public double Classify(Node[] vx)
            {
                //double x = vx[Dim-1].Value;
                //foreach (Node x in vx)
                Node x = vx[Dim-1];
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
                }
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
