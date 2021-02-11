using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	public interface IPasswordAlgorithmFactory
	{
		IPasswordAlgorithm Latest { get; }
		IPasswordAlgorithm GetFor(int version);
		bool IsLatest(int version);
	}
}
