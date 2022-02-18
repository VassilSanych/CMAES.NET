using System;
using System.Collections.Generic;
using CMAESnet;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMAES.NETTests
{
    [TestClass()]
    public class CmaTests
    {
        [TestMethod()]
        public void CmaTest()
        {
            CMA cma = new CMA(Vector<double>.Build.Dense(2), 1.3);

            Console.WriteLine("dd");
        }

        [TestMethod]
        public void CmaesOptimizationTest()
        {
		        double[] initial = { 0, 0 };
		        var cmaoptimizer = new CmaesOptimizer(TestFunction, initial, 1.5);

		        cmaoptimizer.Optimize();

		        var optimizedArray = cmaoptimizer.ResultVector;

		        Console.WriteLine("x1={0}, x2={1}", optimizedArray[0], optimizedArray[1]);


	        double TestFunction(IList<double> x)
	        {
		        return Math.Pow(x[0] - 3, 2) + Math.Pow(10 * (x[1] + 2), 2);
	        }
        }

        [TestMethod]
        public void CmaesParallelOptimizationTest()
        {
	        double[] initial = { 0, 0 };
	        var cmaoptimizer = new CmaesOptimizer(TestFunction, initial, 1.5);

	        cmaoptimizer.OptimizeParallel(7);

	        var optimizedArray = cmaoptimizer.ResultVector;

	        Console.WriteLine("x1={0}, x2={1}", optimizedArray[0], optimizedArray[1]);


	        double TestFunction(IList<double> x)
	        {
		        return Math.Pow(x[0] - 3, 2) + Math.Pow(10 * (x[1] + 2), 2);
	        }
        }

    }
}