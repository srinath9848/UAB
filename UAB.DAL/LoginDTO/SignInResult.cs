using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	public class SignInResult
	{
		public AuthenticationResult Result { get; set; }
		public DateTime? LockedOutUntil { get; set; }
	}
}
