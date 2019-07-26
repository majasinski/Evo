using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

using Autodesk.DesignScript.Runtime;

namespace Evo
{
    /// <summary>
    /// Implementation of an ordered chromosome class usable for order-based problems, e.g. TSP.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class OrderedChromosome : ChromosomeBase
    {
        private readonly int NumberOfElements;
        //public double Fitness { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the OrderedChromosome class.
        /// </summary>
        /// <param name="numberOfElements">Number of unique genes in the chromosome.</param>
        public OrderedChromosome(int numberOfElements) : base(numberOfElements)
        {
            NumberOfElements = numberOfElements;

            var elements = RandomizationProvider.Current.GetUniqueInts(numberOfElements, 0, numberOfElements);
            for (int i = 0; i < numberOfElements; i++)
            {
                ReplaceGene(i, new Gene(elements[i]));
            }
        }

        /// <summary>
        /// Creates the new.
        /// </summary>
        /// <returns>The new.</returns>
        public override IChromosome CreateNew()
        {
            return new OrderedChromosome(NumberOfElements);
        }

        /// <summary>
        /// Generates the gene.
        /// </summary>
        /// <returns>The gene.</returns>
        /// <param name="geneIndex">Gene index.</param>
        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(RandomizationProvider.Current.GetInt(0, NumberOfElements));
        }

        /// <summary>
        /// Clones the chromosome.
        /// </summary>
        /// <returns>Cloned chromosome.</returns>
        public override IChromosome Clone()
        {
            var clone = base.Clone() as OrderedChromosome;
            clone.Fitness = Fitness;
            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current ordered chromosome.
        /// </summary>
        /// <returns>A string that represents the current ordered chromosome.</returns>
        public override string ToString()
        {
            return String.Join("", GetGenes().Select(g => g.Value.ToString()).ToArray());
        }
    }
}
