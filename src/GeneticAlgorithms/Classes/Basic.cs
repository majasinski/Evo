using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Evo;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using GeneticSharp.Domain.Reinsertions;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Main methods of genetic algorithm instances.
    /// </summary>
    public static class Basic
    {
        /// <summary>Creates a genetic algorithm instace.</summary>
        /// <param name="population">A population of initial candidate solutions to resolve.</param>
        /// <param name="selection">A selection instance returned by one of Selections nodes (default selection operator: Elite Selection).</param>
        /// <param name="crossover">A crossover instance returned by one of Crossovers nodes (default crossover operator: Uniform Crossover).</param>
        /// <param name="mutation">A mutation instance returned by one of Mutations nodes (default mutation operator: Flip Bit Mutation).</param>
        /// <param name="termination">A termination instance returned by one of Termination nodes (if not specified, terminated after 100 stagnant generations).</param>
        /// <param name="selectionSize">Minimum number of elements to be selected by a selection operator (if not specified, taken as a half of initial generation size).</param>
        /// <param name="crossoverProbability">Crossover rate.</param>
        /// <param name="mutationProbability">Mutation rate.</param>
        /// <returns name="algorithm">A genetic algorithm instance.</returns>
        [NodeCategory("Create")]
        public static DynamoGeneticAlgorithm CreateGeneticAlgorithm(Population population,
            [DefaultArgument("Evo.DynamoGeneticAlgorithm.Default()")] object selection,
            [DefaultArgument("Evo.DynamoGeneticAlgorithm.Default()")] object crossover,
            [DefaultArgument("Evo.DynamoGeneticAlgorithm.Default()")] object mutation,
            [DefaultArgument("Evo.DynamoGeneticAlgorithm.Default()")] object termination,
            [DefaultArgument("Evo.DynamoGeneticAlgorithm.Default()")] int? selectionSize,
            double crossoverProbability = DynamoGeneticAlgorithm.DefaultCrossoverProbability,
            double mutationProbability = DynamoGeneticAlgorithm.DefaultMutationProbability)
        {
            IChromosome exemplaryChromome = population.CurrentGeneration.Chromosomes[0];
            ChromosomeType chromosomeType = 0;
            if (exemplaryChromome is BinaryChromosome)
            {
                chromosomeType = ((BinaryChromosome) exemplaryChromome).ChromosomeType;
            }
            else if (exemplaryChromome is CombinatorialChromosome)
            {
                chromosomeType = ((CombinatorialChromosome) exemplaryChromome).ChromosomeType;
            }
            else if (exemplaryChromome is DoubleChromosome)
            {
                chromosomeType = ((DoubleChromosome) exemplaryChromome).ChromosomeType;
            }

            ISelection selectionMethod = null;
            if (selection == null)
            {
                selectionMethod = new EliteSelection();
            }
            else
            {
                if (selection is EliteSelection)
                {
                    selectionMethod = selection as EliteSelection;
                }
                else if (selection is RouletteWheelSelection)
                {
                    selectionMethod = selection as RouletteWheelSelection;
                }
                else if (selection is StochasticUniversalSamplingSelection)
                {
                    selectionMethod = selection as StochasticUniversalSamplingSelection;
                }
                else if (selection is TournamentSelection)
                {
                    selectionMethod = selection as TournamentSelection;
                }
                else
                {
                    throw new CrossoverException("Invalid selection input. A valid object returned by a node of the Selections category should be used.");
                }
            }

            ICrossover crossoverMethod = null;
            if (crossover == null)
            {
                crossoverMethod = new UniformCrossover(0.5f);
            }
            else
            {
                if (crossover is AlternatingPositionCrossover)
                {
                    if (chromosomeType != ChromosomeType.CombinatorialChromosome)
                    {
                        throw new CrossoverException("The Alternating-position Crossover (AP) can be only used with combinatorial chromosomes. The specified individuals are not combinatorial chromosomes.");
                    }
                    crossoverMethod = crossover as AlternatingPositionCrossover;
                }
                else if (crossover is CutAndSpliceCrossover)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Cut and Splice Crossover (AP) can be only used with binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    crossoverMethod = crossover as CutAndSpliceCrossover;
                }
                else if (crossover is CycleCrossover)
                {
                    if (chromosomeType != ChromosomeType.CombinatorialChromosome)
                    {
                        throw new CrossoverException("The Cycle Crossover (CX) can be only used with combinatorial chromosomes. The specified individuals are not combinatorial chromosomes.");
                    }
                    crossoverMethod = crossover as CycleCrossover;
                }
                else if (crossover is OnePointCrossover)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The One-point Crossover can be only used with binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    crossoverMethod = crossover as OnePointCrossover;
                }
                else if (crossover is OrderBasedCrossover)
                {
                    if (chromosomeType != ChromosomeType.CombinatorialChromosome)
                    {
                        throw new CrossoverException("The Order Based Crossover (OX2) can be only used with combinatorial chromosomes. The specified individuals are not combinatorial chromosomes.");
                    }
                    crossoverMethod = crossover as OrderBasedCrossover;
                }
                else if (crossover is OrderedCrossover)
                {
                    if (chromosomeType != ChromosomeType.CombinatorialChromosome)
                    {
                        throw new CrossoverException("The Ordered Crossover (OX1) can be only used with combinatorial chromosomes. The specified individuals are not combinatorial chromosomes.");
                    }
                    crossoverMethod = crossover as OrderedCrossover;
                }
                else if (crossover is PartiallyMappedCrossover)
                {
                    if (chromosomeType != ChromosomeType.CombinatorialChromosome)
                    {
                        throw new CrossoverException("The Partially-mapped Crossover (PMX) can be only used with combinatorial chromosomes. The specified individuals are not combinatorial chromosomes.");
                    }
                    crossoverMethod = crossover as PartiallyMappedCrossover;
                }
                else if (crossover is PositionBasedCrossover)
                {
                    if (chromosomeType != ChromosomeType.CombinatorialChromosome)
                    {
                        throw new CrossoverException("The Position Based Crossover (POS) can be only used with combinatorial chromosomes. The specified individuals are not combinatorial chromosomes.");
                    }
                    crossoverMethod = crossover as PositionBasedCrossover;
                }
                else if (crossover is SimulatedBinaryCrossover)
                {
                    if (chromosomeType != ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Simulated Binary Crossover (SBX) can be only used with double chromosomes. The specified individuals are not double chromosomes.");
                    }
                    crossoverMethod = crossover as SimulatedBinaryCrossover;
                }
                else if (crossover is ThreeParentCrossover)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Three-parent Crossover can be only used with binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    crossoverMethod = crossover as ThreeParentCrossover;
                }
                else if (crossover is TwoPointCrossover)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Two-point Crossover can be only used with binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    crossoverMethod = crossover as TwoPointCrossover;
                }
                else if (crossover is UniformCrossover)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Uniform Crossover can be only used with binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    crossoverMethod = crossover as UniformCrossover;
                }
                else if (crossover is VotingRecombinationCrossover)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Voting Recombination Crossover (VR) can be only used with binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    crossoverMethod = crossover as VotingRecombinationCrossover;
                }
                else
                {
                    throw new CrossoverException("Invalid crossover input. A valid object returned by a node of the Crossovers category should be used.");
                }
            }

            IMutation mutationMethod = null;
            if (mutation == null)
            {
                mutationMethod = new FlipBitMutation();
            }
            else
            {
                if (mutation is DisplacementMutation)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Displacement Mutation can be only used on binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    mutationMethod = mutation as DisplacementMutation;
                }
                else if (mutation is FlipBitMutation)
                {
                    if (chromosomeType != ChromosomeType.BinaryChromosome)
                    {
                        throw new CrossoverException("The Flip Bit Mutation can be only used on binary chromosomes. The specified individuals are not binary chromosomes.");
                    }
                    mutationMethod = mutation as FlipBitMutation;
                }
                else if (mutation is GaussianMutation)
                {
                    if (chromosomeType != ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Gaussian Mutation can be only used on double chromosomes. The specified individuals are not double chromosomes.");
                    }
                    mutationMethod = mutation as GaussianMutation;
                }
                else if (mutation is InsertionMutation)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Displacement Mutation can be only used on binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    mutationMethod = mutation as InsertionMutation;
                }
                else if (mutation is PartialShuffleMutation)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Displacement Mutation (PSM) can be only used on binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    mutationMethod = mutation as PartialShuffleMutation;
                }
                else if (mutation is PolynomialMutation)
                {
                    if (chromosomeType != ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Polynomial Mutation can be only used on double chromosomes. The specified individuals are not double chromosomes.");
                    }
                    mutationMethod = mutation as PolynomialMutation;
                }
                else if (mutation is ReverseSequenceMutation)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Displacement Mutation (RSM) can be only used on binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    mutationMethod = mutation as ReverseSequenceMutation;
                }
                else if (mutation is TworsMutation)
                {
                    if (chromosomeType == ChromosomeType.DoubleChromosome)
                    {
                        throw new CrossoverException("The Displacement Mutation can be only used on binary and combinatorial chromosomes. The specified individuals are double chromosomes.");
                    }
                    mutationMethod = mutation as TworsMutation;
                }
                else if (mutation is UniformMutation)
                {
                    if (chromosomeType != ChromosomeType.BinaryChromosome)
                    {
                        throw new CrossoverException("The Uniform Mutation can be only used on binary chromosomes. The specified individuals are not binary chromosomes.");
                    }
                    mutationMethod = mutation as UniformMutation;
                }
                else
                {
                    throw new CrossoverException("Invalid mutation input. A valid object returned by a node of Mutations category should be used.");
                }
            }

            ElitistReinsertion reinsertionMethod = new ElitistReinsertion();

            ITermination terminationMethod = null;
            if (termination == null)
            {
                terminationMethod = new FitnessStagnationTermination(100);
            }
            else
            {
                if (termination is FitnessStagnationTermination)
                {
                    terminationMethod = termination as FitnessStagnationTermination;
                }
                else if (termination is FitnessThresholdTermination)
                {
                    terminationMethod = termination as FitnessThresholdTermination;
                }
                else if (termination is GenerationNumberTermination)
                {
                    terminationMethod = termination as GenerationNumberTermination;
                }
                else if (termination is TimeEvolvingTermination)
                {
                    terminationMethod = termination as TimeEvolvingTermination;
                }
                else if (termination is AndTermination)
                {
                    terminationMethod = termination as AndTermination;
                }
                else if (termination is OrTermination)
                {
                    terminationMethod = termination as OrTermination;
                }
                else
                {
                    throw new CrossoverException("Invalid termination input. An object returned by nodes of Terminations library should be used.");
                }
            }

            if(selectionSize == null)
            {
                selectionSize = (int) Math.Round(0.5 * population.MaxSize);
            }
            else if(!(selectionSize is int))
            {
                throw new CrossoverException("Defined selection size is not an integer.");
            }
            else if(selectionSize > population.MaxSize)
            {
                throw new CrossoverException("Selection size cannot be greater than population size.");
            }

            var algorithm = new DynamoGeneticAlgorithm(population, selectionMethod, crossoverMethod, mutationMethod, reinsertionMethod, terminationMethod)
            {
                SelectionSize = (int)selectionSize,
                CrossoverProbability = (float)crossoverProbability,
                MutationProbability = (float)mutationProbability,

                Timer = Stopwatch.StartNew()
            };
            algorithm.Timer.Start();
            return algorithm;
        }

        /// <summary>Assigns fitness function value to parents, if not assigned yet, and performs one cycle of selection, crossover and mutation in the most recent generation.</summary>
        /// <param name="algorithm">A genetic algorithm instance to evolve in.</param>
        /// <param name="fitness">A list of fitness function results for input generation.</param>
        /// <returns name="parents">Parents selected for crossover.</returns>
        /// <returns name="offsprings">Offsprings produced by crossover and mutation.</returns>
        [NodeCategory("Action")]
        [MultiReturn(new[] { "parents", "offsprings" })]
        public static Dictionary<string, List<IChromosome>> ProduceOffsprings(DynamoGeneticAlgorithm algorithm, List<double?> fitness)
        {
            return algorithm.ProduceOffsprings(fitness);
        }

        /// <summary>Assigns fitness function value to offsprings and registers new generation in a population.</summary>
        /// <param name="algorithm">A genetic algorithm instance to evolve in.</param>
        /// <param name="parents">Parents obtained form the ProduceOffsprings node.</param>
        /// <param name="offsprings">Offsprings obtained form the ProduceOffsprings node.</param>
        /// <param name="offspringsFitness">A list of fitness function results for offsprings.</param>
        /// <returns name="algorithm">A genetic algorithm instance supplemented with a new generation.</returns>
        [NodeCategory("Action")]
        public static DynamoGeneticAlgorithm ProduceNewGeneration(DynamoGeneticAlgorithm algorithm, List<object> parents, List<object> offsprings, List<double> offspringsFitness)
        {
            algorithm.ProduceNewGeneration(parents, offsprings, offspringsFitness);
            return algorithm;
        }

        /// <summary>Checks for termination of a genetic algorithm instance. </summary>
        /// <param name="algorithm">A genetic algorithm instance.</param>
        /// <returns name="finished">Whether genetic algorithm termination conditions are met.</returns>
        [NodeCategory("Query")]
        public static bool HasFinished(DynamoGeneticAlgorithm algorithm)
        {
            return algorithm.State == GeneticAlgorithmState.TerminationReached;
        }

        /// <summary>Returns total processing time of a genetic algorithm instance (in seconds).</summary>
        /// <param name="algorithm">A genetic algorithm instance.</param>
        /// <returns name="seconds">Total processing time of a genetic algorithm instance (in seconds).</returns>
        [NodeCategory("Query")]
        public static double TimeEvolving(DynamoGeneticAlgorithm algorithm)
        {
            return algorithm.TimeEvolving.TotalSeconds;
        }
    }
}