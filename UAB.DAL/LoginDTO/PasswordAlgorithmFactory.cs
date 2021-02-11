using System;
using System.Collections.Generic;
using System.Linq;
namespace UAB.DAL.LoginDTO
{
	public class PasswordAlgorithmFactory : IPasswordAlgorithmFactory
	{
		private readonly Dictionary<int, IPasswordAlgorithm> mAlgorithmsByVersion = new Dictionary<int, IPasswordAlgorithm>();

		/// <summary>
		/// Use this constructor only for testing
		/// </summary>
		/// <param name="algorithms"></param>
		public PasswordAlgorithmFactory(IEnumerable<IPasswordAlgorithm> algorithms)
		{
			if (algorithms == null)
				throw new ArgumentNullException("algorithms");

			foreach (var algorithm in algorithms)
			{
				mAlgorithmsByVersion.Add(algorithm.Version, algorithm);
			}
			if (!mAlgorithmsByVersion.Any())
				throw new ArgumentException("No algorithms suplied", "algorithms");
		}

		public IPasswordAlgorithm Latest
		{
			// Return the one with the largest version number
			get { return mAlgorithmsByVersion[mAlgorithmsByVersion.Keys.Max()]; }
		}

		public IPasswordAlgorithm GetFor(int version)
		{
			if (mAlgorithmsByVersion.ContainsKey(version))
				return mAlgorithmsByVersion[version];
			throw new ArgumentException("Unsupported version");
		}

		public bool IsLatest(int version)
		{
			// If it's the same as the largest version number of known algorithms
			return version == mAlgorithmsByVersion.Keys.Max();
		}
	}
}
