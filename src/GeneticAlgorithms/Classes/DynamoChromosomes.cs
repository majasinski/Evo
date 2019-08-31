using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

using Autodesk.DesignScript.Runtime;
using GeneticSharp.Infrastructure.Framework.Commons;
using GeneticSharp.Infrastructure.Framework.Texts;

namespace Evo
{
    /// <summary>
    /// Chromosome type enumerators.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public enum ChromosomeType
    {
        /// <summary>Binary chromosome.</summary>
        BinaryChromosome,
        /// <summary>Combinatorial chromosome.</summary>
        CombinatorialChromosome,
        /// <summary>Double chromosome.</summary>
        DoubleChromosome
    }

    /// <summary>
    /// Implementation of a binary chromosome class.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class BinaryChromosome : BinaryChromosomeBase
    {
        private readonly double[] MinValues;
        private readonly double[] MaxValues;
        private readonly int[] TotalBits;
        private readonly int[] FractionDigits;
        private readonly string OriginalValueStringRepresentation;

        /// <summary>Chromosome type.</summary>
        public ChromosomeType ChromosomeType { get; private set; }

        /// <summary>Initializes a new instance of the BinaryChromosome class.</summary>
        /// <param name="minValues">Minimum values.</param>
        /// <param name="maxValues">Maximum values.</param>
        /// <param name="totalBits">Total bits.</param>
		/// <param name="fractionDigits">Decimals.</param>
        /// <param name="genes">Genes.</param>
        public BinaryChromosome(double[] minValues, double[] maxValues, int[] totalBits, int[] fractionDigits, double[] genes) : base(totalBits.Sum())
        {
            MinValues = minValues;
            MaxValues = maxValues;
            TotalBits = totalBits;
            FractionDigits = fractionDigits;

            ChromosomeType = ChromosomeType.BinaryChromosome;

            // If values are not supplied, create random values
            if (genes == null)
            {
                genes = new double[minValues.Length];
                var rnd = RandomizationProvider.Current;

                for (int i = 0; i < genes.Length; i++)
                {
                    genes[i] = rnd.GetDouble(minValues[i], maxValues[i]);
                }
            }
            OriginalValueStringRepresentation = string.Join("", BinaryStringRepresentation.ToRepresentation(genes, totalBits, fractionDigits));
            CreateGenes();
        }

        /// <summary>Creates a new chromosome.</summary>
        /// <returns>A new chromosome.</returns>
        public override IChromosome CreateNew()
        {
            return new BinaryChromosome(MinValues, MaxValues, TotalBits, FractionDigits, null);
        }

        /// <summary>Generates a gene.</summary>
        /// <param name="geneIndex">Gene index.</param>
        /// <returns>The gene.</returns>
        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(Convert.ToInt32(OriginalValueStringRepresentation[geneIndex].ToString()));
        }

        /// <summary>Converts the chromosome to the floating point representation.</summary>
        /// <returns>The floating point.</returns>
        public double ToFloatingPoint()
        {
            return EnsureMinMax(BinaryStringRepresentation.ToDouble(string.Join("", GetGenes().Select(g => g.Value.ToString()).ToArray())), 0);
        }
        private double EnsureMinMax(double value, int index)
        {
            if (value < MinValues[index])
            {
                return MinValues[index];
            }
            if (value > MaxValues[index])
            {
                return MaxValues[index];
            }
            return value;
        }

        /// <summary>Converts the chromosome to the floating points representation.</summary>
        /// <returns>The floating points.</returns>
        public double[] ToFloatingPoints()
        {
            return BinaryStringRepresentation.ToDouble(string.Join("", GetGenes().Select(g => g.Value.ToString()).ToArray()), TotalBits, FractionDigits).Select(EnsureMinMax).ToArray();
        }

        /// <summary>Returns a string that represents the current binary chromosome.</summary>
        /// <returns>A string that represents the current binary chromosome.</returns>
        public override string ToString()
        {
            string binaryRepresentation = "[" + string.Join("", GetGenes().Select(g => g.Value.ToString()).ToArray());

            int inserted = 1;
            foreach (int bits in TotalBits)
            {
                binaryRepresentation = binaryRepresentation.Substring(0, inserted + bits) + "][" + binaryRepresentation.Substring(inserted + bits);
                inserted = inserted + bits + 2;
            }
            return binaryRepresentation.Remove(binaryRepresentation.Length - 1);
        }
    }

    /// <summary>
    /// Implementation of a combinatorial chromosome class usable for order-based problems, e.g. TSP.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class CombinatorialChromosome : ChromosomeBase
    {
        private readonly int NumberOfElements;

        /// <summary>Chromosome type.</summary>
        public ChromosomeType ChromosomeType { get; private set; }

        /// <summary>Initializes a new instance of the CombinatorialChromosome class.</summary>
        /// <param name="numberOfElements">Number of unique genes in the chromosome.</param>
        public CombinatorialChromosome(int numberOfElements) : base(numberOfElements)
        {
            NumberOfElements = numberOfElements;
            ChromosomeType = ChromosomeType.CombinatorialChromosome;

            var elements = RandomizationProvider.Current.GetUniqueInts(numberOfElements, 0, numberOfElements);
            for (int i = 0; i < numberOfElements; i++)
            {
                ReplaceGene(i, new Gene(elements[i]));
            }
        }

        /// <summary>Creates a new chromosome.</summary>
        /// <returns>A new chromosome.</returns>
        public override IChromosome CreateNew()
        {
            return new CombinatorialChromosome(NumberOfElements);
        }

        /// <summary>Generates a gene.</summary>
        /// <param name="geneIndex">Gene index.</param>
        /// <returns>The gene.</returns>
        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(RandomizationProvider.Current.GetInt(0, NumberOfElements));
        }

        /// <summary>Returns a string that represents the current combinatorial chromosome.</summary>
        /// <returns>A string that represents the current combinatorial chromosome.</returns>
        public override string ToString()
        {
            return string.Join("", GetGenes().Select(g => "[" + g.Value.ToString() + "]").ToArray());
        }
    }

    /// <summary>
    /// Implementation of a double (floating point) chromosome class.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public sealed class DoubleChromosome : IChromosome
    {
        public readonly double[] MinValues;
        public readonly double[] MaxValues;
        private int ChromosomeLength;
        private Gene[] Genes;

        /// <summary>Chromosome type.</summary>
        public ChromosomeType ChromosomeType { get; private set; }

        /// <summary>Initializes a new instance of the DoubleChromosome class.</summary>
        /// <param name="minValues">Minimum values.</param>
        /// <param name="maxValues">Maximum values.</param>
        public DoubleChromosome(double[] minValues, double[] maxValues)
        {
            MinValues = minValues;
            MaxValues = maxValues;

            ChromosomeType = ChromosomeType.DoubleChromosome;

            ChromosomeLength = minValues.Count();
            Genes = new Gene[ChromosomeLength];

            var rnd = RandomizationProvider.Current;
            for (int i = 0; i < ChromosomeLength; i++)
            {
                ReplaceGene(i, new Gene(rnd.GetDouble(minValues[i], maxValues[i])));
            }
        }

        /// <summary>Gets or sets the fitness of the chromosome in the current problem.</summary>
        public double? Fitness { get; set; }
        /// <summary>Gets the length, in genes, of the chromosome.</summary>
        public int Length
        {
            get
            {
                return ChromosomeLength;
            }
        }

        /// <summary>Creates a new chromosome.</summary>
        /// <returns>A new chromosome.</returns>
        public IChromosome CreateNew()
        {
            return new DoubleChromosome(MinValues, MaxValues);
        }

        /// <summary>Creates a clone.</summary>
        /// <returns>The chromosome clone.</returns>
        public IChromosome Clone()
        {
            var clone = CreateNew();
            clone.ReplaceGenes(0, GetGenes());
            clone.Fitness = Fitness;

            return clone;
        }

        /// <summary>Compares the current object with another object of the same type.</summary>
        /// <param name="other">The other chromosome.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(IChromosome other)
        {
            if (other == null)
            {
                return -1;
            }
        
            var otherFitness = other.Fitness;
            if (Fitness == otherFitness)
            {
                return 0;
            }
            return Fitness > otherFitness ? 1 : -1;
        }

        /// <summary>Replaces the gene in the specified index.</summary>
        /// <param name="index">The gene index to replace.</param>
        /// <param name="gene">The new gene.</param>
        public void ReplaceGene(int index, Gene gene)
        {
            if ((index < 0) || (index >= ChromosomeLength))
            {
                throw new ArgumentOutOfRangeException(nameof(index), "There is no Gene on index {0} to be replaced.".With(index));
            }
            Genes[index] = gene;
            Fitness = null;
        }
        /// <summary>Replaces the genes starting in the specified index.</summary>
        /// <param name="startIndex">Start index.</param>
        /// <param name="genes">The genes.</param>
        public void ReplaceGenes(int startIndex, Gene[] genes)
        {
            ExceptionHelper.ThrowIfNull("genes", genes);

            if (genes.Length > 0)
            {
                if ((startIndex < 0) || (startIndex >= ChromosomeLength))
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), "There is no Gene on index {0} to be replaced.".With(startIndex));
                }

                var genesToBeReplacedLength = genes.Length;
                var availableSpaceLength = ChromosomeLength - startIndex;

                if (genesToBeReplacedLength > availableSpaceLength)
                {
                    throw new ArgumentException(nameof(Gene), "The number of genes to be replaced is greater than available space, there is {0} genes between the index {1} and the end of chromosome, but there is {2} genes to be replaced.".With(availableSpaceLength, startIndex, genesToBeReplacedLength));
                }
                Array.Copy(genes, 0, Genes, startIndex, genes.Length);
                Fitness = null;
            }
        }

        /// <summary>Resizes the chromosome to the new length.</summary>
        /// <param name="newLength">The new length.</param>
        public void Resize(int newLength)
        {
            Array.Resize(ref Genes, newLength);
            ChromosomeLength = newLength;
        }

        /// <summary>Generates a gene.</summary>
        /// <param name="geneIndex">Gene index.</param>
        /// <returns>The gene.</returns>
        public Gene GenerateGene(int geneIndex)
        {
            return new Gene(RandomizationProvider.Current.GetDouble(MinValues[geneIndex], MaxValues[geneIndex]));
        }

        /// <summary> Gets the gene in the specified index.</summary>
        /// <param name="index">The gene index.</param>
        /// <returns>The gene.</returns>
        public Gene GetGene(int index)
        {
            return Genes[index];
        }

        /// <summary>Gets the genes.</summary>
        /// <returns>The genes.</returns>
        public Gene[] GetGenes()
        {
            return Genes;
        }

        /// <summary>Returns a string that represents the current double chromosome.</summary>
        /// <returns>A string that represents the current double chromosome.</returns>
        public override string ToString()
        {
            return string.Join("", GetGenes().Select(g => "[" + ((double) g.Value).ToString("F3") + "]").ToArray());
        }
    }
}