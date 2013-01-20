using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace MinAdaBoost
{
    /// <summary>
    /// Base weak learner interface.
    /// all the other weak learner should implement the interface.
    /// </summary>
    public interface WeakLearner
    {
        /// <summary>
        /// Alpha is weight of WeakLearner.
        /// </summary>
        double Alpha { get; set; }
        /// <summary>
        /// Init options of learning. 
        /// </summary>
        /// <param name="args">options</param>
        /// <returns></returns>
        void InitLearningOptions(string[] args);
        /// <summary>
        /// Create TrainData
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        TrainData CreateTrainData(Problem prob);
        /// <summary>
        /// Train using TrainData with current weight
        /// </summary>
        /// <param name="data">data</param>
        /// <param name="weight">current weight</param>
        /// <returns></returns>
        double Train(TrainData data, double[] weight);
        /// <summary>
        /// Classify a sample by weakLearner
        /// </summary>
        /// <param name="vx">a sample</param>
        /// <returns>1 or -1</returns>
        double Classify(Node[] vx);
        /// <summary>
        /// Serialize to an xml node.
        /// </summary>
        /// <param name="weakLearnerNode">an xml node</param>
        /// <returns></returns>
        void SerializeToXml(ref XmlElement weakLearnerNode);
        /// <summary>
        /// Deserialize from an xml node.
        /// </summary>
        /// <param name="weakLearnerNode">an xml node</param>
        /// <returns></returns>
        void DeserializeFromXml(XmlElement weakLearnerNode);
    }
}
