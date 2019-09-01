using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Randomizations;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Evo
{
    /// <summary>
    /// Implementation of the simulated binary crossover (SBX) for double chromosomes.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class SimulatedBinaryCrossover : CrossoverBase
    {
        /// <summary>Gets or sets a distribution index value. The index should be a non-negative number (small values allow solutions far away from parents to be created as children solutions).</summary>
        public double DistributionIndex { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SimulatedBinaryCrossover"/> class.</summary>
        /// <param name="distributionIndex">A non-negative value defining dispersion rate of offsprings in relation to parents.</param>
        public SimulatedBinaryCrossover(double distributionIndex) : base(2, 2)
        {
            IsOrdered = false;
            DistributionIndex = distributionIndex;
        }

        /// <summary>Performs the cross with specified parents generating the children.</summary>
        /// <param name="parents">The parents chromosomes.</param>
        /// <returns>The offspring (children) of the parents.</returns>
        protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
        {
            if(DistributionIndex < 0)
            {
                throw new CrossoverException(this, "Invalid distribution index value. Distribution index should be non-negative.");
            }

            DoubleChromosome firstParent = null;
            if (parents[0] is DoubleChromosome)
            {
                firstParent = parents[0] as DoubleChromosome;
            }
            DoubleChromosome secondParent = null;
            if (parents[1] is DoubleChromosome)
            {
                secondParent = parents[1] as DoubleChromosome;
            }

            if (firstParent == null || secondParent == null)
            {
                throw new CrossoverException(this, "The Simulated Binary Crossover (SBX) can be only used with double (floating point) chromosomes.");
            }

            double u = RandomizationProvider.Current.GetDouble();
            double beta = 0;
            if (u <= 0.5)
            {
                beta = Math.Pow(2 * u, 1 / (DistributionIndex + 1));
            }
            else
            {
                beta = Math.Pow(1 / (2 * (1 - u)), 1 / (DistributionIndex + 1));
            }

            var firstOffspring = firstParent.CreateNew();
            var secondOffspring = secondParent.CreateNew();
            for (int i = 0; i < firstOffspring.GetGenes().Count(); i++)
            {
                double firstParentGene = (double)firstParent.GetGenes()[i].Value;
                double secondParentGene = (double)secondParent.GetGenes()[i].Value;

                firstOffspring.ReplaceGene(i, new Gene(0.5 * (firstParentGene + secondParentGene - beta * Math.Abs(secondParentGene - firstParentGene))));
                secondOffspring.ReplaceGene(i, new Gene(0.5 * (firstParentGene + secondParentGene + beta * Math.Abs(secondParentGene - firstParentGene))));
            }
            return new List<IChromosome>() { firstOffspring, secondOffspring };
        }
    }
}