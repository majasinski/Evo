using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Infrastructure.Framework.Commons;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for definition of chromosome (candidate solution) patterns.
    /// Physical solutions are created by passing the pattern to the <see cref="Populations"/> class.
    /// </summary>
    public static class Chromosomes
    {
        private static readonly int defaultFractionDigits = 3;

        /// <summary>Creates a pattern of a binary candidate solution.</summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <param name="totalBits">Number of bits for the solution or, in case of multi-variable problem, a list of bits numbers.</param>
        /// <param name="fractionDigits">Number of fraction digits of the solution or, in case of multi-variable problem, a list of fraction digit numbers.</param>
        /// <returns name="chromosome">A pattern of a binary candidate solution. Minimum allowable number of bits and default value of 3 fraction digits is used.</returns>
        [NodeCategory("Create")]
        public static BinaryChromosome CreateBinaryChromosome(IList<double> minValue, IList<double> maxValue)
        {
            if (minValue.Count != maxValue.Count)
            {
                throw new Exception("List dimensions are not equal.");
            }
            List<int> fractionDigits = Enumerable.Repeat(defaultFractionDigits, minValue.Count).ToList();

            return CreateBinaryChromosome(minValue, maxValue, new List<int>(), fractionDigits);
        }

        /// <summary>Creates a pattern of a binary candidate solution.</summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <param name="totalBits">Number of bits for the solution or, in case of multi-variable problem, a list of bits numbers.</param>
        /// <param name="fractionDigits">Number of fraction digits of the solution or, in case of multi-variable problem, a list of fraction digit numbers.</param>
        /// <returns name="chromosome">A pattern of a binary candidate solution. Minimum allowable number of bits is used.</returns>
        [NodeCategory("Create")]
        public static BinaryChromosome CreateBinaryChromosome(IList<double> minValue, IList<double> maxValue, IList<int> fractionDigits)
        {
            if (minValue.Count != maxValue.Count)
            {
                throw new Exception("List dimensions are not equal.");
            }
            if (fractionDigits.Any())
            {
                if (minValue.Count() != fractionDigits.Count())
                {
                    throw new Exception("List dimensions are not equal.");
                }
            }
            return CreateBinaryChromosome(minValue, maxValue, new List<int>(), fractionDigits);
        }

        /// <summary>Creates a pattern of a binary candidate solution.</summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <param name="totalBits">Number of bits for the solution or, in case of multi-variable problem, a list of bits numbers.</param>
        /// <param name="fractionDigits">Number of fraction digits of the solution or, in case of multi-variable problem, a list of fraction digit numbers.</param>
        /// <returns name="chromosome">A pattern of a binary candidate solution.</returns>
        [NodeCategory("Create")]
        public static BinaryChromosome CreateBinaryChromosome(IList<double> minValue, IList<double> maxValue, IList<int> totalBits, IList<int> fractionDigits)
        {
            bool computeBits = false;
            if (minValue.Count() != maxValue.Count())
            {
                throw new Exception("List dimensions are not equal.");
            }
            if (totalBits.Any())
            {
                if (minValue.Count() != totalBits.Count())
                {
                    throw new Exception("List dimensions are not equal.");
                }
                if (totalBits.Max() > 64)
                {
                    throw new Exception("Maximum number of beats per chromosome component is 64.");
                }
            }
            else
            {
                computeBits = true;
            }
            if (fractionDigits.Any())
            {
                if (minValue.Count() != fractionDigits.Count())
                {
                    throw new Exception("List dimensions are not equal.");
                }
            }

            double[] genes = new double[minValue.Count()];
            var rnd = RandomizationProvider.Current;

            int maxBits = 0;
            for (int value = 0; value < genes.Length; value++)
            {
                genes[value] = Math.Round(rnd.GetDouble(minValue[value], maxValue[value]), fractionDigits[value]);
                if (computeBits)
                {
                    int bits = 0;

                    bits = BinaryStringRepresentation.ToRepresentation(
                        (fractionDigits[value] > 0 ? Math.Pow(10, -fractionDigits[value]) : 0) + genes[value], 0, fractionDigits[value]).Length;
                    if (bits > maxBits)
                    {
                        maxBits = bits;
                    }
                    bits = BinaryStringRepresentation.ToRepresentation(
                        (fractionDigits[value] > 0 ? Math.Pow(10, -fractionDigits[value]) : 0) + minValue[value], 0, fractionDigits[value]).Length;
                    if (bits > maxBits)
                    {
                        maxBits = bits;
                    }
                    bits = BinaryStringRepresentation.ToRepresentation(
                        (fractionDigits[value] > 0 ? Math.Pow(10, -fractionDigits[value]) : 0) + maxValue[value], 0, fractionDigits[value]).Length;
                    if (bits > maxBits)
                    {
                        maxBits = bits;
                    }
                }
            }
            if (computeBits)
            {
                totalBits = Enumerable.Repeat(maxBits, minValue.Count()).ToList();
            }

            return new BinaryChromosome(
                minValue.ToArray(),
                maxValue.ToArray(),
                totalBits.ToArray(),
                fractionDigits.ToArray(),
                genes
            );
        }

        /// <summary>Creates a pattern of a double (floating point) candidate solution.</summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <returns name="chromosome">A pattern of a double (floating point) candidate solution.</returns>
        [NodeCategory("Create")]
        public static DoubleChromosome CreateDoubleChromosome(IList<double> minValue, IList<double> maxValue)
        {
            return new DoubleChromosome(minValue.ToArray(), maxValue.ToArray());
        }

        /// <summary>Creates a pattern of a combinatorial candidate solution.</summary>
        /// <param name="numberOfElements">Number of solution components.</param>
        /// <returns name="chromosome">A pattern of a combinatorial candidate solution.</returns>
        [NodeCategory("Create")]
        public static CombinatorialChromosome CreateCombinatorialChromosome(int numberOfElements)
        {
            return new CombinatorialChromosome(numberOfElements);
        }

        /// <summary>Decodes encoded chromosome.</summary>
        /// <param name="chromosome">An encoded chromosome.</param>
        /// <returns name="decoded">Numerical values in an encoded chromosome.</returns>
        [NodeCategory("Action")]
        public static object DecodeChromosome(object chromosome)
        {
            if (chromosome is BinaryChromosome)
            {
                return ((BinaryChromosome)chromosome).ToFloatingPoints().ToList();
            }
            if (chromosome is CombinatorialChromosome)
            {
                List<int> geneValues = new List<int>();
                foreach (Gene gene in ((CombinatorialChromosome)chromosome).GetGenes())
                {
                    geneValues.Add((int)gene.Value);
                }
                return geneValues;
            }
            if (chromosome is DoubleChromosome)
            {
                List<double> geneValues = new List<double>();
                foreach (Gene gene in ((DoubleChromosome)chromosome).GetGenes())
                {
                    geneValues.Add((double)gene.Value);
                }
                return geneValues;
            }
            return null;
        }

        /// <summary>Lists chromosome elements (genes).</summary>
        /// <param name="chromosome">A chromosome or a list of chromosomes to process.</param>
        /// <returns name="genes">A list of chromosome elements (genes).</returns>
        [NodeCategory("Action")]
        public static object GetGenes(object chromosome)
        {
            List<object> genes = new List<object>();
            if (chromosome is BinaryChromosome)
            {
                foreach (Gene gene in ((BinaryChromosome)chromosome).GetGenes())
                {
                    genes.Add((int)gene.Value);
                }
                return genes;
            }
            if (chromosome is CombinatorialChromosome)
            {
                foreach (Gene gene in ((CombinatorialChromosome)chromosome).GetGenes())
                {
                    genes.Add((int)gene.Value);
                }
                return genes;
            }
            if (chromosome is DoubleChromosome)
            {
                foreach (Gene gene in ((DoubleChromosome)chromosome).GetGenes())
                {
                    genes.Add((double)gene.Value);
                }
                return genes;
            }
            return null;
        }

        /// <summary>Returns fitness for an encoded chromosome. For decoded chromosomes, please apply fitness function directly.</summary>
        /// <param name="chromosome">An encoded chromosome.</param>
        /// <returns name="fitness">Fitness function value.</returns>
        [NodeCategory("Query")]
        public static double? Fitness(object chromosome)
        {
            if (chromosome is BinaryChromosome)
            {
                return ((BinaryChromosome)chromosome).Fitness;
            }
            if (chromosome is CombinatorialChromosome)
            {
                return ((CombinatorialChromosome)chromosome).Fitness;
            }
            if (chromosome is DoubleChromosome)
            {
                return ((DoubleChromosome)chromosome).Fitness;
            }
            return null;
        }
    }
}