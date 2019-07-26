using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Infrastructure.Framework.Commons;

using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for definition of chromosome (candidate solution) patterns.
    /// Physical solutions are created by passing a pattern to the <see cref="Populations"/> class.
    /// </summary>
    public static class Chromosomes
    {
        private static readonly int defaultFractionDigits = 3;

        /// <summary>
        /// Creates a pattern of a floating point candidate solution.
        /// </summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <param name="totalBits">Number of bits for the solution or, in case of multi-variable problem, a list of bits numbers.</param>
        /// <param name="fractionDigits">Number of fraction digits of the solution or, in case of multi-variable problem, a list of fraction digit numbers.</param>
        /// <returns name="chromosome">
        /// A pattern of a floating point candidate solution. Default values of total bits of 32 and fraction digits of 3 are used.
        /// </returns>
        [NodeCategory("Create")]
        public static FloatingPointChromosome CreateFloatingPointChromosome(IList<double> minValue, IList<double> maxValue)
        {
            if (minValue.Count != maxValue.Count)
            {
                throw new Exception("List dimensions are not equal.");
            }
            List<int> fractionDigits = Enumerable.Repeat(defaultFractionDigits, minValue.Count).ToList();

            return CreateFloatingPointChromosome(minValue, maxValue, new List<int>(), fractionDigits);
        }

        /// <summary>
        /// Creates a pattern of a floating point candidate solution.
        /// </summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <param name="totalBits">Number of bits for the solution or, in case of multi-variable problem, a list of bits numbers.</param>
        /// <param name="fractionDigits">Number of fraction digits of the solution or, in case of multi-variable problem, a list of fraction digit numbers.</param>
        /// <returns name="chromosome">
        /// A pattern of a floating point candidate solution. Default values of total bits of 32 are used.
        /// </returns>
        [NodeCategory("Create")]
        public static FloatingPointChromosome CreateFloatingPointChromosome(IList<double> minValue, IList<double> maxValue, IList<int> fractionDigits)
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
            return CreateFloatingPointChromosome(minValue, maxValue, new List<int>(), fractionDigits);
        }

        /// <summary>
        /// Creates a pattern of a floating point candidate solution.
        /// </summary>
        /// <param name="minValue">Minimum value of the solution or, in case of multi-variable problem, a list of minimum values.</param>
        /// <param name="maxValue">Maximum value of the solution or, in case of multi-variable problem, a list of maximum values.</param>
        /// <param name="totalBits">Number of bits for the solution or, in case of multi-variable problem, a list of bits numbers.</param>
        /// <param name="fractionDigits">Number of fraction digits of the solution or, in case of multi-variable problem, a list of fraction digit numbers.</param>
        /// <returns name="chromosome">
        /// A pattern of a floating point candidate solution.
        /// </returns>
        [NodeCategory("Create")]
        public static FloatingPointChromosome CreateFloatingPointChromosome(IList<double> minValue, IList<double> maxValue, IList<int> totalBits, IList<int> fractionDigits)
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

            double[] geneValues = new double[minValue.Count()];
            var rnd = RandomizationProvider.Current;

            int maxBits = 0;
            for (int value = 0; value < geneValues.Length; value++)
            {
                geneValues[value] = Math.Round(rnd.GetDouble(minValue[value], maxValue[value]), fractionDigits[value]);
                if (computeBits)
                {
                    int bits = 0;

                    bits = BinaryStringRepresentation.ToRepresentation(
                        (fractionDigits[value] > 0 ? Math.Pow(10, -fractionDigits[value]) : 0) + geneValues[value], 0, fractionDigits[value]).Length;
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

            return new FloatingPointChromosome(
                minValue.ToArray(),
                maxValue.ToArray(),
                totalBits.ToArray(),
                fractionDigits.ToArray(),
                geneValues
            );
        }

        /// <summary>
        /// Creates a pattern of a candidate solution in order-based problem.
        /// </summary>
        /// <param name="numberOfElements">Number of solution components.</param>
        /// <returns name="chromosome">
        /// A pattern of a candidate solution in order-based problem.
        /// </returns>
        [NodeCategory("Create")]
        public static OrderedChromosome CreateOrderedChromosome(int numberOfElements)
        {
            return new OrderedChromosome(numberOfElements);
        }

        /// <summary>
        /// Lists chromosome elements (genes).
        /// </summary>
        /// <param name="chromosomes">A chromosome or a list of chromosomes to process.</param>
        /// <param name="asBits">In case of a floating point chromosome, whether genes should be listed as bits or converted a float point.</param>
        /// <returns name="genes">
        /// A list of chromosome elements (genes).
        /// </returns>
        [NodeCategory("Action")]
        public static object GetGenes(IList chromosomes, bool asBits = false)
        {
            if(!asBits)
            {
                return GetDecodedGenes(chromosomes);
            }

            List<IList> genes = new List<IList>();
            foreach(object chromosome in chromosomes)
            {
                List<int> geneValues = new List<int>();
                if (chromosome is FloatingPointChromosome)
                {
                    foreach (Gene gene in ((FloatingPointChromosome) chromosome).GetGenes())
                    {
                        geneValues.Add((int) gene.Value);
                    }
                }
                else if (chromosome is OrderedChromosome)
                {
                    foreach (Gene gene in ((OrderedChromosome) chromosome).GetGenes())
                    {
                        geneValues.Add((int) gene.Value);
                    }
                }
                genes.Add(geneValues);
            }
            return genes;
        }

        /// <summary>
        /// Lists chromosome elements (genes) or, in case of a floating point chromosome, a floating point value.
        /// </summary>
        /// <param name="chromosomes">A chromosome or a list of chromosomes original representations to convert.</param>
        /// <returns name="values">
        /// A list of chromosome elements (genes) or, in case of a floating point chromosome, a floating point value.
        /// </returns>
        [NodeCategory("Action")]
        public static object GetDecodedGenes(IList chromosomes)
        {
            List<object> chromosomesValues = new List<object>();
            foreach (IChromosome chromosome in chromosomes)
            {
                chromosomesValues.Add(Populations.DecodeChromosome(chromosome));
            }
            return chromosomesValues;
        }
    }
}