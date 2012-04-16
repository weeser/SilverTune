using System;

namespace SilverTune
{
    /*Documentation
     * Struct:  ComplexNumber
     * Author:  Josh Weese with credit to AForge.net
     * Purpose: Contains the necessary methods and variables for creating a complex number
     * Methods: 
     * Globals:   Re...real part
     *              Im...imaginary part
     *              Magnitude
     *              Phase
     *              SquaredMagnitude
     *              PoweredE
     *              Power2
     * Notes:
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public struct ComplexNumber 
    {
        public double Re;
        public double Im;

        /// <summary>
        /// Magnitude value of the complex number.
        /// </summary>
        /// 
        /// <remarks><para>Magnitude of the complex number, which equals to <b>Sqrt( Re * Re + Im * Im )</b>.</para></remarks>
        /// 
        public double Magnitude
        {
            get { return System.Math.Sqrt(Re * Re + Im * Im); }
        }

        /// <summary>
        /// Phase value of the complex number.
        /// </summary>
        /// 
        /// <remarks><para>Phase of the complex number, which equals to <b>Atan( Im / Re )</b>.</para></remarks>
        /// 
        public double Phase
        {
            get { return System.Math.Atan(Im / Re); }
        }

        /// <summary>
        /// Squared magnitude value of the complex number.
        /// </summary>
        public double SquaredMagnitude
        {
            get { return (Re * Re + Im * Im); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Complex"/> class.
        /// </summary>
        /// 
        /// <param name="c">Source complex number.</param>
        /// 
        public ComplexNumber( ComplexNumber c )
        {
            this.Re = c.Re;
            this.Im = c.Im;
        }

        public ComplexNumber(double re)
        {
            this.Re = re;
            this.Im = 0;
        }

        public ComplexNumber(double re, double im)
        {
            this.Re = re;
            this.Im = im;
        }

        public static ComplexNumber operator *(ComplexNumber n1, ComplexNumber n2)
        {
            return new ComplexNumber(n1.Re * n2.Re - n1.Im * n2.Im,
                n1.Im * n2.Re + n1.Re * n2.Im);
        }

        public static ComplexNumber operator +(ComplexNumber n1, ComplexNumber n2)
        {
            return new ComplexNumber(n1.Re + n2.Re, n1.Im + n2.Im);
        }

        public static ComplexNumber operator -(ComplexNumber n1, ComplexNumber n2)
        {
            return new ComplexNumber(n1.Re - n2.Re, n1.Im - n2.Im);
        }

        public static ComplexNumber operator -(ComplexNumber n)
        {
            return new ComplexNumber(-n.Re, -n.Im);
        }

        public static implicit operator ComplexNumber(double n)
        {
            return new ComplexNumber(n, 0);
        }

        public ComplexNumber PoweredE()
        {
            double e = Math.Exp(Re);
            return new ComplexNumber(e * Math.Cos(Im), e * Math.Sin(Im));
        }

        public double Power2()
        {
            return Re * Re - Im * Im;
        }

        public override string ToString()
        {
            return String.Format("{0}+i*{1}", Re, Im);
        }
    }

    /*Documentation
     * class:  ComplexNumberConstants
     * Author:  Josh Weese with credit to AForge.net
     * Purpose: Contains the constants used in operations and manipulation of complex numbers
     * Methods: 
     * Globals:   Zero
     *              One
     *              I
     * Notes:
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class ComplexNumberConstants
    {
        /// <summary>
        ///  A double-precision complex number that represents zero.
        /// </summary>
        public static readonly ComplexNumber Zero = new ComplexNumber(0, 0);

        /// <summary>
        ///  A double-precision complex number that represents one.
        /// </summary>
        public static readonly ComplexNumber One = new ComplexNumber(1, 0);

        /// <summary>
        ///  A double-precision complex number that represents the squere root of (-1).
        /// </summary>
        public static readonly ComplexNumber I = new ComplexNumber(0, 1);
    }
}