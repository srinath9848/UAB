using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	/// <summary>
	/// Contains the result codes that can be returned from an authentication operation.
	/// </summary>
	public enum AuthenticationResult
	{
		/// <summary>
		/// Indicates the authentication operation was successful.
		/// </summary>
		Valid = 0,

		/// <summary>
		/// Indicates the given email or password is not correct for the specified user.
		/// </summary>
		Invalid,

		/// <summary>
		/// Indicates that too many invalid passwords were given in a row, and the user is now temporarily locked out of the system.
		/// </summary>
		TooManyStrikes,

		/// <summary>
		/// Indicates that the credentials were correct, but the password has reached its expiration date and must be changed.
		/// </summary>
		Expired,
		UserNotFound
	}
}
