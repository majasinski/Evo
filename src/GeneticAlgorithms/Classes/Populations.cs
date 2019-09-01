using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for generation of candidate solutions population basing on chromosome patterns.
    /// For chromosome patterns see the <see cref="Chromosomes"/> class.
    /// </summary>
    public static class Populations
    {
        /// <summary>Generates a population of solution candidates basing on a chromosome pattern.</summary>
        /// <param name="size">Size of a population.</param>
        /// <param name="chromosome">A chromosome pattern.</param>
        /// <returns name="population">A population of solution candidates.</returns>
        [NodeCategory("Create")]
        public static Population GeneratePopulation(int size, object chromosome)
        {
            var defaultPerformance = new PerformanceGenerationStrategy(10);
            if (chromosome is BinaryChromosome)
            {
                return GeneratePopulation(size, (BinaryChromosome) chromosome, defaultPerformance);
            }
            if (chromosome is CombinatorialChromosome)
            {
                return GeneratePopulation(size, (CombinatorialChromosome) chromosome, defaultPerformance);
            }
            if (chromosome is DoubleChromosome)
            {
                return GeneratePopulation(size, (DoubleChromosome) chromosome, defaultPerformance);
            }
            return null;
        }

        /// <summary>Generates a population of solution candidates basing on a chromosome pattern.</summary>
        /// <param name="size">Size of a population.</param>
        /// <param name="chromosome">A chromosome pattern.</param>
        /// <param name="strategy">A strategy for a population management: tracking or performance generation strategy.</param>
        /// <returns name="population">A population of solution candidates.</returns>
        [NodeCategory("Create")]
        public static Population GeneratePopulation(int size, object chromosome, object strategy)
        {
            Population population = null;
            if(chromosome is BinaryChromosome)
            {
                population = new Population(size, size, (BinaryChromosome) chromosome);
            }
            else if (chromosome is CombinatorialChromosome)
            {
                population = new Population(size, size, (CombinatorialChromosome) chromosome);
            }
            else if (chromosome is DoubleChromosome)
            {
                population = new Population(size, size, (DoubleChromosome)chromosome);
            }

            if (population == null)
            {
                return null;
            }

            if (strategy is TrackingGenerationStrategy)
            {
                population.GenerationStrategy = (TrackingGenerationStrategy) strategy;
            }
            else
            {
                population.GenerationStrategy = (PerformanceGenerationStrategy) strategy;
            }
            population.CreateInitialGeneration();
            return population;
        }

        /// <summary>Retrieves the most recent generation from a population.</summary>
        /// <param name="population">A population to retrieve most recent generation from.</param>
        /// <returns name="generation">The most recent generation in a population.</returns>
        [NodeCategory("Action")]
        public static Generation GetCurrentGeneration(Population population)
        {
            return population.CurrentGeneration;
        }

        /// <summary>Retrieves an initial generation from a population.</summary>
        /// <param name="population">A population to retrieve an initial generation from.</param>
        /// <returns name="generation">An initial generation in a population.</returns>
        [NodeCategory("Action")]
        public static Generation GetInitialGeneration(Population population)
        {
            if (population.GenerationsNumber > population.Generations.Count())
            {
                throw new Exception("Initial generation data overwritten by population strategy. To see initial generation for the population please use GeneratePopulation node with strategy set to: TrackingGenerationStrategy.");
            }
            return population.Generations.First();
        }

        /// <summary>Retrieves an nth generation from a population.</summary>
        /// <param name="population">A population to retrieve nth generation from.</param>
        /// <param name="generation">A number of a generation to retrieve.</param>
        /// <returns name="generation">An nth generation in a population</returns>
        [NodeCategory("Action")]
        public static Generation GetGeneration(Population population, int generation)
        {
            foreach(Generation gen in population.Generations)
            {
                if(gen.Number == generation)
                {
                    return gen;
                }
            }
            throw new Exception("The given generation does not exist in the population.");
        }

        /// <summary>Retrieves all generations from a population.</summary>
        /// <param name="population">A population to retrieve generations from.</param>
        /// <returns name="generations">A list of all generations in a population.</returns>
        [NodeCategory("Action")]
        public static IList<Generation> GetGenerations(Population population)
        {
            return population.Generations;
        }

        /// <summary>Retrieves all the generations from a population, that have subsequently improved the fittness value.</summary>
        /// <param name="population">A population to retrieve generations from.</param>
        /// <returns name="generations">A list of all the generations in a population, that have subsequently improved the fittness value.</returns>
        [NodeCategory("Action")]
        public static IList<Generation> GetProgressiveGenerations(Population population)
        {
            IList<Generation> generations = new List<Generation>();

            double? bestFitness = double.NegativeInfinity;
            foreach (Generation generation in population.Generations)
            {
                if(generation.BestChromosome == null)
                {
                    continue;
                }

                if(generation.BestChromosome.Fitness > bestFitness)
                {
                    generations.Add(generation);
                    bestFitness = generation.BestChromosome.Fitness;
                }
            }
            return generations;
        }

        /// <summary>Retreives a population processed by a genetic algorithm instance.</summary>
        /// <param name="algorithm">A genetic algorithm instance to retrieve a population from.</param>
        /// <returns name="population">A population processed by a genetic algorithm instance.</returns>
        [NodeCategory("Action")]
        public static Population GetPopulation(DynamoGeneticAlgorithm algorithm)
        {
            return (Population) algorithm.Population;
        }

        /// <summary>Retrieves individuals from a generation.</summary>
        /// <param name="generation">A generation to retrieve individuals from.</param>
        /// <param name="bestChromosomesOnly">Whether only best chromosomes should be listed or all individuals in a generation.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to numerical values.</param>
        /// <returns name="chromosomes">An list of of individuals retrieved from a generation.</returns>
        [NodeCategory("Action")]
        public static object Chromosomes(Generation generation, bool bestChromosomesOnly = false, bool encoded = false)
        {
            if (bestChromosomesOnly)
            {
                return BestChromosome(generation, encoded);
            }

            if (encoded)
            {
                return generation.Chromosomes.ToList();
            }
            IList chromosomes = new IList[generation.Chromosomes.Count()];
            int index = 0;
            foreach (IChromosome chromosome in generation.Chromosomes)
            {
                chromosomes[index] = GeneticAlgorithms.Chromosomes.DecodeChromosome(chromosome);
                index++;
            }
            return chromosomes;
        }

        /// <summary>Retrieves unprocessed individuals from a generation.</summary>
        /// <param name="generation">A generation to retrieve unprocessed individuals from.</param>
        /// <returns name="chromosomes">An list of of unprocessed individuals retrieved from a generation.</returns>
        [NodeCategory("Action")]
        public static object ChromosomesWithNoFitness(Generation generation)
        {
            if (generation.BestChromosome == null)
            {
                List<object> chromosomes = new List<object>();
                foreach(IChromosome chromosome in generation.Chromosomes)
                {
                    chromosomes.Add(chromosome);
                }
                return chromosomes;
            }
            return new List<object>();
        }

        /// <summary>Retrieves the best individual in a generation.</summary>
        /// <param name="generation">A generation to retrieve generations individuals from.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to numerical values.</param>
        /// <returns name="bestChromosome">The best individual in a generation.</returns>
        /// <returns name="fitness">Fitness value for the best chromosome.</returns>
        [NodeCategory("Action")]
        [MultiReturn(new[] { "bestChromosome", "fitness" })]
        public static Dictionary<string, object> BestChromosome(Generation generation, bool encoded = false)
        {
            object chromosome = null;
            if (encoded)
            {
                chromosome = generation.BestChromosome;
            }
            else
            {
                chromosome = GeneticAlgorithms.Chromosomes.DecodeChromosome(generation.BestChromosome);
            }

            double? chromosomeFitness = null;
            if (generation.BestChromosome != null)
            {
                chromosomeFitness = generation.BestChromosome.Fitness;
            }

            return new Dictionary<string, object>
            {
                { "bestChromosome", chromosome },
                { "fitness", chromosomeFitness }
            };
        }

        /// <summary>Returns number of generations in a genetic algorithm instance.</summary>
        /// <param name="algorithm">A genetic algorithm instance.</param>
        /// <returns name="generations">Number of generations in a genetic algorithm instance.</returns>
        [NodeCategory("Query")]
        public static int NumberOfGenerations(DynamoGeneticAlgorithm algorithm)
        {
            return NumberOfGenerations((Population) algorithm.Population);
        }

        /// <summary>Returns number of generations in a population.</summary>
        /// <param name="population">A population.</param>
        /// <returns name="generations">Number of generations in a population.</returns>
        [NodeCategory("Query")]
        public static int NumberOfGenerations(Population population)
        {
            return population.GenerationsNumber;
        }
    }
}