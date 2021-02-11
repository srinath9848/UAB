using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	public enum ChangePasswordResult
	{
		Success,
		UserNotFound,
		CodeNotVerified,
		IncorrectCurrentPassword,
		PasswordInvalid,
		PasswordUsedRecently
	}
}
