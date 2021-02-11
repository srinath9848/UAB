using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	/// <summary>
	/// Defines a password algorithm strategy that can be updated over time to keep up with current security trends.
	/// </summary>
	public interface IPasswordAlgorithm
	{
		/// <summary>
		/// Gets the password version number represented by the current algorithm.
		/// This should be a unique number for each implementation of <see cref="IPasswordAlgorithm"/>.
		/// </summary>
		int Version { get; }

		/// <summary>
		/// Encodes the given password into a new array of <see cref="Byte"/>.
		/// </summary>
		/// <param name="password">The password to be encoded.</param>
		/// <returns>A new <see cref="Byte"/> array that contains the encoded password.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <b>null</b>.</exception>
		/// <remarks>The array returned by this method has a variable length, depending on the algorithm used to encode it.</remarks>
		byte[] EncodePassword(string password);

		/// <summary>
		/// Compares the given password with the password encoded in the given <see cref="Byte"/> array, and returns a value indicating whether they are equal.
		/// </summary>
		/// <param name="encodedPassword">The encoded password that the given password is to be compared to.</param>
		/// <param name="givenPassword">The given password that is to be compared to the encoded password.</param>
		/// <returns><b>true</b> if the given password matches the encoded password; otherwise, <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="encodedPassword"/> is <b>null</b>.
		/// -or- <paramref name="givenPassword"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="encodedPassword"/> does not contain the correct number of bytes for the current algorithm.</exception>
		bool IsValid(byte[] encodedPassword, string givenPassword);
	}
}
