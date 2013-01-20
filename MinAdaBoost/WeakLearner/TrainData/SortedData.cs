using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinAdaBoost
{
    /// <summary>
    /// every dim is sorted by value
    /// </summary>
    class SortedData : TrainData
    {
        /// <summary>
        /// is sample sparse.
        /// </summary>
        public bool IsSparse
        {
            get {
                for (int dim = 1; dim <= _maxDim; dim++ )
                {
                    if (_sortedDimNum[dim]!=_n)
                    {
                        return true;
                    }
                }
                return false; 
            }
        }

        private SortedNode[][] _sortedData = null;
        private int[] _sortedDimNum = null;

        public SortedNode[] this[int dim]
        {
            get
            {
                return _sortedData[dim];
            }
        }

        #region interface TrainData

        public int _maxDim;
        public int MaxDim
        {
            get { return _maxDim; }
            set { _maxDim = value; }
        }

        private int _n;
        public int N
        {
            get { return _n; }
        }

        public void GenTrainData(Problem prob)
        {
            _maxDim = prob.MaxDim;
            _n = prob.N;

            _sortedData = new SortedNode[_maxDim + 1][];
            _sortedDimNum = new int[_maxDim + 1];
            for (int dim = 0; dim <= _maxDim; dim++ )
            {
                _sortedData[dim] = new SortedNode[_n];
            }

            for (int n = 0; n < _n; n++)
            {
                foreach (Node x in prob.X[n])
                {
                    int index = _sortedDimNum[x._dim];
                    _sortedDimNum[x._dim]++;
                    //_sortedData[x._dim][index].Dim = x._dim;
                    _sortedData[x._dim][index].N = n;
                    _sortedData[x._dim][index].Value = x._value;
                    _sortedData[x._dim][index].Y = prob.Y[n];
                }
            }
            for (int dim = 0; dim <= _maxDim; dim++ )
            {
                Array.Sort(_sortedData[dim], 0, _sortedDimNum[dim]);
            }
        }

        #endregion
    }

    internal struct SortedNode : IComparable<SortedNode>
    {
        /// <summary>
        /// dim of this Node.
        /// </summary>
        //internal int Dim;
        /// <summary>
        /// Value at Index.
        /// </summary>
        internal double Value;
        /// <summary>
        /// the Nth sample contain this Node
        /// </summary>
        internal int N;
        /// <summary>
        /// the Nth sample's label 
        /// </summary>
        internal double Y;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value at Index.</param>
        /// <param name="n">the Nth sample contain this Node</param>
        /// <param name="y"> label of the Nth sample </param>
        /// <returns></returns>
        internal SortedNode(double value, int n, double y)
        {
            //Dim = dim;
            Value = value;
            N = n;
            Y = y;
        }

        #region interface IComparable<SortedNote>

        /// <summary>
        /// Compares this node with another.
        /// </summary>
        /// <param name="other">The node to compare to</param>
        /// <returns>A positive number if this node is greater, a negative number if it is less than, or 0 if equal</returns>
        public int CompareTo(SortedNode other)
        {
            return Value.CompareTo(other.Value);
        }

        #endregion
    }

    internal struct SortedDim
    {
        SortedNode[] _sortedNodes;
        SortedDim(int n)
        {
            _sortedNodes = new SortedNode[n];
        }
    }
}
