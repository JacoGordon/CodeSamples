using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
public class Dominos
{
    Boolean verbose;

        public Dominos(Boolean v)
        {
            verbose = v;
        }
        public List<int[]> DominoChain(List<int[]> freeDominos, List<int[]> chain, int[] currentDomino, int length, bool flip)
        {
            freeDominos.Remove(currentDomino);
            if (flip) { Array.Reverse(currentDomino); }
            chain.Add(currentDomino);

            // Checks if the chain is full and checks equality of first and last value in chain
            if (freeDominos.Count() == 0 && chain[0][0] == currentDomino[1])
            {
                return chain;
            }
            List<int[]> newChain;
            
            foreach (int[] domino in freeDominos.ToList())
            {
                if(currentDomino[1] == domino[0]){
                    newChain = DominoChain(freeDominos.ToList(), chain, domino, length, false);
                    if (newChain.Count() == length)
                    {
                        return newChain;
                    }
                }
                else if (currentDomino[1] == domino[1]) {
                    newChain = DominoChain(freeDominos.ToList(), chain, domino, length, true);
                    if (newChain.Count() == length)
                    {
                        return newChain;
                    }
                }
            }
            // Could not continue chain with current end domino
            chain.Remove(currentDomino);
            return chain;
        }

        public List<int[]> DominoCheck(List<int[]> dominos)
        {
            int[] currentDomino = dominos[0];
            List<int[]> dominoChain = new List<int[]>();
            dominoChain = DominoChain(dominos.ToList(), dominoChain, currentDomino, dominos.Count(), false);
            if (dominoChain.Count() != dominos.Count())
            {
                throw new InvalidOperationException("Valid domino chain cannot be created");
            }
            else
            {
                StringBuilder textChain = new StringBuilder();
                int[] prevDomino = dominoChain[dominoChain.Count() - 1];
                foreach (int[] domino in dominoChain)
                {
                    if(prevDomino[1] != domino[0])
                    {
                        throw new InvalidOperationException("Valid domino chain cannot be created");
                    }
                    prevDomino = domino;
                    textChain.Append($"[{domino[0].ToString()}:{domino[1].ToString()}]");
                }
                if(verbose)
                {
                    Console.WriteLine("{0}", textChain.ToString());
                }
                return dominoChain;
            }
        }
        static void Main(string[] args)
        {
            Dominos dom = new Dominos(true);
            List<int[]> testdominos = new List<int[]>
            {
                new int[] {1, 2}, new int[] {2, 5}, new int[] {3, 6},
                new int[] {4, 2}, new int[] {3, 5}, new int[] {1, 2},
                new int[] {4, 6}, new int[] {4, 2}, new int[] {3, 1},
                new int[] {1, 5}, new int[] {2, 5}, new int[] {4, 3}
            };
            
            try
            {
                List<int[]> chain = dom.DominoCheck(testdominos);
            }
            catch(InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }