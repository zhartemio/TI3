using System;
using System.Numerics;

namespace TI_3;

static class RSA
{
 
    public static BigInteger EulerPhi(BigInteger n)
    {
        
        BigInteger result = n; 

        for (BigInteger p = 2; p * p <= n; ++p)
        {
            if (n % p == 0)
            {
            
                while (n % p == 0)
                {
                    n /= p;
                }
                result -= result / p;
            }
        }

      
        if (n > 1)
        {
            result -= result / n;
        }

        return result;
    }
    
    public static bool IsPrime(BigInteger number)
    {
        if (number <= 1)
            return false;
        if (number <= 3)
            return true;

        if (number % 2 == 0 || number % 3 == 0)
            return false;

        for (BigInteger i = 5; i * i <= number; i += 6)
        {
            if (number % i == 0 || number % (i + 2) == 0)
                return false;
        }

        return true;
    }
    
    public static BigInteger FindGcd(BigInteger a, BigInteger b) => b == 0 ? a : FindGcd(b, a % b);
    
    public static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedEuclidean(BigInteger a, BigInteger b)
    {

		BigInteger x0 = 1, y0 = 0, x1 = 0, y1 = 1;
		BigInteger d0 = a, d1 = b;

        while (d1 != 0)
        {

			BigInteger q = d0 / d1;
			BigInteger d2 = d0 % d1;
			BigInteger x2 = x0 - q * x1;
			BigInteger y2 = y0 - q * y1;

           
            d0 = d1;
            d1 = d2;
            x0 = x1;
            x1 = x2;
            y0 = y1;
            y1 = y2;
        }

        if (y0 < 0) 
        {
            y0 += a;
        }

 
        return (d0, x0, y0);
    }

    
    public static BigInteger QuickPowerMod(BigInteger num, BigInteger power, BigInteger mod)
    {
        if (mod == 1)
            return 0;
        
        if (power == 0)
            return 1;

        if (num == 0)
            return 0;

		BigInteger result = 1;
		BigInteger current = num % mod;
		BigInteger exponent = power;
        
        while (exponent > 0)
        {
            if (exponent % 2 == 1)
                result = (result * current) % mod;

            current = (current * current) % mod;
            exponent /= 2;
        }

        return result;
    }
}