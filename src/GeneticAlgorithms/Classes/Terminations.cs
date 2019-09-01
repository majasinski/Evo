using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Texts;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for termination of genetic algorithms.
    /// Terminations break processing of genetic algorithms if given conditions are met.
    /// </summary>
    public static class Terminations
    {
        /// <summary>Terminates genetic algorithm processing if an expected number of stagnant generations is reached.</summary>
        /// <param name="stagnantGenerations">A number of stagnant generations to terminate processing.</param>
        /// <returns name="termination">Termination of genetic algorithm processing triggered by a number of stagnant generations.</returns>
        [NodeCategory("Create")]
        public static FitnessStagnationTermination FitnessStagnationTermination(int stagnantGenerations = 100)
        {
            if (stagnantGenerations < 1)
            {
                throw new Exception("Invalid value. Number of stagnant generations cannot be lower than 1.");
            }
            if (stagnantGenerations > DynamoGeneticAlgorithm.maxNumberOfIterations)
            {
                throw new Exception("Invalid value. Number of stagnant generations cannot be greater than {0}.".With(DynamoGeneticAlgorithm.maxNumberOfIterations));
            }
            return new FitnessStagnationTermination(stagnantGenerations);
        }

        /// <summary>Terminates genetic algorithm processing if an expected fitness value was found.</summary>
        /// <param name="expectedFitness">An expected fitness value to terminate processing.</param>
        /// <returns name="termination">Termination of genetic algorithm processing triggered by a threshold fitness value.</returns>
        [NodeCategory("Create")]
        public static FitnessThresholdTermination FitnessThresholdTermination(double expectedFitness)
        {
            return new FitnessThresholdTermination(expectedFitness);
        }

        /// <summary>Terminates genetic algorithm processing if a given number of generations was reached.</summary>
        /// <param name="maxIterations">A number of generations to product in order to terminate processing.</param>
        /// <returns name="termination">Termination of genetic algorithm processing triggered by a number of produced generations.</returns>
        [NodeCategory("Create")]
        public static GenerationNumberTermination GenerationNumberTermination(int maxIterations = 100)
        {
            if(maxIterations < 1)
            {
                throw new Exception("Invalid value. Maximum number of iterations cannot be lower than 1.");
            }
            if(maxIterations > DynamoGeneticAlgorithm.maxNumberOfIterations)
            {
                throw new Exception("Invalid value. Maximum number of iterations cannot be greater than {0}.".With(DynamoGeneticAlgorithm.maxNumberOfIterations));
            }
            return new GenerationNumberTermination(maxIterations);
        }

        /// <summary>Terminates genetic algorithm processing if given time elapsed.</summary>
        /// <param name="maxSeconds">A number of seconds to terminate processing.</param>
        /// <returns name="termination">Termination of genetic algorithm processing triggered by time elapsed.</returns>
        [NodeCategory("Create")]
        public static TimeEvolvingTermination TimeEvolvingTermination(int maxSeconds)
        {
            if (maxSeconds < 1)
            {
                throw new Exception("Invalid value. Number of seconds cannot be lower than 1.");
            }
            return new TimeEvolvingTermination(TimeSpan.FromSeconds(maxSeconds));
        }

        /// <summary>Combinates terminations and terminates genetic algorithm processing if all combined conditions are met.</summary>
        /// <param name="terminations">Terminations to combine.</param>
        /// <returns name="termination">Combined terminations.</returns>
        [NodeCategory("Action")]
        public static AndTermination AndTerminations(params object[] terminations)
        {
            List<ITermination> terminationList = new List<ITermination>();
            foreach(object t in terminations)
            {
                if(t is FitnessStagnationTermination)
                {
                    terminationList.Add(t as FitnessStagnationTermination);
                }
                else if (t is FitnessThresholdTermination)
                {
                    terminationList.Add(t as FitnessThresholdTermination);
                }
                else if (t is GenerationNumberTermination)
                {
                    terminationList.Add(t as GenerationNumberTermination);
                }
                else if (t is TimeEvolvingTermination)
                {
                    terminationList.Add(t as TimeEvolvingTermination);
                }
                else if (t is AndTermination)
                {
                    terminationList.Add(t as AndTermination);
                }
                else if (t is OrTermination)
                {
                    terminationList.Add(t as OrTermination);
                }
            }
            return new AndTermination(terminationList.ToArray());
        }

        /// <summary>Combinates terminations and terminates genetic algorithm processing if one of combined conditions is met.</summary>
        /// <param name="terminations">Terminations to combine.</param>
        /// <returns name="termination">Combined terminations.</returns>
        [NodeCategory("Action")]
        public static OrTermination OrTerminations(params object[] terminations)
        {
            List<ITermination> terminationList = new List<ITermination>();
            foreach (object t in terminations)
            {
                if (t is FitnessStagnationTermination)
                {
                    terminationList.Add(t as FitnessStagnationTermination);
                }
                else if (t is FitnessThresholdTermination)
                {
                    terminationList.Add(t as FitnessThresholdTermination);
                }
                else if (t is GenerationNumberTermination)
                {
                    terminationList.Add(t as GenerationNumberTermination);
                }
                else if (t is TimeEvolvingTermination)
                {
                    terminationList.Add(t as TimeEvolvingTermination);
                }
                else if (t is AndTermination)
                {
                    terminationList.Add(t as AndTermination);
                }
                else if (t is OrTermination)
                {
                    terminationList.Add(t as OrTermination);
                }
            }
            return new OrTermination(terminationList.ToArray());
        }

        /// <summary>Returns type of termination applied to a genetic algorithm instance.</summary>
        /// <param name="algorithm">A genetic algorithm instance to check for the termination method.</param>
        /// <returns name="termination">Type of termination applied to a genetic algorithm instance.</returns>
        [NodeCategory("Query")]
        public static object GetTermination(DynamoGeneticAlgorithm algorithm)
        {
            if (algorithm.Termination is FitnessStagnationTermination)
            {
                return algorithm.Termination as FitnessStagnationTermination;
            }
            if (algorithm.Termination is FitnessThresholdTermination)
            {
                return algorithm.Termination as FitnessThresholdTermination;
            }
            if (algorithm.Termination is GenerationNumberTermination)
            {
                return algorithm.Termination as GenerationNumberTermination;
            }
            if (algorithm.Termination is TimeEvolvingTermination)
            {
                return algorithm.Termination as TimeEvolvingTermination;
            }
            if (algorithm.Termination is AndTermination)
            {
                return algorithm.Termination as AndTermination;
            }
            if (algorithm.Termination is OrTermination)
            {
                return algorithm.Termination as OrTermination;
            }
            return null;
        }
    }
}