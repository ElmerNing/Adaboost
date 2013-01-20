using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinAdaBoost
{
    public class Prediction
    {
        public static double Predict(Model model, Node[] vx)
        {
            if (model.NumberOfClasses > 2)
            {
                int labelIndex = 0;
                int count = 0;
                for (int i = 0; i < model.NumberOfClasses; i++)
                {
                    if ( !double.IsNaN(model.BinaryClassifiers[i].Classify(vx)) )
                    {
                        labelIndex = i;
                        count++;
                    }
                }
                if (count == 1)
                {
                    return model.BinaryClassifiers[labelIndex].PosLabel;
                }
                else
                    return double.NaN;
            }
            else if (model.NumberOfClasses == 2)
            {
                return model.BinaryClassifiers[0].Classify(vx);
            }
            else
                return double.NaN;;

        }
    }
}
