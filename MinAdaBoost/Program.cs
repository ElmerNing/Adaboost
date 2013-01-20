using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MinAdaBoost
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ExitWindowsEx(UInt32 uFlags, UInt32 dwReason);

        static void Main(string[] args)
        {
            /*
            Problem pro = Problem.Read(@"E:\SerialArea Reflect\Model\_model\total.datset");
            TrainingArg arg = new TrainingArg();
            arg.Iterations = 300;
            arg.WeakLearnerName = "TreeLearner";
            Model model = Training.Train(pro, arg);
            Model.Write("222.xml",model);*/

            Model model = Model.Read("222.xml");
            Problem pro = Problem.Read(@"E:\SerialArea Reflect\2012-2-13lib\total.datset");
            //Problem pro = Problem.Read(@"E:\SerialArea Reflect\Model\_model\total.datset");
            int error = 0;
            Console.WriteLine("sss");
            for (int i=0; i<pro.N; i++)
            {
                if(Prediction.Predict(model,pro.X[i]) != pro.Y[i])
                    error++;
            }
            Console.Write(error);
             
            /*
            Problem pro = Problem.Read("ZM.dat");
            TrainingArg arg = new TrainingArg();
            arg.WeakLearnerName = "TreeLearner";
            string[] a = { "2" };
            arg.WeakLearnerArgs = a;
            arg.Iterations = 200;
            Model model = Training.Train(pro, arg);
            Model.Write("123.xml", model);
            model = Model.Read("123.xml");*/
        }


    }
}
