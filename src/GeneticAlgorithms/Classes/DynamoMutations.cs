using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Evo;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Randomizations;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Evo
{
    /// <summary>
    /// Implementation of the gaussian mutation for double chromosomes.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class GaussianMutation : MutationBase
    {
        /// <summary>Gets or sets a strength parameter used in Gaussian mutation operator. By reference paper (Deb, K. et al. 2014), taken as 1/30.</summary>
        public double StrengthParameter { get; set; }

        /// <summary>Initializes a new instance of the <see cref="GaussianMutation"/> class.</summary>
        /// <param name="strengthParameter">A strength parameter.</param>
        public GaussianMutation(double strengthParameter)
        {
            StrengthParameter = strengthParameter;
            IsOrdered = false;
        }

        /// <summary>Calculates Gauss error function.</summary>
        /// <param name="x">A value.</param>
        /// <returns>Gauss error function value.</returns>
        private static double Erf(double x)
        {
            int sign = 1;
            if (x < 0)
            {
                sign = -1;
            }
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + 0.3275911 * x);
            double erfX = 1.0 - (((((1.061405429 * t - 1.453152027) * t) + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t * Math.Exp(-x * x);

            return sign * erfX;
        }

        /// <summary>Mutate the specified chromosome.</summary>
        /// <param name="chromosome">The chromosome.</param>
        /// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if(!(chromosome is DoubleChromosome))
            {
                throw new MutationException(this, "The Gaussian Mutation can be only used with double (floating point) chromosomes.");
            }

            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                Gene[] genes = chromosome.GetGenes();

                double u = RandomizationProvider.Current.GetDouble();
                double alfa = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));

                if (u <= 0.5)
                {
                    for (int i = 0; i < genes.Count(); i++)
                    {
                        double geneValue = (double) genes[i].Value;
                        double uBound = 0.5 * (Erf((((DoubleChromosome) chromosome).MinValues[i] - geneValue) / (Math.Sqrt(2) * (((DoubleChromosome) chromosome).MaxValues[i] - ((DoubleChromosome) chromosome).MinValues[i]) * StrengthParameter)) + 1);

                        u = 2 * uBound * (1 - 2 * u);
                        int sign = u >= 0 ? 1 : -1;

                        var erf = sign * Math.Sqrt(Math.Sqrt(Math.Pow(2 / (Math.PI * alfa) + Math.Log(1 - Math.Pow(u, 2) / 2), 2) - Math.Log(1 - Math.Pow(u, 2)) / alfa) - (2 / (Math.PI * alfa) + Math.Log(1 - Math.Pow(u, 2)) / 2));

                        chromosome.ReplaceGene(i, new Gene(geneValue + Math.Sqrt(2) * StrengthParameter * (((DoubleChromosome) chromosome).MaxValues[i] - ((DoubleChromosome) chromosome).MinValues[i]) * erf));
                    }
                }
                else
                {
                    for (int i = 0; i < genes.Count(); i++)
                    {
                        double geneValue = (double) genes[i].Value;
                        double uBound = 0.5 * (Erf((((DoubleChromosome) chromosome).MaxValues[i] - geneValue) / (Math.Sqrt(2) * (((DoubleChromosome) chromosome).MaxValues[i] - ((DoubleChromosome) chromosome).MinValues[i]) * StrengthParameter)) + 1);

                        u = 2 * uBound * (2 * u - 1);
                        int sign = u >= 0 ? 1 : -1;

                        var erf = sign * Math.Sqrt(Math.Sqrt(Math.Pow(2 / (Math.PI * alfa) + Math.Log(1 - Math.Pow(u, 2) / 2), 2) - Math.Log(1 - Math.Pow(u, 2)) / alfa) - (2 / (Math.PI * alfa) + Math.Log(1 - Math.Pow(u, 2)) / 2));

                        chromosome.ReplaceGene(i, new Gene(geneValue + Math.Sqrt(2) * StrengthParameter * (((DoubleChromosome) chromosome).MaxValues[i] - ((DoubleChromosome) chromosome).MinValues[i]) * erf));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Implementation of the polynomial mutation for double chromosomes.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class PolynomialMutation : MutationBase
    {
        /// <summary>Gets or sets a polynomial probability distribution value. It is recommended to set this value in range between 20 and 100, adequate in most engineering problems.</summary>
        public double DistributionIndex { get; set; }

        /// <summary>Initializes a new instance of the <see cref="PolynomialMutation"/> class.</summary>
        /// <param name="distributioIndex">A polynomial probability distribution (values of 20 - 100 are recommended).</param>
        public PolynomialMutation(double distributioIndex)
        {
            DistributionIndex = distributioIndex;
            IsOrdered = false;
        }

        /// <summary>Mutate the specified chromosome.</summary>
        /// <param name="chromosome">The chromosome.</param>
        /// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            if (!(chromosome is DoubleChromosome))
            {
                throw new MutationException(this, "The Polynomial Mutation can be only used with double (floating point) chromosomes.");
            }

            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                Gene[] genes = chromosome.GetGenes();

                double u = RandomizationProvider.Current.GetDouble();
                if (u <= 0.5)
                {
                    for (int i = 0; i < genes.Count(); i++)
                    {
                        double geneValue = (double)genes[i].Value;

                        var delta = Math.Pow(2 * u, 1 / (DistributionIndex + 1)) - 1;
                        chromosome.ReplaceGene(i, new Gene(geneValue + delta * (geneValue - ((DoubleChromosome)chromosome).MinValues[i])));
                    }
                }
                else
                {
                    for (int i = 0; i < genes.Count(); i++)
                    {
                        double geneValue = (double)genes[i].Value;

                        var delta = 1 - Math.Pow(2 * (1 - u), 1 / (DistributionIndex + 1));
                        chromosome.ReplaceGene(i, new Gene(geneValue + delta * (((DoubleChromosome)chromosome).MaxValues[i] - geneValue)));
                    }
                }
            }
        }
    }
}