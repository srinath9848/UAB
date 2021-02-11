using System;
using System.Linq;
using System.Security.Cryptography;

namespace UAB.DAL.LoginDTO
{
	/// <summary>
	/// Encapsulates the v1 password algorithm used by the IdentityServer. This algorithm uses the RFC2989 specification,
	/// using 128-bit salt and verifier values and 10,000 iterations of SHA-1.
	/// </summary>
	public class PasswordAlgorithm  : IPasswordAlgorithm
	{
		/// <summary>
		/// The password version number represented by the current algorithm.
		/// </summary>
		public const int VersionNumber = 1;

		/// <summary>
		/// The number of bytes in the randomly-generated salt value.
		/// </summary>
		public const int SaltSize = 16; // 128-bit

		/// <summary>
		/// The number of bytes in the calculated verifier value.
		/// </summary>
		public const int VerifierSize = 16; // 128-bit

		/// <summary>
		/// The number of iterations used to calculate the verifier.
		/// </summary>
		/// <remarks>The iterations used with the RFC2898 algorithm are meant to increase over time, to offset increasing hardware capabilities.
		/// The goal is to include enough iterations that it would significantly slow down a hacker (who needs to perform billions of runs),
		/// but still be fast enough to appear negligable to a valid user (who only needs a single run - to log in).
		/// <para>Here are some helpful yardsticks:
		/// <list type="bullet">
		///   <item>
		///     <description>September 2000 - RFC2898 recommends 1,000+ iterations.</description>
		///   </item>
		///   <item>
		///     <description>February 2005 - AES in Kerberos 5 'defaults' to 4,096 iterations of SHA-1.</description>
		///   </item>
		///   <item>
		///     <description>September 2010 - ElcomSoft claims iOS 3.x uses 2,000 iterations and iOS 4.x uses 10,000 iterations.
		///                                   BlackBerry used only 1 iteration for backups, which were subsequently cracked.</description>
		///   </item>
		///   <item>
		///     <description>May 2011 - LastPass uses 100,000 iterations of SHA-256.</description>
		///   </item>
		/// </list></para></remarks>
		public const int Iterations = 10000;

		/// <summary>
		/// Gets the password version number represented by the current algorithm.
		/// This should be a unique number for each implementation of <see cref="IPasswordAlgorithm"/>.
		/// </summary>
		public int Version
		{
			get { return VersionNumber; }
		}

		/// <summary>
		/// Encodes the given password into a new array of <see cref="Byte"/>.
		/// </summary>
		/// <param name="password">The password to be encoded.</param>
		/// <returns>A new <see cref="Byte"/> array that contains the encoded password.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <b>null</b>.</exception>
		/// <remarks>The array returned by this method has a variable length, depending on the algorithm used to encode it.</remarks>
		public byte[] EncodePassword(string password)
		{
			if (password == null)
				throw new ArgumentNullException("password");

			var result = new byte[SaltSize + VerifierSize];

			// Generate random salt value and embed it in the first portion of the result array
			var cryptoProvider = new RNGCryptoServiceProvider();
			var salt = new byte[SaltSize];
			cryptoProvider.GetNonZeroBytes(salt);
			salt.CopyTo(result, 0);

			// Calculate the verifier and embed it in the remaining portion of the result array
			var verifier = CalculateVerifier(password, salt);
			verifier.CopyTo(result, SaltSize);

			return result;
		}

		/// <summary>
		/// Compares the given password with the password encoded in the given <see cref="Byte"/> array, and returns a value indicating whether they are equal.
		/// </summary>
		/// <param name="encodedPassword">The encoded password that the given password is to be compared to.</param>
		/// <param name="givenPassword">The given password that is to be compared to the encoded password.</param>
		/// <returns><b>true</b> if the given password matches the encoded password; otherwise, <b>false</b>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="encodedPassword"/> is <b>null</b>.
		/// -or- <paramref name="givenPassword"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="encodedPassword"/> does not contain the correct number of bytes for the current algorithm.</exception>
		public bool IsValid(byte[] encodedPassword, string givenPassword)
		{
			if (encodedPassword == null)
				throw new ArgumentNullException("encodedPassword");
			if (encodedPassword.Length != SaltSize + VerifierSize)
				throw new ArgumentOutOfRangeException("encodedPassword", "The encoded password is not in the required format.");
			if (givenPassword == null)
				throw new ArgumentNullException("givenPassword");

			// Separate the salt and verifier values from the given encoded password
			var salt = new byte[SaltSize];
			var verifier = new byte[VerifierSize];
			Array.Copy(encodedPassword, 0, salt, 0, SaltSize);
			Array.Copy(encodedPassword, SaltSize, verifier, 0, VerifierSize);

			// Re-calculate the verifier and compare with the encoded verifier
			var newVerifier = CalculateVerifier(givenPassword, salt);
			return !verifier.Where((t, i) => t != newVerifier[i]).Any();
		}

		/// <summary>
		/// Performs the actual work of combining password and salt values and calculating the derived verifier.
		/// </summary>
		/// <param name="password">The password string to be included in the calculation.</param>
		/// <param name="salt">The array of random salt bytes to be included in the calculation.</param>
		/// <returns>A new array of bytes that contains the verifier.</returns>
		private static byte[] CalculateVerifier(string password, byte[] salt)
		{
			var engine = new Rfc2898DeriveBytes(password, salt, Iterations);
			return engine.GetBytes(VerifierSize);
		}
	}
}
