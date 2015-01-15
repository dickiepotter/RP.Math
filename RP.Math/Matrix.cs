using System;

namespace RPUtil.Math.Math3D
{
    public class Matrix: IEquatable<Matrix>, IFormattable
    {
        private readonly double[,] _vals;

        #region Constructors

        public Matrix(double[,] arr)
        {
            if (arr.GetLength(0) == 3)
            {
                if (arr.GetLength(1) == 3) 
                {
                    _vals = new double[,] 
                    {
                        {arr[0,0], arr[0,1], arr[0,2], 0},
                        {arr[1,0], arr[1,1], arr[1,2], 0},
                        {arr[2,0], arr[2,1], arr[2,2], 0},
                        {0       , 0       , 0       , 1}
                    };
                }
                if (arr.GetLength(1) == 4) 
                {
                    _vals = new double[,] 
                    {
                        {arr[0,0], arr[0,1], arr[0,2], arr[0,3]},
                        {arr[1,0], arr[1,1], arr[1,2], arr[1,3]},
                        {arr[2,0], arr[2,1], arr[2,2], arr[2,3]},
                        {0       , 0       , 0       , 1       }
                    };
                }
            }
            else if (arr.GetLength(0) == 4)
            {
                if (arr.GetLength(1) == 3)
                {
                    _vals = new double[,] 
                    {
                        {arr[0,0], arr[0,1], arr[0,2], 0},
                        {arr[1,0], arr[1,1], arr[1,2], 0},
                        {arr[2,0], arr[2,1], arr[2,2], 0},
                        {arr[3,0], arr[3,1], arr[3,2], 1}
                    };
                }
                if (arr.GetLength(1) == 4) 
                {
                    _vals = new double[,] 
                    {
                        {arr[0,0], arr[0,1], arr[0,2], arr[0,3]},
                        {arr[1,0], arr[1,1], arr[1,2], arr[1,3]},
                        {arr[2,0], arr[2,1], arr[2,2], arr[2,3]},
                        {arr[3,0], arr[3,1], arr[3,2], arr[3,3]}
                    };
                }
            }

            if (_vals == null) throw new ArgumentException(MULTI_DIM_COMPONENT_COUNT, "arr");
        }

        public Matrix( double[] arr ) 
        {
            switch (arr.Length)
            {
                case 3:
                    _vals = new double[,] 
                    {
                        {arr[0], 0     , 0     , 0},
                        {0     , arr[1], 0     , 0},
                        {0     , 0     , arr[2], 0},
                        {0     , 0     , 0     , 1}
                    };
                    break;
                case 9:
                    _vals = new double[,] 
                    {
                        {arr[0], arr[1], arr[2], 0},
                        {arr[3], arr[4], arr[5], 0},
                        {arr[6], arr[7], arr[8], 0},
                        {0     , 0     , 0     , 1}
                    };
                    break;
                case 16:
                    _vals = new double[,] 
                    {
                        {arr[0] , arr[1] , arr[2] , arr[3] },
                        {arr[4] , arr[5] , arr[6] , arr[7] },
                        {arr[8] , arr[9] , arr[10], arr[11]},
                        {arr[12], arr[13], arr[14], arr[15]}
                    };
                    break;
                default:
                    throw new ArgumentException(COMPONENT_COUNT, "arr");
            }
        }

        public Matrix() 
        {
            _vals = new double[4,4];
        }

        public Matrix(Matrix m1) 
        {
            _vals = new double[,] 
            {
                {m1.A1_1, m1.A1_2, m1.A1_3, m1.A1_4},
                {m1.A2_1, m1.A2_2, m1.A2_3, m1.A2_4},
                {m1.A3_1, m1.A3_2, m1.A3_3, m1.A3_4},
                {m1.A4_1, m1.A4_2, m1.A4_3, m1.A4_4}
            };
        }

        public Matrix
        (
            double a1_1,
            double a1_2,
            double a1_3,
            double a1_4,
            double a2_1,
            double a2_2,
            double a2_3,
            double a2_4,
            double a3_1,
            double a3_2,
            double a3_3,
            double a3_4,
            double a4_1,
            double a4_2,
            double a4_3,
            double a4_4
        )
        {
            _vals = new double[,] 
            {
                {a1_1, a1_2, a1_3, a1_4},
                {a2_1, a2_2, a2_3, a2_4},
                {a3_1, a3_2, a3_3, a3_4},
                {a4_1, a4_2, a4_3, a4_4}
            };
        }

        public Matrix
        (
            double a1_1,
            double a1_2,
            double a1_3,
            double a2_1,
            double a2_2,
            double a2_3,
            double a3_1,
            double a3_2,
            double a3_3
        )
        {
            _vals = new double[,] 
            {
                {a1_1, a1_2, a1_3, 0},
                {a2_1, a2_2, a2_3, 0},
                {a3_1, a3_2, a3_3, 0},
                {0   , 0   , 0   , 1}
            };
        }

        public Matrix
        (
            Vector v1,
            Vector v2,
            Vector v3
        ) 
        {
            _vals = new double[,] 
            {
                {v1.X, v1.Y, v1.Z, 0},
                {v2.X, v2.Y, v2.Z, 0},
                {v3.X, v3.Y, v3.Z, 0},
                {0   , 0   , 0   , 1}
            };
        }

        public Matrix(Vector xyz) 
        {
            _vals = new double[,] 
            {
                {xyz.X, 0    , 0    , 0},
                {0    , xyz.Y, 0    , 0},
                {0    , 0    , xyz.Z, 0},
                {0    , 0    , 0    , 1}
            };
        }

        #endregion

        #region Accessors

        public double A1_1 { get { return _vals[0, 0]; } }
        public double A1_2 { get { return _vals[0, 1]; } }
        public double A1_3 { get { return _vals[0, 2]; } }
        public double A1_4 { get { return _vals[0, 3]; } }
        public double A2_1 { get { return _vals[1, 0]; } }
        public double A2_2 { get { return _vals[1, 1]; } }
        public double A2_3 { get { return _vals[1, 2]; } }
        public double A2_4 { get { return _vals[1, 3]; } }
        public double A3_1 { get { return _vals[2, 0]; } }
        public double A3_2 { get { return _vals[2, 1]; } }
        public double A3_3 { get { return _vals[2, 2]; } }
        public double A3_4 { get { return _vals[2, 3]; } }
        public double A4_1 { get { return _vals[3, 0]; } }
        public double A4_2 { get { return _vals[3, 1]; } }
        public double A4_3 { get { return _vals[3, 2]; } }
        public double A4_4 { get { return _vals[3, 3]; } }

        public double[] A1 { get { return new double[] { A1_1, A1_2, A1_3, A1_4 }; } }
        public double[] A2 { get { return new double[] { A2_1, A2_2, A2_3, A2_4 }; } }
        public double[] A3 { get { return new double[] { A3_1, A3_2, A3_3, A3_4 }; } }
        public double[] A4 { get { return new double[] { A4_1, A4_2, A4_3, A4_4 }; } }

        public double this[int index] 
        {
            get
            {
                if (index >= 0 && index < 16)
                {
                    int row = (int)System.Math.Floor((double)(index/4));
                    int col = index - (4 * row);
                    return _vals[row, col];
                }
                throw new ArgumentOutOfRangeException("index", index, INDEX_COUNT);
            }
        }

        public double this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 3) throw new ArgumentOutOfRangeException("row", row, INDEX_COUNT);
                if (column < 0 || column > 3) throw new ArgumentOutOfRangeException("column", column, INDEX_COUNT);
                return _vals[row, column];
            }
        }

        #endregion

        #region Constants and Identities

        public static readonly Matrix Zero      = new Matrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        public static readonly Matrix Identity  = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        #endregion

        #region messages

        private const string COMPONENT_COUNT            = "Array must contain exactly three, nine or sixteen components.";
        private const string INDEX_COUNT                = "Matrix index must be zero or greater and less than sixteen (4x4).";
        private const string MULTI_DIM_COMPONENT_COUNT  = "Array must have dimensions of [3,3], [3,4], [4,3] or [4,4].";
        private const string THREE_COMPONENTS           = "Transformation operation array arguments must have exactly three components (_x,_y,_z)";
        private const string NOT_HOMOGENEOUS            = "The product of a Matrix and a Vector does not resolve to a homogeneous vector e.g.(_x,_y,_z,1).";

        #endregion

        #region Operators

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {m1[0,0]+m2[0,0], m1[0,1]+m2[0,1], m1[0,2]+m2[0,2], m1[0,3]+m2[0,3]},
                        {m1[1,0]+m2[1,0], m1[1,1]+m2[1,1], m1[1,2]+m2[1,2], m1[1,3]+m2[1,3]},
                        {m1[2,0]+m2[2,0], m1[2,1]+m2[2,1], m1[2,2]+m2[2,2], m1[2,3]+m2[2,3]},
                        {m1[3,0]+m2[3,0], m1[3,1]+m2[3,1], m1[3,2]+m2[3,2], m1[3,3]+m2[3,3]}
                    }
                )
            );
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {m1[0,0]-m2[0,0], m1[0,1]-m2[0,1], m1[0,2]-m2[0,2], m1[0,3]-m2[0,3]},
                        {m1[1,0]-m2[1,0], m1[1,1]-m2[1,1], m1[1,2]-m2[1,2], m1[1,3]-m2[1,3]},
                        {m1[2,0]-m2[2,0], m1[2,1]-m2[2,1], m1[2,2]-m2[2,2], m1[2,3]-m2[2,3]},
                        {m1[3,0]-m2[3,0], m1[3,1]-m2[3,1], m1[3,2]-m2[3,2], m1[3,3]-m2[3,3]}
                    }
                )
            );
        }

        // Scalar multiplication
        public static Matrix operator *(Matrix m1, double s2)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {m1[0,0]*s2, m1[0,1]*s2, m1[0,2]*s2, m1[0,3]*s2},
                        {m1[1,0]*s2, m1[1,1]*s2, m1[1,2]*s2, m1[1,3]*s2},
                        {m1[2,0]*s2, m1[2,1]*s2, m1[2,2]*s2, m1[2,3]*s2},
                        {m1[3,0]*s2, m1[3,1]*s2, m1[3,2]*s2, m1[3,3]*s2}
                    }
                )
            );
        }

        public static Vector operator *(Matrix m1, Vector v2)
        {
            Vector toRet =  new Vector
            (
                m1[0,0]*v2.X + m1[0,1]*v2.Y + m1[0,2]*v2.Z + m1[0,3]*1,
                m1[1,0]*v2.X + m1[1,1]*v2.Y + m1[1,2]*v2.Z + m1[1,3]*1,
                m1[2,0]*v2.X + m1[2,1]*v2.Y + m1[2,2]*v2.Z + m1[2,3]*1
                
            );

            double w = m1[3,0]*v2.X + m1[3,1]*v2.Y + m1[3,2]*v2.Z + m1[3,3]*1;
            if (w != 1) throw new ArithmeticException(NOT_HOMOGENEOUS);

            return toRet;
            
        }

        public static Matrix operator *(double s1, Matrix m2) { return m2 * s1;  }

        // Matrix multiplication non-commutative
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            return
            (
                new Matrix
                    (
                        new double[,] 
                        {
                            {
                                m1[0,0]*m2[0,0] + m1[1,0]*m2[0,1] + m1[2,0]*m2[0,2] + m1[3,0]*m2[0,3],
                                m1[0,1]*m2[0,0] + m1[1,1]*m2[0,1] + m1[2,1]*m2[0,2] + m1[3,1]*m2[0,3],
                                m1[0,2]*m2[0,0] + m1[1,2]*m2[0,1] + m1[2,2]*m2[0,2] + m1[3,2]*m2[0,3],
                                m1[0,3]*m2[0,0] + m1[1,3]*m2[0,1] + m1[2,3]*m2[0,2] + m1[3,3]*m2[0,3]
                            },
                            {
                                m1[0,0]*m2[1,0] + m1[1,0]*m2[1,1] + m1[2,0]*m2[1,2] + m1[3,0]*m2[1,3],
                                m1[0,1]*m2[1,0] + m1[1,1]*m2[1,1] + m1[2,1]*m2[1,2] + m1[3,1]*m2[1,3],
                                m1[0,2]*m2[1,0] + m1[1,2]*m2[1,1] + m1[2,2]*m2[1,2] + m1[3,2]*m2[1,3],
                                m1[0,3]*m2[1,0] + m1[1,3]*m2[1,1] + m1[2,3]*m2[1,2] + m1[3,3]*m2[1,3]
                            },
                            {
                                m1[0,0]*m2[2,0] + m1[1,0]*m2[2,1] + m1[2,0]*m2[2,2] + m1[3,0]*m2[2,3],
                                m1[0,1]*m2[2,0] + m1[1,1]*m2[2,1] + m1[2,1]*m2[2,2] + m1[3,1]*m2[2,3],
                                m1[0,2]*m2[2,0] + m1[1,2]*m2[2,1] + m1[2,2]*m2[2,2] + m1[3,2]*m2[2,3],
                                m1[0,3]*m2[2,0] + m1[1,3]*m2[2,1] + m1[2,3]*m2[2,2] + m1[3,3]*m2[2,3]
                            },
                            {
                                m1[0,0]*m2[3,0] + m1[1,0]*m2[3,1] + m1[2,0]*m2[3,2] + m1[3,0]*m2[3,3],
                                m1[0,1]*m2[3,0] + m1[1,1]*m2[3,1] + m1[2,1]*m2[3,2] + m1[3,1]*m2[3,3],
                                m1[0,2]*m2[3,0] + m1[1,2]*m2[3,1] + m1[2,2]*m2[3,2] + m1[3,2]*m2[3,3],
                                m1[0,3]*m2[3,0] + m1[1,3]*m2[3,1] + m1[2,3]*m2[3,2] + m1[3,3]*m2[3,3]
                            },
                        }
                    )
            );
        }

        public static Matrix operator /(Matrix m1, double s2)
        {
            return
             (
                 new Matrix
                 (
                     new double[,] 
                    {
                        {m1[0,0]/s2, m1[0,1]/s2, m1[0,2]/s2, m1[0,3]/s2},
                        {m1[1,0]/s2, m1[1,1]/s2, m1[1,2]/s2, m1[1,3]/s2},
                        {m1[2,0]/s2, m1[2,1]/s2, m1[2,2]/s2, m1[2,3]/s2},
                        {m1[3,0]/s2, m1[3,1]/s2, m1[3,2]/s2, m1[3,3]/s2}
                    }
                 )
             );
        }

        public static bool operator ==(Matrix m1, Matrix m2) { return m1.Equals(m2); }

        public static bool operator !=(Matrix m1, Matrix m2) { return ! m1.Equals(m2); }

        public static Matrix operator -(Matrix m1)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {-m1[0,0], -m1[0,1], -m1[0,2], -m1[0,3]},
                        {-m1[1,0], -m1[1,1], -m1[1,2], -m1[1,3]},
                        {-m1[2,0], -m1[2,1], -m1[2,2], -m1[2,3]},
                        {-m1[3,0], -m1[3,1], -m1[3,2], -m1[3,3]}
                    }
                )
            );
        }

        public static Matrix operator +(Matrix m1)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {+m1[0,0], +m1[0,1], +m1[0,2], +m1[0,3]},
                        {+m1[1,0], +m1[1,1], +m1[1,2], +m1[1,3]},
                        {+m1[2,0], +m1[2,1], +m1[2,2], +m1[2,3]},
                        {+m1[3,0], +m1[3,1], +m1[3,2], +m1[3,3]}
                    }
                )
            );
        }

        #endregion

        #region Standard Functions

        public bool Equals(Matrix other)
        {
            return
                _vals[0, 0] == other[0, 0] &&
                _vals[0, 1] == other[0, 1] &&
                _vals[0, 2] == other[0, 2] &&
                _vals[0, 3] == other[0, 3] &&

                _vals[1, 0] == other[1, 0] &&
                _vals[1, 1] == other[1, 1] &&
                _vals[1, 2] == other[1, 2] &&
                _vals[1, 3] == other[1, 3] &&

                _vals[2, 0] == other[2, 0] &&
                _vals[2, 1] == other[2, 1] &&
                _vals[2, 2] == other[2, 2] &&
                _vals[2, 3] == other[2, 3] &&
                _vals[3, 0] == other[3, 0] &&
                _vals[3, 1] == other[3, 1] &&
                _vals[3, 2] == other[3, 2] &&
                _vals[3, 3] == other[3, 3];
        }

        public bool Equals(Matrix other, double tolerance)
        {
            return
                System.Math.Abs(_vals[0, 0] - other[0, 0]) <= tolerance &&
                System.Math.Abs(_vals[0, 1] - other[0, 1]) <= tolerance &&
                System.Math.Abs(_vals[0, 2] - other[0, 2]) <= tolerance &&
                System.Math.Abs(_vals[0, 3] - other[0, 3]) <= tolerance &&
                                 
                System.Math.Abs(_vals[1, 0] - other[1, 0]) <= tolerance &&
                System.Math.Abs(_vals[1, 1] - other[1, 1]) <= tolerance &&
                System.Math.Abs(_vals[1, 2] - other[1, 2]) <= tolerance &&
                System.Math.Abs(_vals[1, 3] - other[1, 3]) <= tolerance &&
                                
                System.Math.Abs(_vals[2, 0] - other[2, 0]) <= tolerance &&
                System.Math.Abs(_vals[2, 1] - other[2, 1]) <= tolerance &&
                System.Math.Abs(_vals[2, 2] - other[2, 2]) <= tolerance &&
                System.Math.Abs(_vals[2, 3] - other[2, 3]) <= tolerance &&
                System.Math.Abs(_vals[3, 0] - other[3, 0]) <= tolerance &&
                System.Math.Abs(_vals[3, 1] - other[3, 1]) <= tolerance &&
                System.Math.Abs(_vals[3, 2] - other[3, 2]) <= tolerance &&
                System.Math.Abs(_vals[3, 3] - other[3, 3]) <= tolerance;
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix)
            {
                Matrix matrix = (Matrix)obj;
                return matrix.Equals(this);
            }
            else return false;
        }

        public bool Equals(object obj, double tolerance)
        {
            if (obj is Matrix)
            {
                Matrix matrix = (Matrix)obj;
                return matrix.Equals(this, tolerance);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return
            (
                (int)(SumComponents() % Int32.MaxValue)
            );
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            // If no format is passed
            if (format == null || format == "")
                return String.Format
                (
                    "[ [{0}, {1}, {2}, {3}], [{4}, {5}, {6}, {7}], [{8}, {9}, {10}, {11}], [{12}, {13}, {14}, {15}] ]",
                    _vals[0, 0], _vals[0, 1], _vals[0, 2], _vals[0, 3],
                    _vals[1, 0], _vals[1, 1], _vals[1, 2], _vals[1, 3],
                    _vals[2, 0], _vals[2, 1], _vals[2, 2], _vals[2, 3],
                    _vals[3, 0], _vals[3, 1], _vals[3, 2], _vals[3, 3]
                );

            char firstChar = Char.ToLower(format[0]);
            string remainder = null;

            if (format.Length > 1)
                remainder = format.Substring(1);

            switch (firstChar)
            {
                case 'm':
                    if (remainder != null && Char.ToLower(remainder[0]) == '3')
                    {
                        if (format.Length > 2)
                        remainder = format.Substring(2);
                        return String.Format
                        (
                            "[{0}, {1}, {2}]\n[{4}, {5}, {6}]\n[{8}, {9}, {10}]",
                            _vals[0, 0].ToString(remainder, formatProvider), 
                            _vals[0, 1].ToString(remainder, formatProvider), 
                            _vals[0, 2].ToString(remainder, formatProvider), 

                            _vals[1, 0].ToString(remainder, formatProvider),
                            _vals[1, 1].ToString(remainder, formatProvider),
                            _vals[1, 2].ToString(remainder, formatProvider),

                            _vals[2, 0].ToString(remainder, formatProvider),
                            _vals[2, 1].ToString(remainder, formatProvider),
                            _vals[2, 2].ToString(remainder, formatProvider)
                        );
                    }
                    else return String.Format
                    (
                        "[{0}, {1}, {2}, {3}]\n[{4}, {5}, {6}, {7}]\n[{8}, {9}, {10}, {11}]\n[{12}, {13}, {14}, {15}]",
                        _vals[0, 0].ToString(remainder, formatProvider), 
                        _vals[0, 1].ToString(remainder, formatProvider), 
                        _vals[0, 2].ToString(remainder, formatProvider), 
                        _vals[0, 3].ToString(remainder, formatProvider),

                        _vals[1, 0].ToString(remainder, formatProvider),
                        _vals[1, 1].ToString(remainder, formatProvider),
                        _vals[1, 2].ToString(remainder, formatProvider),
                        _vals[1, 3].ToString(remainder, formatProvider),

                        _vals[2, 0].ToString(remainder, formatProvider),
                        _vals[2, 1].ToString(remainder, formatProvider),
                        _vals[2, 2].ToString(remainder, formatProvider),
                        _vals[2, 3].ToString(remainder, formatProvider),

                        _vals[3, 0].ToString(remainder, formatProvider),
                        _vals[3, 1].ToString(remainder, formatProvider),
                        _vals[3, 2].ToString(remainder, formatProvider),
                        _vals[3, 3].ToString(remainder, formatProvider)
                    );
                case '3':
                    if (remainder != null && Char.ToLower(remainder[0]) == 'm')
                    {
                        remainder = "3"; goto case 'm';
                    }
                    else
                        return String.Format
                        (
                            "[ [{0}, {1}, {2}], [{4}, {5}, {6}], [{8}, {9}, {10}], [{12}, {13}, {14}] ]",
                            _vals[0, 0].ToString(remainder, formatProvider),
                            _vals[0, 1].ToString(remainder, formatProvider),
                            _vals[0, 2].ToString(remainder, formatProvider),

                            _vals[1, 0].ToString(remainder, formatProvider),
                            _vals[1, 1].ToString(remainder, formatProvider),
                            _vals[1, 2].ToString(remainder, formatProvider),

                            _vals[2, 0].ToString(remainder, formatProvider),
                            _vals[2, 1].ToString(remainder, formatProvider),
                            _vals[2, 2].ToString(remainder, formatProvider)
                        );
                case 'p':
                    return String.Format
                        (
                            "[{0}, {1}, {2}]",
                            _vals[0, 0].ToString(remainder, formatProvider),
                            _vals[1, 1].ToString(remainder, formatProvider),
                            _vals[2, 2].ToString(remainder, formatProvider)
                        );
                case 'v':
                    return String.Format
                    (
                        "{16}\n"+
                        "[a1_1(index=[0,0] position=0 )={0},  a1_2(index=[0,1] position=1 )={1},  a1_3(index=[0,2] position=2 )={2},  a1_4(index=[0,3] position=3 )={3}]\n"+
                        "[a2_1(index=[1,0] position=4 )={4},  a2_2(index=[1,1] position=5 )={5},  a2_3(index=[1,2] position=6 )={6},  a2_4(index=[1,3] position=7 )={7}]\n" +
                        "[a3_1(index=[2,0] position=8 )={8},  a3_2(index=[2,1] position=9 )={9},  a3_3(index=[2,2] position=10)={10}, a3_4(index=[2,3] position=11)={11}]\n" +
                        "[a4_1(index=[3,0] position=12)={12}, a4_2(index=[3,1] position=13)={13}, a4_3(index=[3,2] position=14)={14}, a4_4(index=[3,3] position=15)={15}]",
                       
                        _vals[0, 0].ToString(remainder, formatProvider),
                        _vals[0, 1].ToString(remainder, formatProvider),
                        _vals[0, 2].ToString(remainder, formatProvider),
                        _vals[0, 3].ToString(remainder, formatProvider),

                        _vals[1, 0].ToString(remainder, formatProvider),
                        _vals[1, 1].ToString(remainder, formatProvider),
                        _vals[1, 2].ToString(remainder, formatProvider),
                        _vals[1, 3].ToString(remainder, formatProvider),

                        _vals[2, 0].ToString(remainder, formatProvider),
                        _vals[2, 1].ToString(remainder, formatProvider),
                        _vals[2, 2].ToString(remainder, formatProvider),
                        _vals[2, 3].ToString(remainder, formatProvider),

                        _vals[3, 0].ToString(remainder, formatProvider),
                        _vals[3, 1].ToString(remainder, formatProvider),
                        _vals[3, 2].ToString(remainder, formatProvider),
                        _vals[3, 3].ToString(remainder, formatProvider),

                        ( IsIdentity ? "Identity matrix: " : IsZero ? "Zero matrix: " : IsTranslationMatrix ? "Translation matrix: " : IsScalingMatrix ? "Scaling matrix: " : "Matrix:")
                    );
                default:
                    return String.Format
                (
                    "[ [{0}, {1}, {2}, {3}], [{4}, {5}, {6}, {7}], [{8}, {9}, {10}, {11}], [{12}, {13}, {14}, {15}] ]",
                    _vals[0, 0].ToString(remainder, formatProvider), 
                        _vals[0, 1].ToString(remainder, formatProvider), 
                        _vals[0, 2].ToString(remainder, formatProvider), 
                        _vals[0, 3].ToString(remainder, formatProvider),

                        _vals[1, 0].ToString(remainder, formatProvider),
                        _vals[1, 1].ToString(remainder, formatProvider),
                        _vals[1, 2].ToString(remainder, formatProvider),
                        _vals[1, 3].ToString(remainder, formatProvider),

                        _vals[2, 0].ToString(remainder, formatProvider),
                        _vals[2, 1].ToString(remainder, formatProvider),
                        _vals[2, 2].ToString(remainder, formatProvider),
                        _vals[2, 3].ToString(remainder, formatProvider),

                        _vals[3, 0].ToString(remainder, formatProvider),
                        _vals[3, 1].ToString(remainder, formatProvider),
                        _vals[3, 2].ToString(remainder, formatProvider),
                        _vals[3, 3].ToString(remainder, formatProvider)
                );
            }
        }

        #endregion

        #region Functions

        public double Determinant
        {
            get
            {
                // 4! permutations for a 4x4 matrix i.e. 24 permutations
                return
                _vals[0, 3] * _vals[1, 2] * _vals[2, 1] * _vals[3, 0] -
                _vals[0, 2] * _vals[1, 3] * _vals[2, 1] * _vals[3, 0] -
                _vals[0, 3] * _vals[1, 1] * _vals[2, 2] * _vals[3, 0] +
                _vals[0, 1] * _vals[1, 3] * _vals[2, 2] * _vals[3, 0] +
                _vals[0, 2] * _vals[1, 1] * _vals[2, 3] * _vals[3, 0] -
                _vals[0, 1] * _vals[1, 2] * _vals[2, 3] * _vals[3, 0] -
                _vals[0, 3] * _vals[1, 2] * _vals[2, 0] * _vals[3, 1] +
                _vals[0, 2] * _vals[1, 3] * _vals[2, 0] * _vals[3, 1] +
                _vals[0, 3] * _vals[1, 0] * _vals[2, 2] * _vals[3, 1] -
                _vals[0, 0] * _vals[1, 3] * _vals[2, 2] * _vals[3, 1] -
                _vals[0, 2] * _vals[1, 0] * _vals[2, 3] * _vals[3, 1] +
                _vals[0, 0] * _vals[1, 2] * _vals[2, 3] * _vals[3, 1] +
                _vals[0, 3] * _vals[1, 1] * _vals[2, 0] * _vals[3, 2] -
                _vals[0, 1] * _vals[1, 3] * _vals[2, 0] * _vals[3, 2] -
                _vals[0, 3] * _vals[1, 0] * _vals[2, 1] * _vals[3, 2] +
                _vals[0, 0] * _vals[1, 3] * _vals[2, 1] * _vals[3, 2] +
                _vals[0, 1] * _vals[1, 0] * _vals[2, 3] * _vals[3, 2] -
                _vals[0, 0] * _vals[1, 1] * _vals[2, 3] * _vals[3, 2] -
                _vals[0, 2] * _vals[1, 1] * _vals[2, 0] * _vals[3, 3] +
                _vals[0, 1] * _vals[1, 2] * _vals[2, 0] * _vals[3, 3] +
                _vals[0, 2] * _vals[1, 0] * _vals[2, 1] * _vals[3, 3] -
                _vals[0, 0] * _vals[1, 2] * _vals[2, 1] * _vals[3, 3] -
                _vals[0, 1] * _vals[1, 0] * _vals[2, 2] * _vals[3, 3] +
                _vals[0, 0] * _vals[1, 1] * _vals[2, 2] * _vals[3, 3];
            }
        }

        public Matrix Transpose()
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {_vals[0,0], _vals[1,0], _vals[2,0], _vals[3,0]},
                        {_vals[0,1], _vals[1,1], _vals[2,1], _vals[3,1]},
                        {_vals[0,2], _vals[1,2], _vals[2,2], _vals[3,2]},
                        {_vals[0,3], _vals[1,3], _vals[2,3], _vals[3,3]}
                    }
                )
            );
        }

        public static Matrix TranslationMatrix(Vector v1) { return TranslationMatrix(v1.X, v1.Y, v1.Z); }

        public static Matrix TranslationMatrix(double[] xyz) 
        {
            if (xyz.Length != 3) throw new ArgumentOutOfRangeException("xyz", THREE_COMPONENTS);
            return TranslationMatrix(xyz[0], xyz[1], xyz[2]); 
        }

        public static Matrix TranslationMatrix(double x, double y, double z)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {1, 0, 0, x},
                        {0, 1, 0, y},
                        {0, 0, 1, z},
                        {0, 0, 0, 1}
                    }
                )
            );
        }

        public static Matrix ScalingMatrix(double[] xyz)
        {
            if (xyz.Length != 3) throw new ArgumentOutOfRangeException("xyz", THREE_COMPONENTS);
            return TranslationMatrix(xyz[0], xyz[1], xyz[2]);
        }

        public static Matrix ScalingMatrix(double x, double y, double z)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {x, 0, 0, 0},
                        {0, y, 0, 0},
                        {0, 0, z, 0},
                        {0, 0, 0, 1}
                    }
                )
            );
        }

        // Eulier rotation about _x axis
        public static Matrix RotationMatrixAboutXAxis(Angle ax)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {1, 0       , 0        , 0},
                        {0, ax.Cos(), -ax.Sin(), 0},
                        {0, ax.Sin(), ax.Cos() , 0},
                        {0, 0       , 0        , 1}
                    }
                )
            );
        }

        // Eulier rotation about _y axis
        public static Matrix RotationMatrixAboutYAxis(Angle ay)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {ay.Cos() , 0, ay.Sin(), 0},
                        {0        , 1, 0       , 0},
                        {-ay.Sin(), 0, ay.Cos(), 0},
                        {0        , 0, 0       , 1}
                    }
                )
            );
        }

        // Eulier rotation about _z axis
        public static Matrix RotationMatrixAboutZAxis(Angle az)
        {
            return
            (
                new Matrix
                (
                    new double[,] 
                    {
                        {az.Cos(), -az.Sin(), 0, 0},
                        {az.Sin(), az.Cos() , 0, 0},
                        {0       , 0        , 1, 0},
                        {0       , 0        , 0, 1}
                    }
                )
            );
        }

        #endregion

        #region Decisions

        public bool IsIdentity  { get { return Equals(Identity); } }
        public bool IsZero      { get { return Equals(Zero); } }

        public bool IsTranslationMatrix
        {
            get
            {
                return
                _vals[0, 0] == 1 &&
                _vals[0, 1] == 0 &&
                _vals[0, 2] == 0 &&
                // placeholder

                _vals[1, 0] == 0 &&
                _vals[1, 1] == 1 &&
                _vals[1, 2] == 0 &&
                // placeholder

                _vals[2, 0] == 0 &&
                _vals[2, 1] == 0 &&
                _vals[2, 2] == 1 &&
                // placeholder

                _vals[3, 0] == 0 &&
                _vals[3, 1] == 0 &&
                _vals[3, 2] == 0 &&
                _vals[3, 3] == 1;
            }
        }

        public bool IsScalingMatrix
        {
            get 
            {
                return
                // placeholder
                _vals[0, 1] == 0 &&
                _vals[0, 2] == 0 &&
                _vals[0, 3] == 0 &&

                _vals[1, 0] == 0 &&
                // placeholder
                _vals[1, 2] == 0 &&
                _vals[1, 3] == 0 &&

                _vals[2, 0] == 0 &&
                _vals[2, 1] == 0 &&
                // placeholder
                _vals[2, 3] == 0 &&

                _vals[3, 0] == 0 &&
                _vals[3, 1] == 0 &&
                _vals[3, 2] == 0 &&
                _vals[3, 3] == 1;
            }
        }

        #endregion

        #region Component Functions

        public double SumComponents()
        {
            double result = 0;
            foreach (double d in _vals) { result += d; }
            return result;
        }

        #endregion
    }
}
