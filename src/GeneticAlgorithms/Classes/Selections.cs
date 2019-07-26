using System;
using System.Collections;
using System.Collections.Generic;

using Evo;
using GeneticSharp.Domain.Selections;

using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for definition of selection classes.
    /// Selection, besides crossover and mutation, is one of the main operators in genetic algorithms.
    /// </summary>
    public static class Selections
    {
        /// <summary>
        /// Defines a selection instance: Elite Selection.
        /// </summary>
        /// <returns name="selection">Selection
        /// A selection instance: Elite Selection.
        /// </returns>
        [NodeCategory("Create")]
        public static EliteSelection EliteSelection()
        {
            return new EliteSelection();
        }

        /// <summary>
        /// Defines a selection instance: Roulette Wheel Selection.
        /// </summary>
        /// <returns name="selection">
        /// A selection instance: Roulette Wheel Selection.
        /// </returns>
        [NodeCategory("Create")]
        public static RouletteWheelSelection RouletteWheelSelection()
        {
            return new RouletteWheelSelection();
        }

        /// <summary>
        /// Defines a selection instance: Stochastic Universal Sampling Selection.
        /// </summary>
        /// <returns name="selection">
        /// A selection instance: Stochastic Universal Sampling Selection.
        /// </returns>
        [NodeCategory("Create")]
        public static StochasticUniversalSamplingSelection StochasticUniversalSamplingSelection()
        {
            return new StochasticUniversalSamplingSelection();
        }

        /// <summary>
        /// Defines a selection instance: Tournament Selection.
        /// </summary>
        /// <param name="tournamentSize">Number of random individuals taken to each tournament.</param>
        /// <param name="allowRepetetitions">Whether a given individual can win several tournaments.</param>
        /// <returns name="selection">
        /// A selection instance: Tournament Selection.
        /// </returns>
        [NodeCategory("Create")]
        public static TournamentSelection TournamentSelection(int tournamentSize = 3, bool allowRepetetitions = true)
        {
            return new TournamentSelection(tournamentSize, allowRepetetitions);
        }

        /// <summary>
        /// Returns type of selection applied to a genetic algorithm instance.
        /// </summary>
        /// <param name="algorithm">A genetic algorithm instance to check for the selection method.</param>
        /// <returns name="selection">
        /// Type of selection applied to a genetic algorithm instance.
        /// </returns>
        [NodeCategory("Query")]
        public static object GetSelection(DynamoGeneticAlgorithm algorithm)
        {
            if (algorithm.Selection is EliteSelection)
            {
                return algorithm.Selection as EliteSelection;
            }
            if (algorithm.Selection is RouletteWheelSelection)
            {
                return algorithm.Selection as RouletteWheelSelection;
            }
            if (algorithm.Selection is StochasticUniversalSamplingSelection)
            {
                return algorithm.Selection as StochasticUniversalSamplingSelection;
            }
            if (algorithm.Selection is TournamentSelection)
            {
                return algorithm.Selection as TournamentSelection;
            }
            return null;
        }
    }
}