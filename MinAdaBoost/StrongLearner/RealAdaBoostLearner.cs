using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;

namespace MinAdaBoost
{
    /// <summary>
    /// RealAdaboost algorithm.
    /// Refer to "Theodoridis.S.,.Koutroumbas.K..Pattern.Recognition,.4ed,.AP,.2009" chapter 4.22.
    /// </summary>
    public class RealAdaBoostLearner : StrongLeaner
    {
        SortedList<int, WeakLearner> _weakLearners = null;

        #region interface StrongLearner

        public void Train(Problem prob, string weakLearnerName, string[] weakLearnerArgs, int iter)
        {
            //Creat weaklearner and traindata
            Assembly asm = Assembly.GetAssembly(typeof(WeakLearner));
            WeakLearner srcLearner = (WeakLearner)asm.CreateInstance(typeof(WeakLearner).Namespace + "." + weakLearnerName, true);
            srcLearner.InitLearningOptions(weakLearnerArgs);
            TrainData traindata = srcLearner.CreateTrainData(prob);

            // set the smoothing value to avoid numerical problem 1/N
            //"Improved boosting algorithms using confidence-rated predictions". chapter 4.2
            double smoothingVal = 1.0 / traindata.N;

            //init weight
            double[] weight = new double[traindata.N];
            for (int t=0;  t < weight.Length; t++)
            {
                weight[t] = smoothingVal;
            }

            //show sth
            Console.WriteLine("\tStrongLearner:{0}", this.GetType().Name);
            Console.WriteLine("\tWeakLearner:{0}", weakLearnerName);
            int cursorX = Console.CursorLeft;
            int cursorY = Console.CursorTop;

            //start iterating
            _weakLearners = new SortedList<int, WeakLearner>(iter);
            for (int t = 0; t < iter; t++)
            {
                //creat a new learner from srcLearner
                WeakLearner subLearner = (WeakLearner)asm.CreateInstance(srcLearner.GetType().FullName);//srcLearner.CreateSubLearner();

                //init args again
                subLearner.InitLearningOptions(weakLearnerArgs);

                //train the learner(the suboptimal solution with current weight)

                double Pm = subLearner.Train(traindata, weight);
                if (Pm >= 0.5)
                {
                    throw new Exception(Messege.CouldNotClassify);
                }

                //calculate Alpha
                //note : eps_min = Pm , eps_pls = 1-Pm
                double eps_min = 0.0, eps_pls = 0.0;
                double[] result = new double[prob.N];
                for (int n = 0; n < prob.N; n++ )
                {
                    result[n] = subLearner.Classify(prob.X[n]);
                    if ((result[n] * prob.Y[n]) < 0) 
                        eps_min += weight[n];
                    if ((result[n] * prob.Y[n]) > 0) 
                        eps_pls += weight[n];
                }

                double Alpha = 0.5 * Math.Log((eps_pls + smoothingVal) / (eps_min + smoothingVal));
                subLearner.Alpha = Alpha;

                //update weight
                double Z = 0;
                for (int n = 0; n < prob.N; n++ )
                {
                    weight[n] = weight[n] * Math.Exp(-1 * prob.Y[n] * result[n] * Alpha);
                    Z += weight[n];
                }
                for (int n = 0; n < prob.N; n++ )
                    weight[n] /= Z;

                //test
                double sum = 0;
                for (int n = 0; n < prob.N; n++)
                    sum += weight[n];

                //save
                _weakLearners.Add(t,subLearner);

                //show sth
                Console.SetCursorPosition(cursorX, cursorY);
                Console.WriteLine("\titerations {0}/{1}", t+1, iter);
            }
        }

        public double Classify(Node[] vx)
        {
            if (_weakLearners == null || _weakLearners.Count <= 0)
            {
                throw new Exception(Messege.WeakLearnerNull);
            }
            double fx = 0;
            foreach (WeakLearner wl in _weakLearners.Values)
            {
                fx += wl.Alpha * wl.Classify(vx);
            }
            if (fx > 0)
                return 1;
            else
                return -1;
        }

        public void SerializeToXml(ref XmlElement strongLearnerNode)
        {
            foreach (KeyValuePair<int, WeakLearner> iterAndWeakLearner in _weakLearners)
            {
                WeakLearner learner = iterAndWeakLearner.Value;//_weakLearners[iter];
                XmlElement learnerNote = strongLearnerNode.OwnerDocument.CreateElement("WeakLearner");
                learnerNote.SetAttribute("type", learner.GetType().Name);
                learnerNote.SetAttribute("iter", iterAndWeakLearner.Key.ToString());
                learner.SerializeToXml(ref learnerNote);
                strongLearnerNode.AppendChild(learnerNote);
            }
        }

        public void DerializeFromXML(XmlElement strongLearnerNode)
        {
            _weakLearners = new SortedList<int, WeakLearner>();
            foreach (XmlElement node in strongLearnerNode.ChildNodes)
            {
                if (node.Name != "WeakLearner")
                    continue;

                int iter = int.Parse(node.Attributes["iter"].Value);
                Assembly asm = Assembly.GetAssembly(typeof(StrongLeaner));
                WeakLearner learner = (WeakLearner)asm.CreateInstance(typeof(StrongLeaner).Namespace + "." + node.Attributes["type"].Value);
                learner.DeserializeFromXml(node);
                _weakLearners.Add(iter, learner);
             }
        }
            
        #endregion
    }
}
