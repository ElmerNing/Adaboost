using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinAdaBoost
{
    /// <summary>
    /// OrigData's data structure is same as Problem.
    /// </summary>
    public class OrigData :  TrainData
    {
        #region interface TrainData

        public double[] Y;
        public Node[][] X;
        public int _maxDim;
        public int MaxDim
        {
            get { return _maxDim; }
        }
        private int _n;
        public int N
        {
            get { return _n; }
        }
        public void GenTrainData(Problem prob)
        {
            X = prob.X;
            Y = prob.Y;
            _maxDim = prob.MaxDim;
            _n = prob.N;
        }

        #endregion
    }
}
