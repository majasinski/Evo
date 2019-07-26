using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        /// Generates a population of solution candidates basing on a chromosome pattern.
        /// </summary>
        /// <param name="size">Size of a population.</param>
        /// <param name="chromosome">A chromosome pattern defining number of variables, and in case of floating point chromosomes: also their minimum and maximum values, total bits and fraction digits.</param>
        /// <returns name="population">
        /// A population of solution candidates.
        /// </returns>
        [NodeCategory("Create")]
        public static Population GeneratePopulation(int size, object chromosome)
        {
            if (chromosome is FloatingPointChromosome)
            {
                return GeneratePopulation(size, (FloatingPointChromosome) chromosome, new PerformanceGenerationStrategy(10));
            }
            if (chromosome is OrderedChromosome)
            {
                return GeneratePopulation(size, (OrderedChromosome) chromosome, new PerformanceGenerationStrategy(10));
            }
            return null;
        }

        /// <summary>
        /// Generates a population of solution candidates basing on a chromosome pattern.
        /// </summary>
        /// <param name="size">Size of a population.</param>
        /// <param name="chromosome">A chromosome pattern defining number of variables, and in case of floating point chromosomes: also their minimum and maximum values, total bits and fraction digits.</param>
        /// <param name="strategy">A strategy for a population management: tracking or performance generation strategy.</param>
        /// <returns name="population">
        /// A population of solution candidates.
        /// </returns>
        [NodeCategory("Create")]
        public static Population GeneratePopulation(int size, object chromosome, object strategy)
        {
            Population population = null;
            if(chromosome is FloatingPointChromosome)
            {
                population = new Population(size, size, (FloatingPointChromosome) chromosome);
            }
            else if (chromosome is OrderedChromosome)
            {
                population = new Population(size, size, (OrderedChromosome) chromosome);
            }

            if(population == null)
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

        /// <summary>
        /// Retrieves individuals of the most recent generation in a population.
        /// </summary>
        /// <param name="population">A population to retrieve most recent generation individuals from.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to lists of components.</param>
        /// <returns name="individuals">
        /// An individuals list of the most recent generation in a population.
        /// </returns>
        [NodeCategory("Action")]
        public static IList GetCurrentGeneration(Population population, bool encoded = false)
        {
            if (encoded)
            {
                return population.CurrentGeneration.Chromosomes.ToList();
            }

            IList generationIndividuals = new IList[population.MaxSize];
            int generationIndividual = 0;
            foreach (IChromosome populationChromosome in population.CurrentGeneration.Chromosomes)
            {
                generationIndividuals[generationIndividual] = DecodeChromosome(populationChromosome);
                generationIndividual++;
            }
            return generationIndividuals;
        }

        /// <summary>
        /// Retrieves individuals of an initial generation in a population.
        /// </summary>
        /// <param name="population">A population to retrieve initial generation individuals from.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to lists of components.</param>
        /// <returns name="individuals">
        /// An individuals list of initial generation in a population.
        /// </returns>
        [NodeCategory("Action")]
        public static IList GetInitialGeneration(Population population, bool encoded = false)
        {
            if (population.GenerationsNumber > population.Generations.Count())
            {
                throw new Exception("Initial generation data overwritten by population strategy. To see initial generation for the population please use GeneratePopulation node with strategy set to: TrackingGenerationStrategy.");
            }
            if (encoded)
            {
                return population.Generations.First().Chromosomes.ToList();
            }

            IList generationIndividuals = new IList[population.MaxSize];
            int generationIndividual = 0;
            foreach (IChromosome populationChromosome in population.Generations.First().Chromosomes)
            {
                generationIndividuals[generationIndividual] = DecodeChromosome(populationChromosome);
                generationIndividual++;
            }
            return generationIndividuals;
        }

        /// <summary>
        /// Retrieves individuals of a nth generation from a population.
        /// </summary>
        /// <param name="population">A population to retrieve nth generation individuals from.</param>
        /// <param name="generation">A number of a generation to retrieve individuals for.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to lists of components.</param>
        /// <returns name="individuals">
        /// An individuals list of nth generation in a population.
        /// </returns>
        [NodeCategory("Action")]
        public static IList GetGeneration(Population population, int generation, bool encoded = false)
        {
            if ((generation < 0) || (generation > population.Generations.Count()))
            {
                throw new Exception("The given generation does not exist in the population.");
            }
            if (encoded)
            {
                return population.Generations[generation].Chromosomes.ToList();
            }

            IList generationIndividuals = new IList[population.MaxSize];
            int generationIndividual = 0;
            foreach (IChromosome populationChromosome in population.Generations[generation].Chromosomes)
            {
                generationIndividuals[generationIndividual] = DecodeChromosome(populationChromosome);
                generationIndividual++;
            }
            return generationIndividuals;
        }

        /// <summary>
        /// Retrieves individuals of all generations in a population.
        /// </summary>
        /// <param name="population">A population to retrieve generations individuals from.</param>
        /// <param name="bestChromosomesOnly">Whether only best chromosomes should be listed or all individuals in generations.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to lists of components.</param>
        /// <returns name="members">
        /// An individuals list of all generations in a population.
        /// </returns>
        [NodeCategory("Action")]
        public static IList GetGenerations(Population population, bool bestChromosomesOnly = true, bool encoded = false)
        {
            List<object> generationIndividuals = new List<object>();
            foreach (Generation generation in population.Generations)
            {
                if (!encoded)
                {
                    if (bestChromosomesOnly)
                    {
                        if (generation.BestChromosome != null)
                        {
                            generationIndividuals.Add(DecodeChromosome(generation.BestChromosome));
                        }
                        else
                        {
                            var mostCommon = generation.Chromosomes.GroupBy(i => i).OrderByDescending(c => c.Count()).Select(group => group.Key).First();
                            generationIndividuals.Add(DecodeChromosome(mostCommon));
                        }
                    }
                    else
                    {
                        List<object> collector = new List<object>();
                        foreach (IChromosome populationChromosome in population.Generations[population.Generations.IndexOf(generation)].Chromosomes)
                        {
                            collector.Add(DecodeChromosome(populationChromosome));
                        }
                        generationIndividuals.Add(collector);
                    }
                }
                else
                {
                    if (bestChromosomesOnly)
                    {
                        if (generation.BestChromosome != null)
                        {
                            generationIndividuals.Add(generation.BestChromosome);
                        }
                        else
                        {
                            var mostCommon = generation.Chromosomes.GroupBy(i => i).OrderByDescending(c => c.Count()).Select(group => group.Key).First();
                            generationIndividuals.Add(mostCommon);
                        }
                    }
                    else
                    {
                        generationIndividuals.Add(population.Generations[population.Generations.IndexOf(generation)].Chromosomes.ToList());
                    }
                }
            }
            return generationIndividuals;
        }

        /// <summary>
        /// Retrieves individuals of all the generations in a population, that have subsequently improved the fittness value.
        /// </summary>
        /// <param name="population">A population to retrieve generations individuals from.</param>
        /// <param name="bestChromosomesOnly">Whether only best chromosomes should be listed or all individuals in progressive generations.</param>
        /// <param name="encoded">Whether chromosomes should be listed as original string representations or converted to lists of components.</param>
        /// <returns name="individuals">
        /// An individuals list of all the generations in a population, that have subsequently improved the fittness value.
        /// </returns>
        [NodeCategory("Action")]
        public static IList GetProgressiveGenerations(Population population, bool bestChromosomesOnly = true, bool encoded = false)
        {
            List<object> generationIndividuals = new List<object>();

            int generations = 0;
            double? bestFitness = Double.NegativeInfinity;
            foreach (Generation generation in population.Generations)
            {
                if (generation.BestChromosome != null)
                {
                    if (generation.BestChromosome.Fitness > bestFitness)
                    {
                        bestFitness = generation.BestChromosome.Fitness;
                        generations++;
                    }
                }
                else if(generation.Number == population.GenerationsNumber)
                {
                    generations++;
                }
            }

            bestFitness = Double.NegativeInfinity;
            foreach (Generation generation in population.Generations)
            {
                if(generation.BestChromosome == null)
                {
                    if (generation.Number != population.GenerationsNumber)
                    {
                        continue;
                    }
                }
                else
                {
                    if (generation.BestChromosome.Fitness <= bestFitness)
                    {
                        continue;
                    }
                }

                if (!encoded)
                {
                    if (bestChromosomesOnly)
                    {
                        if (generation.BestChromosome != null)
                        {
                            generationIndividuals.Add(DecodeChromosome(generation.BestChromosome));
                        }
                        else
                        {
                            var mostCommon = generation.Chromosomes.GroupBy(i => i).OrderByDescending(c => c.Count()).Select(group => group.Key).First();
                            generationIndividuals.Add(DecodeChromosome(mostCommon));
                        }
                    }
                    else
                    {
                        List<object> collector = new List<object>();
                        foreach (IChromosome populationChromosome in population.Generations[population.Generations.IndexOf(generation)].Chromosomes)
                        {
                            collector.Add(DecodeChromosome(populationChromosome));
                        }
                        generationIndividuals.Add(collector);
                    }
                }
                else
                {
                    if (bestChromosomesOnly)
                    {
                        if (generation.BestChromosome != null)
                        {
                            generationIndividuals.Add(generation.BestChromosome);
                        }
                        else
                        {
                            var mostCommon = generation.Chromosomes.GroupBy(i => i).OrderByDescending(c => c.Count()).Select(group => group.Key).First();
                            generationIndividuals.Add(mostCommon);
                        }
                    }
                    else
                    {
                        generationIndividuals.Add(population.Generations[population.Generations.IndexOf(generation)].Chromosomes.ToList());
                    }
                }

                if (generation.BestChromosome != null)
                {
                    bestFitness = generation.BestChromosome.Fitness;
                }
            }
            return generationIndividuals;
        }

        /// <summary>
        /// Retreives a population processed by a genetic algorithm instance.
        /// </summary>
        /// <param name="algorithm">A genetic algorithm instance to retrieve a population from.</param>
        /// <returns name="population">
        /// A population processed by a genetic algorithm instance.
        /// </returns>
        [NodeCategory("Action")]
        public static Population GetPopulation(DynamoGeneticAlgorithm algorithm)
        {
            return (Population) algorithm.Population;
        }

        /// <summary>
        /// Returns number of generations in a genetic algorithm instance.
        /// </summary>
        /// <param name="algorithm">A genetic algorithm instance.</param>
        /// <returns name="generations">
        /// Number of generations in a genetic algorithm instance.
        /// </returns>
        [NodeCategory("Query")]
        public static int GenerationsNumber(DynamoGeneticAlgorithm algorithm)
        {
            return algorithm.Population.GenerationsNumber;
        }

        /// <summary>
        /// Lists chromosome elements (genes) or, in case of a floating point chromosome, a floating point value.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static object DecodeChromosome(IChromosome chromosome)
        {
            if (chromosome is FloatingPointChromosome)
            {
                return ((FloatingPointChromosome)chromosome).ToFloatingPoints().ToList();
            }
            if (chromosome is OrderedChromosome)
            {
                List<int> geneValues = new List<int>();
                foreach (Gene gene in ((OrderedChromosome)chromosome).GetGenes())
                {
                    geneValues.Add((int)gene.Value);
                }
                return geneValues;
            }
            return null;
        }
    }
}