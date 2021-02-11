using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	/// <summary>
	/// A clock to implement the IClock interface in production code (as opposed to unit tests)
	/// </summary>
	public class Clock : IClock
	{
		/// <summary>
		/// Uses the system clock to get current UTC time.
		/// </summary>
		/// <returns>The current UTC time according to the system clock.</returns>
		public DateTime GetUtcNow()
		{
			return DateTime.UtcNow;
		}
	}
}
