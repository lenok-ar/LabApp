using System;
using NCalc;
using System.Text.RegularExpressions;
using System.Linq;

namespace DichotomyMethodApp
{
    public class DichotomyMethod
    {
        private readonly Expression _expression;

        public DichotomyMethod(string function)
        {
            string processedFunction = ProcessFunctionForNCalc(function.ToLower());
            _expression = new Expression(processedFunction, EvaluateOptions.IgnoreCase);

            _expression.Parameters["pi"] = Math.PI;
            _expression.Parameters["e"] = Math.E;

            _expression.EvaluateFunction += EvaluateFunction;
            _expression.EvaluateParameter += EvaluateParameter;
        }

        private string ProcessFunctionForNCalc(string function)
        {
            string result = function;
            result = ConvertPowerOperator(result);
            return result;
        }

        private string ConvertPowerOperator(string expression)
        {
            string result = expression;
            int maxIterations = 20;
            int iteration = 0;

            while (iteration < maxIterations)
            {
                Match match = Regex.Match(result, @"([a-zA-Z0-9\.\(\)]+)\s*\^\s*([a-zA-Z0-9\.\(\)]+)");

                if (!match.Success)
                    break;

                string left = match.Groups[1].Value.Trim();
                string right = match.Groups[2].Value.Trim();

                if (left.Contains('+') || left.Contains('-') || left.Contains('*') || left.Contains('/'))
                    left = $"({left})";

                if (right.Contains('+') || right.Contains('-') || right.Contains('*') || right.Contains('/'))
                    right = $"({right})";

                string replacement = $"pow({left},{right})";
                result = result.Replace(match.Value, replacement);
                iteration++;
            }

            return result;
        }

        private void EvaluateParameter(string name, ParameterArgs args)
        {
            switch (name.ToLower())
            {
                case "pi":
                    args.Result = Math.PI;
                    break;
                case "e":
                    args.Result = Math.E;
                    break;
            }
        }

        private void EvaluateFunction(string name, FunctionArgs args)
        {
            switch (name.ToLower())
            {
                case "sin":
                    args.Result = Math.Sin(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "cos":
                    args.Result = Math.Cos(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "tan":
                    args.Result = Math.Tan(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "atan":
                    args.Result = Math.Atan(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "exp":
                    args.Result = Math.Exp(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "sqrt":
                    args.Result = Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "abs":
                    args.Result = Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    break;
                case "log":
                    if (args.Parameters.Length == 1)
                        args.Result = Math.Log(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    else if (args.Parameters.Length == 2)
                        args.Result = Math.Log(Convert.ToDouble(args.Parameters[0].Evaluate()),
                                             Convert.ToDouble(args.Parameters[1].Evaluate()));
                    else
                        throw new ArgumentException("Функция log требует 1 или 2 аргумента");
                    break;
                case "log10":
                    if (args.Parameters.Length == 1)
                        args.Result = Math.Log10(Convert.ToDouble(args.Parameters[0].Evaluate()));
                    else
                        throw new ArgumentException("Функция log10 требует 1 аргумент");
                    break;
                case "pow":
                    if (args.Parameters.Length == 2)
                        args.Result = Math.Pow(
                            Convert.ToDouble(args.Parameters[0].Evaluate()),
                            Convert.ToDouble(args.Parameters[1].Evaluate()));
                    else
                        throw new ArgumentException("Функция pow требует 2 аргумента");
                    break;
                default:
                    throw new ArgumentException($"Неизвестная функция: {name}");
            }
        }

        public double CalculateFunction(double x)
        {
            try
            {
                _expression.Parameters["x"] = x;
                var result = _expression.Evaluate();

                if (result is double doubleResult)
                {
                    if (double.IsInfinity(doubleResult) || double.IsNaN(doubleResult))
                        return double.MaxValue;
                    return doubleResult;
                }

                if (result is int intResult)
                    return intResult;

                if (result is decimal decimalResult)
                    return (double)decimalResult;

                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка вычисления функции в точке x={x}: {ex.Message}");
            }
        }

        public (double root, double fValue, int iterations) FindRoot(double a, double b, double epsilon)
        {
            if (a >= b)
                throw new ArgumentException("Интервал [a, b] задан неверно: a должно быть меньше b");

            if (epsilon <= 0)
                throw new ArgumentException("Точность epsilon должна быть положительной");

            // Вычисляем значения функции на концах интервала
            double fa = CalculateFunction(a);
            double fb = CalculateFunction(b);

            // Если на одном из концов функция уже равна 0 (или очень близка)
            if (Math.Abs(fa) < epsilon * 10)
                return (a, fa, 0);

            if (Math.Abs(fb) < epsilon * 10)
                return (b, fb, 0);

            // Проверяем условие f(a)*f(b) < 0 (разные знаки)
            if (fa * fb > 0)  // Изменено: теперь > 0 вместо >= 0
                throw new ArgumentException($"Функция не меняет знак на концах интервала.\n" +
                                          $"f({a}) = {fa}\nf({b}) = {fb}\n" +
                                          $"Условие f(a)*f(b) < 0 не выполняется.");

            int iterations = 0;
            double left = a;
            double right = b;
            double fLeft = fa;

            // Основной цикл метода дихотомии
            while (Math.Abs(right - left) > epsilon)
            {
                iterations++;

                // Вычисляем середину интервала
                double mid = (left + right) / 2;
                double fMid = CalculateFunction(mid);

                // Если значение функции в середине достаточно близко к нулю
                if (Math.Abs(fMid) < epsilon)
                {
                    // Уточняем корень дополнительными вычислениями
                    for (int i = 0; i < 3; i++)
                    {
                        double testMid = (left + right) / 2;
                        double testFmid = CalculateFunction(testMid);

                        if (Math.Abs(testFmid) < Math.Abs(fMid))
                        {
                            mid = testMid;
                            fMid = testFmid;
                        }

                        // Сдвигаем границы для лучшего уточнения
                        if (fLeft * testFmid < 0)
                            right = testMid;
                        else
                            left = testMid;
                    }
                    return (mid, fMid, iterations);
                }

                // Определяем, в какой половине находится корень
                if (fLeft * fMid < 0)
                {
                    // Корень в левой половине
                    right = mid;
                }
                else
                {
                    // Корень в правой половине
                    left = mid;
                    fLeft = fMid;
                }

                // Защита от бесконечного цикла
                if (iterations > 100)
                    break;
            }

            // Возвращаем приближенное значение корня
            double root = (left + right) / 2;
            double fRoot = CalculateFunction(root);

            return (root, fRoot, iterations);
        }

        // Проверка функции на интервале
        public bool TestFunctionOnInterval(double a, double b)
        {
            try
            {
                int testPoints = 10;
                double step = (b - a) / testPoints;

                for (int i = 0; i <= testPoints; i++)
                {
                    double x = a + i * step;
                    double value = CalculateFunction(x);
                    if (double.IsNaN(value) || double.IsInfinity(value))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Поиск интервала, содержащего корень
        public (double a, double b, bool found)? FindRootInterval(double start, double end, double step)
        {
            double x1 = start;
            double f1 = CalculateFunction(x1);

            for (double x2 = x1 + step; x2 <= end; x2 += step)
            {
                double f2 = CalculateFunction(x2);

                if (f1 * f2 < 0)
                    return (x1, x2, true);

                x1 = x2;
                f1 = f2;
            }

            return null;
        }
    }
}