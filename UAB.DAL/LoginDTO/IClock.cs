using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
    public interface IClock
    {
		/// <summary>
		/// Encapsulates the current UTC time. Allows for injecting custom clocks to aid in unit testing.
		/// </summary>
			DateTime GetUtcNow();
		
	}
}
