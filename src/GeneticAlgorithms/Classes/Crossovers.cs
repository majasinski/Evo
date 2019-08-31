using System;
using System.Collections;
using System.Collections.Generic;

using Evo;
using GeneticSharp.Domain.Crossovers;

using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for definition of crossover classes.
    /// Crossover, besides selection and mutation, is one of the main operators in genetic algorithms.
    /// </summary>
    public static class Crossovers
    {
        /// <summary>Defines a crossover instance: Alternating-position Crossover (AP). Designed for combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Alternating-point Crossover (AP).</returns>
        [NodeCategory("Create")]
        public static AlternatingPositionCrossover AlternatingPositionCrossover()
        {
            return new AlternatingPositionCrossover();
        }

        /// <summary>Defines a crossover instance: Cut and Splice Crossover. Designed for binary and combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Cut and Splice Crossover.</returns>
        [NodeCategory("Create")]
        public static CutAndSpliceCrossover CutAndSpliceCrossover()
        {
            return new CutAndSpliceCrossover();
        }

        /// <summary>Defines a crossover instance: Cycle Crossover (CX). Designed for combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Cycle Crossover (CX).</returns>
        [NodeCategory("Create")]
        public static CycleCrossover CycleCrossover()
        {
            return new CycleCrossover();
        }

        /// <summary>Defines a crossover instance: One-point Crossover. Designed for binary and combinatorial chromosomes.</summary>
        /// <param name="swapPoint">Fixed cutting point along the chromosome length.</param>
        /// <returns name="crossover">A crossover instance: Once-point Crossover.</returns>
        [NodeCategory("Create")]
        public static OnePointCrossover OnePointCrossover(int swapPoint)
        {
            if (swapPoint < 1)
            {
                throw new CrossoverException("Invalid swap point for one-point crossover.");
            }
            return new OnePointCrossover(swapPoint - 1);
        }

        /// <summary>Defines a crossover instance: Order Based Crossover (OX2). Designed for combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Order Based Crossover (OX2).</returns>
        [NodeCategory("Create")]
        public static OrderBasedCrossover OrderBasedCrossover()
        {
            return new OrderBasedCrossover();
        }

        /// <summary>Defines a crossover instance: Ordered Crossover (OX1). Designed for combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Ordered Crossover (OX1).</returns>
        [NodeCategory("Create")]
        public static OrderedCrossover OrderedCrossover()
        {
            return new OrderedCrossover();
        }

        /// <summary>Defines a crossover instance: Partially-mapped Crossover (PMX). Designed for combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Partially-mapped Crossover (PMX).</returns>
        [NodeCategory("Create")]
        public static PartiallyMappedCrossover PartiallyMappedCrossover()
        {
            return new PartiallyMappedCrossover();
        }

        /// <summary>Defines a crossover instance: Position Based Crossover (POS). Designed for combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Position Based Crossover (POS).</returns>
        [NodeCategory("Create")]
        public static PositionBasedCrossover PositionBasedCrossover()
        {
            return new PositionBasedCrossover();
        }

        /// <summary>Defines a crossover instance: Simulated Binary Crossover (SBX). Designed for double chromosomes.</summary>
        /// <param name="distributionIndex">A non-negative value defining dispersion rate of offsprings in relation to parents.</param>
        /// <returns name="crossover">A crossover instance: Simulated Binary Crossover (SBX).</returns>
        [NodeCategory("Create")]
        public static SimulatedBinaryCrossover SimulatedBinaryCrossover(double distributionIndex = 1)
        {
            return new SimulatedBinaryCrossover(distributionIndex);
        }

        /// <summary>Defines a crossover instance: Three Parent Crossover. Designed for binary and combinatorial chromosomes.</summary>
        /// <returns name="crossover">A crossover instance: Three Parent Crossover.</returns>
        [NodeCategory("Create")]
        public static ThreeParentCrossover ThreeParentCrossover()
        {
            return new ThreeParentCrossover();
        }

        /// <summary>Defines a crossover instance: Two-point Crossover. Designed for binary and combinatorial chromosomes.</summary>
        /// <param name="swapStartPoint">Fixed initial cutting point along the chromosome length.</param>
        /// <param name="swapEndPoint">Fixed end cutting point along the chromosome length.</param>
        /// <returns name="crossover">A crossover instance: Two-point Crossover.</returns>
        [NodeCategory("Create")]
        public static TwoPointCrossover TwoPointCrossover(int swapStartPoint, int swapEndPoint)
        {
            if (swapStartPoint < 1)
            {
                throw new CrossoverException("Invalid initial swap point for two-point crossover.");
            }
            if (swapEndPoint < 1)
            {
                throw new CrossoverException("Invalid end swap point for two-point crossover.");
            }
            if (swapEndPoint <= swapStartPoint)
            {
                throw new CrossoverException("Initial swap point cannot be greater or equal to the end swap point.");
            }
            return new TwoPointCrossover(swapStartPoint - 1, swapEndPoint - 1);
        }

        /// <summary>Defines a crossover instance: Uniform Crossover. Designed for combinatorial chromosomes. Designed for binary and combinatorial chromosomes.</summary>
        /// <param name="mixProbability">Probability of inheriting from the first parent in a parents' pair.</param>
        /// <returns name="crossover">A crossover instance: Uniform Crossover.</returns>
        [NodeCategory("Create")]
        public static UniformCrossover UniformCrossover(double mixProbability = 0.5)
        {
            if ((mixProbability < 0) || (mixProbability > 1))
            {
                throw new CrossoverException("Invalid value of inhertitance probability. The given value should be in range between 0.0 and 1.0.");
            }
            return new UniformCrossover((float) mixProbability);
        }

        /// <summary>Defines a crossover instance: Voting Recombination Crossover (VR). Designed for binary and combinatorial chromosomes.</summary>
        /// <param name="parentsNumber">Number of parents selected for a single crossover process.</param>
        /// <param name="threshold">Minimal number of gene occurrences to inherit by the offspring.</param>
        /// <returns name="crossover">A crossover instance: Voting Recombination Crossover (VR).</returns>
        [NodeCategory("Create")]
        public static VotingRecombinationCrossover VotingRecombinationCrossover(int parentsNumber = 4, int threshold = 3)
        {
            if (parentsNumber < 2)
            {
                throw new CrossoverException("Invalid number of parents. The given value cannot be lower than 2.");
            }
            if (parentsNumber < threshold)
            {
                throw new CrossoverException("Number of parents selected for the crossover cannot be lower than threshold.");
            }
            return new VotingRecombinationCrossover(parentsNumber, threshold);
        }

        /// <summary>Returns type of crossover applied to a genetic algorithm instance.</summary>
        /// <param name="algorithm">A genetic algorithm instance to check for the crossover method.</param>
        /// <returns name="crossover">Type of crossover applied to a genetic algorithm instance.</returns>
        [NodeCategory("Query")]
        public static object GetCrossover(DynamoGeneticAlgorithm algorithm)
        {
            if (algorithm.Crossover is AlternatingPositionCrossover)
            {
                return algorithm.Crossover as AlternatingPositionCrossover;
            }
            if (algorithm.Crossover is CutAndSpliceCrossover)
            {
                return algorithm.Crossover as CutAndSpliceCrossover;
            }
            if (algorithm.Crossover is CycleCrossover)
            {
                return algorithm.Crossover as CycleCrossover;
            }
            if (algorithm.Crossover is OnePointCrossover)
            {
                return algorithm.Crossover as OnePointCrossover;
            }
            if (algorithm.Crossover is OrderedCrossover)
            {
                return algorithm.Crossover as OrderedCrossover;
            }
            if (algorithm.Crossover is OrderBasedCrossover)
            {
                return algorithm.Crossover as OrderBasedCrossover;
            }
            if (algorithm.Crossover is PartiallyMappedCrossover)
            {
                return algorithm.Crossover as PartiallyMappedCrossover;
            }
            if (algorithm.Crossover is PositionBasedCrossover)
            {
                return algorithm.Crossover as PositionBasedCrossover;
            }
            if (algorithm.Crossover is SimulatedBinaryCrossover)
            {
                return algorithm.Crossover as SimulatedBinaryCrossover;
            }
            if (algorithm.Crossover is ThreeParentCrossover)
            {
                return algorithm.Crossover as ThreeParentCrossover;
            }
            if (algorithm.Crossover is TwoPointCrossover)
            {
                return algorithm.Crossover as TwoPointCrossover;
            }
            if (algorithm.Crossover is UniformCrossover)
            {
                return algorithm.Crossover as UniformCrossover;
            }
            if (algorithm.Crossover is VotingRecombinationCrossover)
            {
                return algorithm.Crossover as VotingRecombinationCrossover;
            }
            return null;
        }

        /// <summary>Returns crossover probability for a genetic algorithm instance.</summary>
        /// <param name="algorithm">A genetic algorithm instance to check for crossover probability.</param>
        /// <returns name="crossoverProbability">Crossover probability for a genetic algorithm instance.</returns>
        [NodeCategory("Query")]
        public static double GetCrossoverProbability(DynamoGeneticAlgorithm algorithm)
        {
            return algorithm.CrossoverProbability;
        }
    }
}