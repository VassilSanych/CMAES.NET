using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace CMAESnet
{
	public class CmaesOptimizer
	{
		private readonly CMA _cma;
		private readonly Func<Vector<double>, double> _function;
		private readonly int _maxIteration;

		/// <summary>
		/// Array of optimization results
		/// </summary>
		public double[] ResultVector { get; private set; }

		/// <summary>
		/// The result of assigning the optimization result array to the target function
		/// </summary>
		public double ResultValue { get; private set; }


		/// <summary>
		/// An Optimization Solver Using CMA-ES
		/// </summary>
		/// <param name="function">Objective function.</param>
		/// <param name="initial">Initial values.</param>
		/// <param name="sigma">Step size of CMA-ES.</param>
		/// <param name="lowerBounds">Lower limit of the optimized search range.</param>
		/// <param name="upperBounds">Upper limit of the optimized search range.</param>
		/// <param name="randSeed">(Optional) A seed number.</param>
		public CmaesOptimizer(
			Func<IList<double>, double> function,
			double[] initial,
			double sigma,
			double[] lowerBounds = null,
			double[] upperBounds = null,
			int randSeed = 0)
		{
			if (lowerBounds != null && initial.Length != lowerBounds.Length)
			{
				throw new ArgumentException("Length of lowerBounds must be equal to that of initial.");
			}

			if (upperBounds != null && initial.Length != upperBounds.Length)
			{
				throw new ArgumentException("Length of upperBounds must be equal to that of initial");
			}

			_function = function;
			_maxIteration = initial.Length * 200;

			Matrix<double> bounds = null;
			if (upperBounds?.Any() == true && lowerBounds?.Any() == true)
			{
				bounds = Matrix<double>.Build.Dense(initial.Length, 2);
				bounds.SetColumn(0, lowerBounds);
				bounds.SetColumn(1, upperBounds);
			}

			_cma = new CMA(initial, sigma, bounds, seed: randSeed);

			ResultValue = double.MaxValue;
		}


		public CmaesOptimizer(
			ICmaesFunction func)
		{
			_function = func.F;
			_maxIteration = func.Initial.Length * func.MaxIteration;

			Matrix<double> bounds = null;
			if (func.UpperBounds?.Any() == true && func.LowerBounds?.Any() == true)
			{
				bounds = Matrix<double>.Build.Dense(func.Initial.Length, 2);
				bounds.SetColumn(0, func.LowerBounds);
				bounds.SetColumn(1, func.UpperBounds);
			}
			
			_cma = new CMA(func.Initial, func.Sigma, bounds, seed: func.RandSeed);
			

			ResultValue = double.MaxValue;
		}

		/// <summary>
		/// Perform optimization calculations with CMA-ES.
		/// </summary>
		public void Optimize()
		{
			(Vector<double>, double) bestSolution = default;
			var yBest = double.PositiveInfinity;

			(Vector<double>, double)[] solutions = new (Vector<double>, double)[_cma.PopulationSize];
			var isConverged = false;
			try
			{
				for (var generation = 0; generation < _maxIteration; generation++)
				{
					for (var i = 0; i < _cma.PopulationSize; i++)
					{
						var x = _cma.Ask();
						var value = _function(x);
						var solution = (x, value);
						if (value < yBest)
						{
							yBest = value;
							bestSolution = solution;
						}

						solutions[i] = (x, value);
					}

					_cma.Tell(solutions);
					isConverged = _cma.IsConverged();
					if (!isConverged)
						continue;

					break;
				}
			}
			catch (NonConvergenceException)
			{
				Console.WriteLine("Reached MathNet max iteration.");
			}

			if (!isConverged)
				Console.WriteLine("Reached max iteration.");

			ResultVector = bestSolution.Item1.ToArray();
			ResultValue = yBest;
		}


		/// <summary>
		/// Perform optimization calculations with CMA-ES.
		/// </summary>
		public void OptimizeParallel(int coreCount)
		{
			(Vector<double>, double) best = default;
			var yBest = double.PositiveInfinity;
			var isConverged = false;

			var vectors = new Vector<double>[_cma.PopulationSize];
			var solutions = new (Vector<double>, double)[_cma.PopulationSize];
			try
			{
				for (var generation = 0; generation < _maxIteration; generation++)
				{
					for (var i = 0; i < _cma.PopulationSize; i++)
						vectors[i] = _cma.Ask();

					var stopped = false;
					Parallel.ForEach(
						vectors,
						new ParallelOptions {MaxDegreeOfParallelism = coreCount},
						(vector, state, i) =>
						{
							var value = _function(vector);
							solutions[i] = (vector, value);
							var stateStopped = state.IsExceptional || state.IsStopped || state.ShouldExitCurrentIteration;
							stopped = stopped || stateStopped; // not safe but ok
						});

					if (stopped)
						break;

					_cma.Tell(solutions);
					var currentBest = solutions.MinBy(x => x.Item2);
					var yCurrentBest = currentBest.Item2;

					isConverged = _cma.IsConverged();

					if (yCurrentBest < yBest)
					{
						yBest = yCurrentBest;
						best = currentBest;
					}

					if (isConverged)
					{
						break;
					}
				}

			}
			catch (NonConvergenceException)
			{
				Console.WriteLine("Reached MathNet max iteration.");
			}

			if (!isConverged)
				Console.WriteLine("Reached max iteration.");

			ResultVector = best.Item1.ToArray();
			ResultValue = best.Item2;
		}
	}
}
