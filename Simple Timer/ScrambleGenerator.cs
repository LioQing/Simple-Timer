using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Timer
{
    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }
    }

    static class ScrambleGenerator
    {
        public delegate List<int> GetAdj(int face);

        public static string GetScrambleMoves(string cube)
        {
            switch (cube)
            {
                case "3x3x3":
                    return Get333ScrambleMoves();

                case "2x2x2":
                    return Get222ScrambleMoves();

                case "4x4x4":
                    return Get444ScrambleMoves();

                default:
                    return "No Scramble";
            }
        }

        private static Pair<int, int>[] Scramble(int moveCount, int noFace, int noTurn, GetAdj getAdj)
        {
            var random = new Random();
            var scramble = new Pair<int, int>[moveCount];

            int forbidFace = -1;
            int lastFace = -1;
            var lastAdj = new List<int>();

            for (var i = 0; i < moveCount; ++i)
            {
            restart:

                int face = random.Next(0, noFace);

                if (lastFace == face)
                    goto restart;

                if (forbidFace != -1 && face == forbidFace)
                    goto restart;

                if (!lastAdj.Contains(face))
                    forbidFace = lastFace;
                else
                    forbidFace = -1;

                scramble[i] = new Pair<int, int>() { First = face, Second = random.Next(0, noTurn) };

                lastFace = face;
                lastAdj = getAdj(face);
            }

            return scramble;
        }

        // 3x3x3
        public static string Get333ScrambleMoves()
        {
            string scrambleMove = "";
            foreach (var pair in Scramble(25, 6, 3, Get333Adj))
            {
                var face = pair.First;
                var turn = pair.Second;
                scrambleMove += Get333TurnNotation(face, turn) + " ";
            }
            return scrambleMove;
        }

        private static string Get333TurnNotation(int face, int turn)
        {
            string[] faceNotation = new string[] { "U", "D", "L", "R", "F", "B" };
            string[] turnNotation = new string[] { "", "'", "2" };

            return faceNotation[face] + turnNotation[turn];
        }

        private static List<int> Get333Adj(int face)
        {
            switch (face)
            {
                case 0:
                case 1:
                    return new List<int>() { 2, 3, 4, 5 };

                case 2:
                case 3:
                    return new List<int>() { 0, 1, 4, 5 };

                case 4:
                case 5:
                    return new List<int>() { 0, 1, 2, 3 };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // 2x2x2
        public static string Get222ScrambleMoves()
        {
            string scrambleMove = "";
            foreach (var pair in Scramble(10, 3, 3, Get222Adj))
            {
                var face = pair.First;
                var turn = pair.Second;
                scrambleMove += Get222TurnNotation(face, turn) + " ";
            }
            return scrambleMove;
        }

        private static string Get222TurnNotation(int face, int turn)
        {
            string[] faceNotation = new string[] { "U", "R", "F" };
            string[] turnNotation = new string[] { "", "'", "2" };

            return faceNotation[face] + turnNotation[turn];
        }

        private static List<int> Get222Adj(int face)
        {
            return new List<int>() { };
        }

        // 4x4x4
        public static string Get444ScrambleMoves()
        {
            string scrambleMove = "";
            foreach (var pair in Scramble(40, 9, 3, Get444Adj))
            {
                var face = pair.First;
                var turn = pair.Second;
                scrambleMove += Get444TurnNotation(face, turn) + " ";
            }
            return scrambleMove;
        }

        private static string Get444TurnNotation(int face, int turn)
        {
            string[] faceNotation = new string[] { "U", "D", "L", "R", "F", "B", "Uw", "Rw", "Fw" };
            string[] turnNotation = new string[] { "", "'", "2" };

            return faceNotation[face] + turnNotation[turn];
        }

        private static List<int> Get444Adj(int face)
        {
            switch (face)
            {
                case 0:
                case 1:
                case 6:
                    return new List<int>() { 2, 3, 4, 5, 7, 8 };

                case 2:
                case 3:
                case 7:
                    return new List<int>() { 0, 1, 4, 5, 6, 8 };

                case 4:
                case 5:
                case 8:
                    return new List<int>() { 0, 1, 2, 3, 6, 7 };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
