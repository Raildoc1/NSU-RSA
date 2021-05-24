using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace RSA
{
    class Program
    {
        public const int k = 100;

        static void Main(string[] args)
        {
            BigInteger p, q;

            byte[] plainText;

            plainText = File.ReadAllBytes("input.txt");

            Console.WriteLine($"plaint text:");
            foreach (var c in plainText)
            {
                Console.Write($"{(char)c}");
            }
            Console.WriteLine("");

            CalculatePQ(out p, out q);

            BigInteger n = BigInteger.Multiply(p, q);
            BigInteger phi = BigInteger.Multiply(p - 1, q - 1);

            Console.WriteLine($"phi = {phi}");

            int e = 65537;

            BigInteger d;

            Console.WriteLine($"gcd = {EuclideGCD(e, phi, out d, out _)}");

            Console.WriteLine($"d = {d}");

            while (d < 0)
            {
                d += phi;
            }

            Console.WriteLine($"d = {d}");

            Console.WriteLine($"d * e = {BigInteger.Multiply(d, e) % phi}");

            Console.WriteLine($"public key = ({e}, {n})");
            Console.WriteLine($"private key = ({d}, {n})");

            BigInteger[] chiper = new BigInteger[plainText.Length];

            if (File.Exists("chiper.txt"))
            {
                File.Delete("chiper.txt");
            }
            File.Create("chiper.txt").Close();

            Console.Write($"chiper:");
            for (int i = 0; i < plainText.Length; i++)
            {
                chiper[i] = BigInteger.ModPow(plainText[i], e, n);

                byte[] bytes = chiper[i].ToByteArray();

                File.AppendAllText("chiper.txt",  System.Text.Encoding.Default.GetString(chiper[i].ToByteArray()));
            }


            Console.WriteLine("");

            BigInteger[] dechiper = new BigInteger[chiper.Length];
            Console.Write($"plain:");
            for (int i = 0; i < plainText.Length; i++)
            {
                dechiper[i] = BigInteger.ModPow(chiper[i], d, n);
                Console.Write((char)(dechiper[i]));
            }
            Console.WriteLine("");

            return;
        }

        private static void CalculatePQ(out BigInteger p, out BigInteger q)
        {
            p = new BigInteger(GetRandomBytes(k));
            q = new BigInteger(GetRandomBytes(k));

            //Random random = new Random();

            //BigInteger p = new BigInteger(random.Next(1_000_000));
            //BigInteger q = new BigInteger(random.Next(1_000_000));

            if (p < 0) { p = -p; }
            if (q < 0) { q = -q; }

            byte[] integerBytes = { 1, 1 };

            Console.WriteLine($"random p = {p}");
            Console.WriteLine($"random q = {q}");

            Primarize(ref p);
            Primarize(ref q);

            Console.WriteLine($"primary p = {p}");
            Console.WriteLine($"primary q = {q}");
        }

        public static byte[] GetRandomBytes(int amount)
        {
            Random random = new Random();
            byte[] result = new byte[amount];
            random.NextBytes(result);
            return result;
        }

        public static void Primarize(ref BigInteger number)
        {
            if (number % 2 == 0)
            {
                number++;
            }

            while (!IsPrime(number))
            {
                number += 2;
            }

            while (!IsAlmostSurelyPrime(number, k))
            {
                number += 2;
            }
        }

        public static bool IsPrime(BigInteger number)
        {
            if (number % 2 == 0)
            {
                return false;
            }

            for (int i = 3; i < 256; i += 2)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAlmostSurelyPrime(BigInteger number, int rounds)
        {
            Random random = new Random();

            BigInteger power = new BigInteger(1);
            int s = 0;
            BigInteger t;

            do
            {
                s++;
                power *= 2;
                t = (number - 1) / power;
            } while (t % 2 == 0);

            for (int i = 0; i < rounds; i++)
            {
                double multiplier = random.NextDouble();
                long longMuliplier = (long)(multiplier * 1_000_000_000);

                BigInteger a = (longMuliplier * number) / 1_000_000_000;
                BigInteger x = BigInteger.ModPow(a, t, number);

                if (x == 1 || x == number - 1)
                {
                    continue;
                }

                for (int j = 0; j < s - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, number);
                    if (x == 1)
                    {
                        return false;
                    }
                    if (x == number - 1)
                    {
                        break;
                    }
                }

                if (x == number - 1)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static BigInteger Euler(BigInteger n)
        {
            BigInteger result = n;
            for (int i = 2; i * i <= n; i++)
            {
                if (n % i == 0)
                {
                    while (n % i == 0)
                    {
                        n /= i;
                    }
                    result -= result / i;
                }
            }
            if (n > 1)
            {
                result -= result / n;
            }
            return result;
        }

        public static BigInteger EuclideGCD(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }
            BigInteger x1, y1;
            BigInteger g = EuclideGCD(BigInteger.ModPow(b, 1, a), a, out x1, out y1);
            x = BigInteger.Subtract(y1, BigInteger.Multiply(BigInteger.Divide(b, a), x1));
            y = x1;
            return g;
        }
    }
}
