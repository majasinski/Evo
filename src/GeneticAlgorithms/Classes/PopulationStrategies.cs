using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Populations;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for generation of population strategies.
    /// Population strategies are then used in the <see cref="Populations"/> class methods, defining the way next generations are stored in a population.
    /// </summary>
    public static class PopulationStrategies
    {
        /// <summary>Defines a population storage strategy: only x most recent generations are stored in a population.</summary>
        /// <param name="maxGenerations">Maximum number of generations to be stored in a population.</param>
        /// <returns name="strategy">A population storage strategy: only x most recent generations are stored in a population.</returns>
        [NodeCategory("Create")]
        public static PerformanceGenerationStrategy PerformanceStrategy(int maxGenerations = 10)
        {
            PerformanceGenerationStrategy strategy = new PerformanceGenerationStrategy(maxGenerations);
            return strategy;
        }

        /// <summary>Defines a population storage strategy: all generations are stored in a population.</summary>
        /// <returns name="strategy">A population storage strategy: all generations are stored in a population.</returns>
        [NodeCategory("Create")]
        public static TrackingGenerationStrategy TrackingStrategy()
        {
            TrackingGenerationStrategy strategy = new TrackingGenerationStrategy();
            return strategy;
        }
    }
}