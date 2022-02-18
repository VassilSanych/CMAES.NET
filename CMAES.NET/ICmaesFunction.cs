using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace CMAESnet;

public interface ICmaesFunction
{
	/// <summary>
	///  Целевая функция
	/// </summary>
	/// <param name="ai_var"></param>
	/// <returns>результат функции с обратным знаком (алгоритм оптимизации минимизирует, а нам нужно максимизировать)</returns>
	double F(IList<double> ai_var);

	double[] Initial { get; init; }
	double Sigma => 1.5;
	double[] LowerBounds{ get; init; }
	double[] UpperBounds{ get; init; }
	int MaxIteration => 200;
	int RandSeed => 0;
}