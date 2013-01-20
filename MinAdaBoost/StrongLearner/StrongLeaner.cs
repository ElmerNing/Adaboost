using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MinAdaBoost
{
    /// <summary>
    /// Base strong learner interface.
    /// all the other strong learner should implement the interface.
    /// </summary>
    public interface StrongLeaner
    {
        /// <summary>
        /// Train a problem
        /// </summary>
        /// <param name="prob"></param>
        /// <param name="weakLearnerArgs">
        ///     options of weakLearner.
        ///     weakLearnerArgs[0] is the name of WeakLearner, such as "StumpLearner"
        /// </param>
        /// <param name="iter">iterations</param>
        void Train(Problem prob, string weakLearnerName, string[] weakLearnerArgs, int iter);
        /// <summary>
        /// Classify a sample
        /// </summary>
        /// <param name="vx">sample</param>
        /// <returns>1 or -1</returns>
        double Classify(Node[] vx);
        /// <summary>
        /// Serialize to an xml node.
        /// </summary>
        /// <param name="strongLearnerNode">an xml node</param>
        /// <returns></returns>
        void SerializeToXml(ref XmlElement strongLearnerNode);
        /// <summary>
        /// Deserialize from an xml node.
        /// </summary>
        /// <param name="strongLearnerNode">an xml node</param>
        /// <returns></returns>
        void DerializeFromXML(XmlElement strongLearnerNode);
    }
}
