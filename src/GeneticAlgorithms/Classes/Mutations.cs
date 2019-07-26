using System;
using System.Collections;
using System.Collections.Generic;

using Evo;
using GeneticSharp.Domain.Mutations;

using Dynamo.Graph.Nodes;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Includes methods responsible for definition of mutation classes.
    /// Mutation, besides selection and crossover, is one of the main operators in genetic algorithms.
    /// </summary>
    public static class Mutations
    {
        /// <summary>
        /// Defines a mutation instance: Displacement Mutation.
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Displacement Mutation.
        /// </returns>
        [NodeCategory("Create")]
        public static DisplacementMutation DisplacementMutation()
        {
            return new DisplacementMutation();
        }

        /// <summary>
        /// Defines a mutation instance: Flip Bit Mutation.
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Flip Bit Mutation.
        /// </returns>
        [NodeCategory("Create")]
        public static FlipBitMutation FlipBitMutation()
        {
            return new FlipBitMutation();
        }

        /// <summary>
        /// Defines a mutation instance: Insertion Mutation.
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Insertion Mutation.
        /// </returns>
        [NodeCategory("Create")]
        public static InsertionMutation InsertionMutation()
        {
            return new InsertionMutation();
        }

        /// <summary>
        /// Defines a mutation instance: Partial Shuffle Mutation (PSM).
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Partial Shuffle Mutation (PSM).
        /// </returns>
        [NodeCategory("Create")]
        public static PartialShuffleMutation PartialShuffleMutation()
        {
            return new PartialShuffleMutation();
        }

        /// <summary>
        /// Defines a mutation instance: Reverse Sequence Mutation (RSM).
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Reverse Sequence Mutation (RSM).
        /// </returns>
        [NodeCategory("Create")]
        public static ReverseSequenceMutation ReverseSequenceMutation()
        {
            return new ReverseSequenceMutation();
        }

        /// <summary>
        /// Defines a mutation instance: Twors Mutation.
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Twors Mutation.
        /// </returns>
        [NodeCategory("Create")]
        public static TworsMutation TworsMutation()
        {
            return new TworsMutation();
        }

        /// <summary>
        /// Defines a mutation instance: Uniform Mutation.
        /// </summary>
        /// <returns name="mutation">
        /// A mutation instance: Uniform Mutation.
        /// </returns>
        [NodeCategory("Create")]
        public static UniformMutation UniformMutation()
        {
            return new UniformMutation();
        }

        /// <summary>
        /// Returns type of mutation applied to a genetic algorithm instance.
        /// </summary>
        /// <param name="algorithm">A genetic algorithm instance to check for the mutation method.</param>
        /// <returns name="mutation">
        /// Type of mutation applied to a genetic algorithm instance.
        /// </returns>
        [NodeCategory("Query")]
        public static object GetMutation(DynamoGeneticAlgorithm algorithm)
        {
            if (algorithm.Mutation is DisplacementMutation)
            {
                return algorithm.Mutation as DisplacementMutation;
            }
            if (algorithm.Mutation is FlipBitMutation)
            {
                return algorithm.Mutation as FlipBitMutation;
            }
            if (algorithm.Mutation is InsertionMutation)
            {
                return algorithm.Mutation as InsertionMutation;
            }
            if (algorithm.Mutation is PartialShuffleMutation)
            {
                return algorithm.Mutation as PartialShuffleMutation;
            }
            if (algorithm.Mutation is ReverseSequenceMutation)
            {
                return algorithm.Mutation as ReverseSequenceMutation;
            }
            if (algorithm.Mutation is TworsMutation)
            {
                return algorithm.Mutation as TworsMutation;
            }
            if (algorithm.Mutation is UniformMutation)
            {
                return algorithm.Mutation as UniformMutation;
            }
            return null;
        }

        /// <summary>
        /// Returns mutation probability for a genetic algorithm instance.
        /// </summary>
        /// <param name="algorithm">A genetic algorithm instance to check for mutation probability.</param>
        /// <returns name="mutationProbability">
        /// Mutation probability for a genetic algorithm instance.
        /// </returns>
        [NodeCategory("Query")]
        public static double GetMutationProbability(DynamoGeneticAlgorithm algorithm)
        {
            return algorithm.MutationProbability;
        }
    }
}