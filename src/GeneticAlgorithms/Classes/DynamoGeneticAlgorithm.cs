using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Texts;
using GeneticSharp.Infrastructure.Framework.Threading;
using GeneticSharp.Infrastructure.Framework.Commons;

using Autodesk.DesignScript.Runtime;

namespace Evo
{
    #region Enums

    /// <summary>
    /// Possible states for a genetic algorithm.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public enum GeneticAlgorithmState
    {
        /// <summary>
        /// The GA has not been started yet.
        /// </summary>
        NotStarted,

        /// <summary>
        /// The GA has been started and is running.
        /// </summary>
        Started,

        /// <summary>
        /// The GA has been stopped and is not running.
        /// </summary>
        Stopped,

        /// <summary>
        /// The GA has been resumed after a stop or termination reach and is running.
        /// </summary>
        Resumed,

        /// <summary>
        /// The GA has reach the termination condition and is not running.
        /// </summary>
        TerminationReached
    }

    #endregion

    /// <summary>
    /// GeneticSharp's GeneticAlgorithm modifications making the class applicable to Dynamo Environment.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public sealed class DynamoGeneticAlgorithm : IGeneticAlgorithm
    {
        /// <summary>The default crossover probability.</summary>
        public const double DefaultCrossoverProbability = 0.75;
        /// <summary>efault mutation probability.</summary>
        public const double DefaultMutationProbability = 0.05;
        /// <summary>Maximum number of iterations.</summary>
        public const int maxNumberOfIterations = 10000; //Maximum number of iteratitions for a single genetic algorithm instance.

        /// <summary>Measures evolving time.</summary>
        public Stopwatch Timer;

        /// <summary>Initializes a new instance of the <see cref="GeneticAlgorithm"/> class.</summary>
        /// <param name="population">The chromosomes population.</param>
        /// <param name="selection">The selection operator.</param>
        /// <param name="crossover">The crossover operator.</param>
        /// <param name="mutation">The mutation operator.</param>
        /// <param name="reinsertion">The reinsertion operator.</param>
        /// <param name="termination">The termination operator.</param>

        public DynamoGeneticAlgorithm(
                          IPopulation population,
                          ISelection selection,
                          ICrossover crossover,
                          IMutation mutation,
                          IReinsertion reinsertion,
                          ITermination termination)
        {
            ExceptionHelper.ThrowIfNull("population", population);
            ExceptionHelper.ThrowIfNull("selection", selection);
            ExceptionHelper.ThrowIfNull("crossover", crossover);
            ExceptionHelper.ThrowIfNull("mutation", mutation);
            ExceptionHelper.ThrowIfNull("reinsertion", reinsertion);
            ExceptionHelper.ThrowIfNull("termination", termination);

            Population = population;
            Selection = selection;
            Crossover = crossover;
            Mutation = mutation;
            Reinsertion = reinsertion;
            Termination = termination;

            SelectionSize = 0;
            CrossoverProbability = (float) DefaultCrossoverProbability;
            MutationProbability = (float) DefaultMutationProbability;

            TimeEvolving = TimeSpan.Zero;
            OperatorsStrategy = new DefaultOperatorsStrategy();

            State = GeneticAlgorithmState.NotStarted;
        }

        /// <summary>Gets or sets the operators strategy.</summary>
        public IOperatorsStrategy OperatorsStrategy { get; set; }
        /// <summary>Gets or sets the population.</summary>
        public IPopulation Population { get; private set; }

        /// <summary>Gets or sets the selection operator.</summary>
        public ISelection Selection { get; set; }
        /// <summary>Gets or sets the crossover operator.</summary>
        public ICrossover Crossover { get; set; }
        /// <summary>Gets or sets the mutation operator.</summary>
        public IMutation Mutation { get; set; }
        /// <summary>Gets or sets the reinsertion operator.</summary>
        public IReinsertion Reinsertion { get; set; }
        /// <summary>Gets or sets the termination condition.</summary>
        public ITermination Termination { get; set; }

        /// <summary>Gets or sets the number of members selected per selection.</summary>
        public int SelectionSize { get; set; }
        /// <summary>Gets or sets the crossover probability.</summary>
        public float CrossoverProbability { get; set; }
        /// <summary>Gets or sets the mutation probability.</summary>
        public float MutationProbability { get; set; }

        /// <summary>Gets the generations number.</summary>
        public int GenerationsNumber
        {
            get
            {
                return Population.GenerationsNumber;
            }
        }
        /// <summary>Gets the best chromosome.</summary>
        public IChromosome BestChromosome
        {
            get
            {
                return Population.BestChromosome;
            }
        }

        /// <summary>Gets the time evolving.</summary>
        public TimeSpan TimeEvolving { get; private set; }
        /// <summary>Gets the algorithm state.</summary>
        public GeneticAlgorithmState State { get; private set; }
        /// <summary>Gets a value indicating whether this instance is running.</summary>
        public bool IsRunning
        {
            get
            {
                return (State == GeneticAlgorithmState.Started) || (State == GeneticAlgorithmState.Resumed);
            }
        }

        /// <summary>Passes null as default input value.</summary>
        public static object Default()
        {
            return null;
        }

        /// <summary>Assigns fitness function value to parents, if not assigned yet, and performs one cycle of selection, crossover and mutation in the most recent generation.</summary>
        /// <param name="fitness">A list of fitness function results for input generation.</param>
        /// <returns name="parents">Parents selected for crossover.</returns>
        /// <returns name="offsprings">Offsprings produced by crossover and mutation.</returns>
        [MultiReturn(new[] { "parents", "offsprings" })]
        public Dictionary<string, List<IChromosome>> ProduceOffsprings(List<double?> fitness)
        {
            if (State == GeneticAlgorithmState.TerminationReached)
            {
                return new Dictionary<string, List<IChromosome>>
                {
                    { "parents", Population.CurrentGeneration.Chromosomes.ToList() },
                    { "offsprings", new List<IChromosome>() }
                };
            }

            State = GeneticAlgorithmState.Started;
            if (fitness.Any())
            {
                for (int i = 0; i < Population.CurrentGeneration.Chromosomes.Count(); i++)
                {
                    if (fitness[i] != null)
                    {
                        Population.CurrentGeneration.Chromosomes[i].Fitness = fitness[i];
                        Population.Generations[Population.Generations.IndexOf(Population.CurrentGeneration)].Chromosomes[i].Fitness = fitness[i];
                    }
                }
            }

            Population.MinSize = SelectionSize;

            //Select parents out of population basing on the given selection operator:
            var parents = SelectParents();
            //Produce offsprings basing on the given crossover operator and crossover probability:
            var offsprings = Cross(parents);
            //Mutate offsprings basing on the given mutation operator and mutation probability:
            Mutate(offsprings);
            return new Dictionary<string, List<IChromosome>>
            {
                { "parents", parents.ToList() },
                { "offsprings", offsprings.ToList() }
            };
        }

        /// <summary>Assigns fitness function value to offsprings and registers new generation in a population.</summary>
        /// <param name="parents">Parents obtained form the ProduceOffsprings node.</param>
        /// <param name="offsprings">Offsprings obtained form the ProduceOffsprings node.</param>
        /// <param name="offspringsFitness">A list of fitness function results for offsprings.</param>
        /// <returns name="algorithm">A genetic algorithm instance supplemented with a new generation.</returns>
        public void ProduceNewGeneration(List<object> parents, List<object> offsprings, List<double> offspringsFitness)
        {
            if (State == GeneticAlgorithmState.TerminationReached)
            {
                return;
            }

            IList<IChromosome> parentsChromosomes = new List<IChromosome>();
            foreach(object parent in parents)
            {
                parentsChromosomes.Add((IChromosome) parent);
            }
            IList<IChromosome> offspringsChromosomes = new List<IChromosome>();
            foreach (object offspring in offsprings)
            {
                offspringsChromosomes.Add((IChromosome) offspring);
                offspringsChromosomes.Last().Fitness = offspringsFitness[offsprings.IndexOf(offspring)];
            }

            Population.MinSize = Population.MaxSize;

            var newGenerationChromosomes = Reinsert(offspringsChromosomes, parentsChromosomes);
            while (newGenerationChromosomes.Count < Population.CurrentGeneration.Chromosomes.Count())
            {
                newGenerationChromosomes = Reinsert(newGenerationChromosomes, parentsChromosomes);
            }
            Population.CreateNewGeneration(newGenerationChromosomes);
            Population.EndCurrentGeneration();

            if (Termination.HasReached(this) || (Population.GenerationsNumber >= maxNumberOfIterations))
            {
                State = GeneticAlgorithmState.TerminationReached;

                TimeEvolving = Timer.Elapsed;
                Timer.Stop();
            }
        }

        /// <summary>Selects the parents.</summary>
        private IList<IChromosome> SelectParents()
        {
            return Selection.SelectChromosomes(Population.MinSize, Population.CurrentGeneration);
        }
        /// <summary>Crosses the specified parents.</summary>
        /// <param name="parents">The parents.</param>
        private IList<IChromosome> Cross(IList<IChromosome> parents)
        {
            return OperatorsStrategy.Cross(Population, Crossover, CrossoverProbability, parents);
        }
        /// <summary>Mutates the specified chromosomes.</summary>
        /// <param name="chromosomes">The chromosomes.</param>
        private void Mutate(IList<IChromosome> chromosomes)
        {
            OperatorsStrategy.Mutate(Mutation, MutationProbability, chromosomes);
        }
        /// <summary>Reinserts the specified offspring and parents.</summary>
        /// <param name="offspring">The offspring chromosomes.</param>
        /// <param name="parents">The parents chromosomes.</param>
        private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
        {
            return Reinsertion.SelectChromosomes(Population, offspring, parents);
        }
    }
}