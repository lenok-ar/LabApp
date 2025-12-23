using System;

namespace DichotomyMethodApp
{
    public static class DichotomyAlgorithm
    {
        /// <summary>
        /// Классический метод дихотомии для нахождения корня уравнения f(x) = 0
        /// </summary>
        /// <param name="func">Функция f(x)</param>
        /// <param name="a">Начало интервала</param>
        /// <param name="b">Конец интервала</param>
        /// <param name="epsilon">Точность</param>
        /// <returns>(корень, значение функции, количество итераций)</returns>
        public static (double root, double fValue, int iterations) FindRoot(
            Func<double, double> func, double a, double b, double epsilon)
        {
            // Проверка входных параметров
            if (a >= b)
                throw new ArgumentException("Интервал задан неверно: a должно быть меньше b");

            if (epsilon <= 0)
                throw new ArgumentException("Точность должна быть положительной");

            // Проверка условия f(a)*f(b) < 0
            double fa = func(a);
            double fb = func(b);

            if (fa * fb >= 0)
                throw new ArgumentException($"Функция не меняет знак на интервале [{a}, {b}]");

            int iterations = 0;
            double left = a;
            double right = b;
            double fLeft = fa;

            while (Math.Abs(right - left) > epsilon)
            {
                double mid = (left + right) / 2;
                double fMid = func(mid);

                iterations++;

                // Если значение функции достаточно близко к нулю
                if (Math.Abs(fMid) < epsilon * 0.1)
                    return (mid, fMid, iterations);

                // Определяем новую границу интервала
                if (fLeft * fMid < 0)
                {
                    right = mid;
                }
                else
                {
                    left = mid;
                    fLeft = fMid;
                }

                // Защита от бесконечного цикла
                if (iterations > 1000)
                    throw new InvalidOperationException("Превышено максимальное количество итераций");
            }

            double root = (left + right) / 2;
            double fRoot = func(root);

            return (root, fRoot, iterations);
        }

        /// <summary>
        /// Автоматический поиск интервала, содержащего корень
        /// </summary>
        public static (double a, double b, bool found) FindRootInterval(
            Func<double, double> func, double start, double end, double step)
        {
            double x1 = start;
            double f1 = func(x1);

            for (double x2 = x1 + step; x2 <= end; x2 += step)
            {
                double f2 = func(x2);

                // Если функция меняет знак
                if (f1 * f2 < 0)
                    return (x1, x2, true);

                x1 = x2;
                f1 = f2;
            }

            return (0, 0, false);
        }
    }
}