using System;
using System.Collections.Generic;
using System.Text;

namespace TAlex.GameOfLife.Engine
{
    public struct LifeRule : IFormattable
    {
        #region Fields

        public const string RLEFormat = "By/Sx";
        public const string LifeFormat = "x/y";

        private const char Separator = '/';
        private const string BirthSign = "B";
        private const string SurvivalSign = "S";

        internal byte[] B;
        internal byte[] S;
        internal int BN;
        internal int SN;

        public static readonly LifeRule EmptyRule;

        public static readonly LifeRule ReplicatorRule;
        public static readonly LifeRule SeedsRule;
        public static readonly LifeRule PersianRugRule;
        public static readonly LifeRule LifeWithoutDeathRule;
        public static readonly LifeRule MazeRule;
        public static readonly LifeRule StandardLifeRule;
        public static readonly LifeRule CoralRule;
        public static readonly LifeRule Life34Rule;
        public static readonly LifeRule AssimilationRule;
        public static readonly LifeRule LongLifeRule;
        public static readonly LifeRule DiamoebaRule;
        public static readonly LifeRule AmoebaRule;
        public static readonly LifeRule PseudoLifeRule;
        public static readonly LifeRule TwoByTwoRule;
        public static readonly LifeRule HighLifeRule;
        public static readonly LifeRule StainsRule;
        public static readonly LifeRule DayAndNightRule;
        public static readonly LifeRule MoveRule;
        public static readonly LifeRule CoagulationsRule;
        public static readonly LifeRule WalledCitiesRule;
        
        #endregion

        #region Constructors

        static LifeRule()
        {
            EmptyRule = new LifeRule( new byte[] {}, new byte[] {} );

            ReplicatorRule = new LifeRule(new byte[] { 1, 3, 5, 7 }, new byte[] { 1, 3, 5, 7 });
            SeedsRule = new LifeRule(new byte[] { 2 }, new byte[] { });
            PersianRugRule = new LifeRule(new byte[] { 2, 3, 4 }, new byte[] { });
            LifeWithoutDeathRule = new LifeRule(new byte[] { 3 }, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
            MazeRule = new LifeRule(new byte[] { 3 }, new byte[] { 1, 2, 3, 4, 5 });
            StandardLifeRule = new LifeRule(new byte[] { 3 }, new byte[] { 2, 3 });
            CoralRule = new LifeRule(new byte[] { 3 }, new byte[] { 4, 5, 6, 7, 8 });
            Life34Rule = new LifeRule(new byte[] { 3, 4 }, new byte[] { 3, 4 });
            AssimilationRule = new LifeRule(new byte[] { 3, 4, 5 }, new byte[] { 4, 5, 6, 7 });
            LongLifeRule = new LifeRule(new byte[] { 3, 4, 5 }, new byte[] { 5 });
            DiamoebaRule = new LifeRule(new byte[] { 3, 5, 6, 7, 8 }, new byte[] { 5, 6, 7, 8 });
            AmoebaRule = new LifeRule(new byte[] { 3, 5, 7 }, new byte[] { 1, 3, 5, 8 });
            PseudoLifeRule = new LifeRule(new byte[] { 3, 5, 7 }, new byte[] { 2, 3, 8 });
            TwoByTwoRule = new LifeRule(new byte[] { 3, 6 }, new byte[] { 1, 2, 5 });
            HighLifeRule = new LifeRule(new byte[] { 3, 6 }, new byte[] { 2, 3 });
            StainsRule = new LifeRule(new byte[] { 3, 6, 7, 8 }, new byte[] { 2, 3, 5, 6, 7, 8 });
            DayAndNightRule = new LifeRule(new byte[] { 3, 6, 7, 8 }, new byte[] { 3, 4, 6, 7, 8 });
            MoveRule = new LifeRule(new byte[] { 3, 6, 8 }, new byte[] { 2, 4, 5 });
            CoagulationsRule = new LifeRule(new byte[] { 3, 7, 8 }, new byte[] { 2, 3, 5, 6, 7, 8 });
            WalledCitiesRule = new LifeRule(new byte[] { 4, 5, 6, 7, 8 }, new byte[] { 2, 3, 4, 5 });
        }

        public LifeRule(byte[] B, byte[] S)
        {
            if (B == null)
                throw new ArgumentNullException();

            List<byte> b_list = new List<byte>();

            for (int i = 0; i < B.Length; i++)
            {
                if (B[i] > 8)
                    throw new ArgumentOutOfRangeException();

                if (b_list.Contains(B[i]) == false)
                    b_list.Add(B[i]);
            }

            b_list.Sort();
            this.BN = b_list.Count;
            this.B = b_list.ToArray();

            if (S == null)
            {
                this.SN = 0;
                this.S = null;
            }
            else
            {
                List<byte> s_list = new List<byte>();

                for (int i = 0; i < S.Length; i++)
                {
                    if (S[i] > 8)
                        throw new ArgumentOutOfRangeException();

                    if (s_list.Contains(S[i]) == false)
                        s_list.Add(S[i]);
                }

                s_list.Sort();
                this.SN = s_list.Count;
                this.S = s_list.ToArray();
            }
        }

        #endregion

        #region Methods

        public static LifeRule Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException();

            s = s.Trim().ToUpper();
            if (s.EndsWith(Separator.ToString()))
            {
                s = s.Substring(0, s.Length - 1);
            }

            if (s.Length == 0)
                throw new ArgumentException();

            byte[] B = null;
            byte[] S = null;

            string[] BSparts = s.Split(Separator);

            if (BSparts.Length == 1)
            {
                string Bpart = BSparts[0];
                if (Bpart.StartsWith(BirthSign))
                    Bpart = Bpart.Substring(BirthSign.Length);

                B = new byte[Bpart.Length];
                for (int i = 0; i < Bpart.Length; i++)
                    B[i] = byte.Parse(Bpart[i].ToString());
            }
            else if (BSparts.Length == 2)
            {
                string Bpart = BSparts[0].TrimEnd();
                string Spart = BSparts[1].TrimStart();

                if ((Bpart.StartsWith(SurvivalSign) && Spart.StartsWith(BirthSign)) ||
                    (!Bpart.StartsWith(BirthSign) && !Spart.StartsWith(SurvivalSign)))
                {
                    string temp = Bpart;
                    Bpart = Spart;
                    Spart = temp;
                }

                if (String.IsNullOrEmpty(Bpart))
                    throw new FormatException();

                if (Bpart.StartsWith(BirthSign))
                    Bpart = Bpart.Substring(BirthSign.Length);
                if (Spart.StartsWith(SurvivalSign))
                    Spart = Spart.Substring(SurvivalSign.Length);

                B = new byte[Bpart.Length];
                for (int i = 0; i < Bpart.Length; i++)
                    B[i] = byte.Parse(Bpart[i].ToString());

                S = new byte[Spart.Length];
                for (int i = 0; i < Spart.Length; i++)
                    S[i] = byte.Parse(Spart[i].ToString());
            }
            else
            {
                throw new FormatException();
            }

            return new LifeRule(B, S);
        }

        public static bool TryParse(string s, out LifeRule rule)
        {
            try
            {
                rule = Parse(s);
                return true;
            }
            catch (FormatException)
            {
                rule = LifeRule.EmptyRule;
                return false;
            }
            catch (ArgumentException)
            {
                rule = LifeRule.EmptyRule;
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is LifeRule)
            {
                return Equals((LifeRule)obj);
            }

            return false;
        }

        public bool Equals(LifeRule rule)
        {
            return ToString() == rule.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return ToString(RLEFormat, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            StringBuilder x = new StringBuilder();

            if (S != null)
            {
                for (int i = 0; i < SN; i++)
                {
                    x.Append(S[i]);
                }
            }

            StringBuilder y = new StringBuilder();

            for (int i = 0; i < BN; i++)
            {
                y.Append(B[i]);
            }

            string result = format;
            result = result.Replace("x", x.ToString());
            result = result.Replace("y", y.ToString());

            if (result.EndsWith("/S"))
            {
                result = result.Remove(result.Length - 2);
            }

            return result;
        }

        #endregion
    }
}
