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
        #region Constants

        /// <summary>
        /// The default crossover probability.
        /// </summary>
        public const float DefaultCrossoverProbability = 0.75f;

        /// <summary>
        /// The default mutation probability.
        /// </summary>
        public const float DefaultMutationProbability = 0.10f;

        /// <summary>
        /// Maximum number of iterations.
        /// </summary>
        public const int maxNumberOfIterations = 10000; //Maximum number of iteratitions for a single genetic algorithm instance. It is equal to maximum number of generations that can be reached.

        #endregion

        #region Fields

        private bool algorithmStopRequested;
        private readonly object algorithmLock = new object();
        private GeneticAlgorithmState algorithmState;
        /// <summary>
        /// Measures evolving time.
        /// </summary>
        public Stopwatch Timer;

        #endregion              

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticAlgorithm"/> class.
        /// </summary>
        /// <param name="population">The chromosomes population.</param>
        /// <param name="selection">The selection operator.</param>
        /// <param name="crossover">The crossover operator.</param>
        /// <param name="mutation">The mutation operator.</param>
        public DynamoGeneticAlgorithm(
                          IPopulation population,
                          ISelection selection,
                          ICrossover crossover,
                          IMutation mutation)
        {
            ExceptionHelper.ThrowIfNull("population", population);
            ExceptionHelper.ThrowIfNull("selection", selection);
            ExceptionHelper.ThrowIfNull("crossover", crossover);
            ExceptionHelper.ThrowIfNull("mutation", mutation);

            Population = population;
            Selection = selection;
            Crossover = crossover;
            Mutation = mutation;
            Reinsertion = new ElitistReinsertion();
            Termination = new GenerationNumberTermination(100);
            SelectionSize = (int) Math.Round(0.5 * population.MaxSize);

            CrossoverProbability = DefaultCrossoverProbability;
            MutationProbability = DefaultMutationProbability;
            TimeEvolving = TimeSpan.Zero;
            State = GeneticAlgorithmState.NotStarted;
            TaskExecutor = new LinearTaskExecutor();
            OperatorsStrategy = new DefaultOperatorsStrategy();
        }

        #region Events

        /// <summary>
        /// Occurs when generation ran.
        /// </summary>
        public event EventHandler GenerationRan;

        /// <summary>
        /// Occurs when termination reached.
        /// </summary>
        public event EventHandler TerminationReached;

        /// <summary>
        /// Occurs when stopped.
        /// </summary>
        public event EventHandler Stopped;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the operators strategy
        /// </summary>
        public IOperatorsStrategy OperatorsStrategy { get; set; }

        /// <summary>
        /// Gets the population.
        /// </summary>
        /// <value>The population.</value>
        public IPopulation Population { get; private set; }

        /// <summary>
        /// Gets the list of fitness function results.
        /// </summary>
        public List<double> Fitness { get; set; }

        /// <summary>
        /// Gets or sets the selection operator.
        /// </summary>
        public ISelection Selection { get; set; }

        /// <summary>
        /// Gets or sets the number of members selected per selection.
        /// </summary>
        public int SelectionSize { get; set; }

        /// <summary>
        /// Gets or sets the crossover operator.
        /// </summary>
        /// <value>The crossover.</value>
        public ICrossover Crossover { get; set; }

        /// <summary>
        /// Gets or sets the crossover probability.
        /// </summary>
        public float CrossoverProbability { get; set; }

        /// <summary>
        /// Gets or sets the mutation operator.
        /// </summary>
        public IMutation Mutation { get; set; }

        /// <summary>
        /// Gets or sets the mutation probability.
        /// </summary>
        public float MutationProbability { get; set; }

        /// <summary>
        /// Gets or sets the reinsertion operator.
        /// </summary>
        public IReinsertion Reinsertion { get; set; }

        /// <summary>
        /// Gets or sets the termination condition.
        /// </summary>
        public ITermination Termination { get; set; }

        /// <summary>
        /// Gets the generations number.
        /// </summary>
        /// <value>The generations number.</value>
        public int GenerationsNumber
        {
            get
            {
                return Population.GenerationsNumber;
            }
        }

        /// <summary>
        /// Gets the best chromosome.
        /// </summary>
        /// <value>The best chromosome.</value>
        public IChromosome BestChromosome
        {
            get
            {
                return Population.BestChromosome;
            }
        }

        /// <summary>
        /// Gets the time evolving.
        /// </summary>
        public TimeSpan TimeEvolving { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public GeneticAlgorithmState State
        {
            get
            {
                return algorithmState;
            }

            private set
            {
                var shouldStop = Stopped != null && algorithmState != value && value == GeneticAlgorithmState.Stopped;
                algorithmState = value;

                if (shouldStop)
                {
                    Stopped?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public bool IsRunning
        {
            get
            {
                return State == GeneticAlgorithmState.Started || State == GeneticAlgorithmState.Resumed;
            }
        }

        /// <summary>
        /// Gets or sets the task executor which will be used to execute fitness evaluation.
        /// </summary>
        public ITaskExecutor TaskExecutor { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Passes null as default input value.
        /// </summary>
        public static object Default()
        {
            return null;
        }

        /// <summary>
        /// Starts the genetic algorithm using population, selection, crossover, mutation and termination configured, passing the given list of fitness function results.
        /// </summary>
        public void Start(List<double> fitness)
        {
            lock (algorithmLock)
            {
                State = GeneticAlgorithmState.Started;
                //algorithmStopwatch.Stop();
                //TimeEvolving = TimeEvolving.Add(algorithmStopwatch.Elapsed);
            }
            Resume(fitness);
        }

        /// <summary>
        /// Resumes the last evolution of the genetic algorithm.
        /// <remarks>
        /// If genetic algorithm was not explicit Stop (calling Stop method), you will need provide a new extended Termination.
        /// </remarks>
        /// </summary>
        public void Resume(List<double> fitness)
        {
            try
            {
                lock (algorithmLock)
                {
                    algorithmStopRequested = false;
                    if (Timer.IsRunning == false)
                    {
                        Timer.Start();
                    }
                }
                if (Population.GenerationsNumber == 0)
                {
                    throw new InvalidOperationException("Attempt to resume a genetic algorithm which was not yet started.");
                }

                if (Population.GenerationsNumber > 1)
                {
                    //if (Termination.HasReached(this))
                    if(State == GeneticAlgorithmState.TerminationReached)
                    {
                        throw new InvalidOperationException("Attempt to resume a genetic algorithm with a termination ({0}) already reached. Please, specify a new termination or extend the current one.".With(Termination));
                    }
                    State = GeneticAlgorithmState.Resumed;
                }

                for (int i = 0; i < Population.CurrentGeneration.Chromosomes.Count(); i++)
                {
                    Population.CurrentGeneration.Chromosomes[i].Fitness = fitness[i];
                    Population.Generations[Population.Generations.IndexOf(Population.CurrentGeneration)].Chromosomes[i].Fitness = fitness[i];
                }

                Population.EndCurrentGeneration();
                if (algorithmState != GeneticAlgorithmState.TerminationReached)
                {
                    TimeEvolving = Timer.Elapsed;
                    EvolveOneGeneration();
                }
                else
                {
                    return;
                }
            }
            catch
            {
                State = GeneticAlgorithmState.Stopped;

                TimeEvolving = Timer.Elapsed;
                Timer.Stop();
                throw;
            }
        }

        /// <summary>
        /// Stops the genetic algorithm..
        /// </summary>
        public void Stop()
        {
            if (Population.GenerationsNumber == 0)
            {
                throw new InvalidOperationException("Attempt to stop a genetic algorithm which was not yet started.");
            }

            lock (algorithmLock)
            {
                algorithmStopRequested = true;
            }
        }

        /// <summary>
        /// Evolve one generation.
        /// </summary>
        /// <returns>True if termination has been reached, otherwise false.</returns>
        private bool EvolveOneGeneration()
        {
            Population.MinSize = SelectionSize;
            if (Population.MinSize > Population.MaxSize)
            {
                Population.MinSize = Population.MaxSize;
            }

            var parents = SelectParents();
            var offspring = Cross(parents);
            Mutate(offspring);

            Population.MinSize = Population.MaxSize;
            var newGenerationChromosomes = Reinsert(offspring, Population.CurrentGeneration.Chromosomes);

            Population.CreateNewGeneration(newGenerationChromosomes);
            return CheckForTermination();
        }

        /// <summary>
        /// Ends the current generation.
        /// </summary>
        /// <returns><c>true</c>, if current generation was ended, <c>false</c> otherwise.</returns>
        private bool CheckForTermination()
        {
            var handler = GenerationRan;
            handler?.Invoke(this, EventArgs.Empty);

            if (Termination.HasReached(this) || (Population.GenerationsNumber >= maxNumberOfIterations))
            {
                State = GeneticAlgorithmState.TerminationReached;

                handler = TerminationReached;
                handler?.Invoke(this, EventArgs.Empty);

                TimeEvolving = Timer.Elapsed;
                Timer.Stop();

                return true;
            }
            if (algorithmStopRequested)
            {
                TaskExecutor.Stop();
                State = GeneticAlgorithmState.Stopped;

                TimeEvolving = Timer.Elapsed;
                Timer.Stop();
            }
            return false;
        }

        /// <summary>
        /// Selects the parents.
        /// </summary>
        /// <returns>The parents.</returns>
        private IList<IChromosome> SelectParents()
        {
            return Selection.SelectChromosomes(Population.MinSize, Population.CurrentGeneration);
        }

        /// <summary>
        /// Crosses the specified parents.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <returns>The result chromosomes.</returns>
        private IList<IChromosome> Cross(IList<IChromosome> parents)
        {
            return OperatorsStrategy.Cross(Population, Crossover, CrossoverProbability, parents);
        }

        /// <summary>
        /// Mutate the specified chromosomes.
        /// </summary>
        /// <param name="chromosomes">The chromosomes.</param>
        private void Mutate(IList<IChromosome> chromosomes)
        {
            OperatorsStrategy.Mutate(Mutation, MutationProbability, chromosomes);
        }

        /// <summary>
        /// Reinsert the specified offspring and parents.
        /// </summary>
        /// <param name="offspring">The offspring chromosomes.</param>
        /// <param name="parents">The parents chromosomes.</param>
        /// <returns>
        /// The reinserted chromosomes.
        /// </returns>
        private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
        {
            return Reinsertion.SelectChromosomes(Population, offspring, parents);
        }

        #endregion
    }
}