using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MinAdaBoost
{
    public class Training
    {
        public static Model Train(Problem prob, TrainingArg arg)
        {
            List<double> labels = GetLabels(prob);
            if (labels.Count > 2)
            {
                List<BinaryClassifier> classifiers = new List<BinaryClassifier>(labels.Count);
                foreach (double label in labels)
                {
                    Console.WriteLine("{0} vs all:",label);
                    BinaryClassifier classifier = BinaryClassifier.Train(prob, arg, label);
                    classifiers.Add(classifier);
                    Console.WriteLine("finish");
                }
                Model model = new Model();
                model.NumberOfClasses = classifiers.Count;
                model.BinaryClassifiers = classifiers.ToArray();
                return model;
            }
            else if (labels.Count == 2)
            {
                BinaryClassifier[] classifiers = new BinaryClassifier[1];
                classifiers[0] = BinaryClassifier.Train(prob, arg, labels[0], labels[1]);
                Model model = new Model();
                model.NumberOfClasses = 2;
                model.BinaryClassifiers = classifiers;
                return model;
            }
            else
                throw new Exception(Messege.CouldNotClassify); 
        }

        private static List<double> GetLabels(Problem prob)
        {
            List<double> labels = new List<double>();
            for (int n = 0; n < prob.N; n++)
            {
                double y = prob.Y[n];
                if (!labels.Contains(y))
                {
                    labels.Add(y);
                }
            }
            return labels;
        }
    }

    public class TrainingArg
    {
        public string StrongLearnerName = "RealAdaBoostLearner";
        public string[] StrongLearnerArgs = null;
        public string WeakLearnerName = "StumpLearner";
        public string[] WeakLearnerArgs = null;
        public int Iterations = 200;
        public TrainingArg()
        { }
        public TrainingArg(string strongLearnerName, string[] strongLearnerArgs, string weakLearnerName, string[] weakLearnerArgs, int iterations)
        {
            StrongLearnerName = strongLearnerName;
            StrongLearnerArgs = strongLearnerArgs;
            WeakLearnerName = weakLearnerName;
            WeakLearnerArgs = weakLearnerArgs;
            Iterations = iterations;
        }
    }
}
