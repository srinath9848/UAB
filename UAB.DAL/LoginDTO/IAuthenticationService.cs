using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
	public interface IAuthenticationService1
	{
		/// <summary>
		/// Verifies whether or not the given password is valid for the user identified by the given email.
		/// </summary>
		/// <param name="email">Email of the user to authenticate.</param>
		/// <param name="password">Password to check for authenticity.</param>
		/// <returns>The <see cref="SignInResult"/> that contains an enum that indicates success/failure/other cases and the <see cref="StrikeCollection" /> containing failed attempt information</returns>
		SignInResult SignIn(string email, string password);
        AuthenticationService.UserInfo GetUserInfoByEmail(string Email);

		/// <summary>
		/// Attempts to change the password of the user identified by the given email.  Authentication will NOT be performed before changing password.
		/// </summary>
		/// <param name="email">Email of the user.</param>
		/// <param name="newPassword">New password to change user's password to.</param>
		/// <returns>The <see cref="ChangePasswordResult"/> that indicates the outcome of changing the password.</returns>
		//ChangePasswordResult ChangePassword(string email, string newPassword);

		/// <summary>
		/// Attempts to change the password of the user identified by the given email. Authentication will be performed before changing password.
		/// </summary>
		/// <param name="email">Email of the user.</param>
		/// <param name="newPassword">New password to change user's password to.</param>
		/// <param name="currentPassword">Current password of the user.</param>
		/// <returns>The <see cref="ChangePasswordResult"/> that indicates the outcome of changing the password.</returns>
		//ChangePasswordResult ChangePasswordAndAuthenticate(string email, string newPassword, string currentPassword);

		/// <summary>
		/// Adds an EmailVerification onto the user.  Sends user an email with a generated six digit alphanumeric code.
		/// </summary>
		/// <param name="user">User to generate code and send email for.</param>
		//SendEmailVerificationResult SendInviteEmailVerificationCode(User user);

		/// <summary>
		/// Adds an EmailVerification onto the user.  Sends user an email with a generated six digit alphanumeric code.
		/// </summary>
		/// <param name="email">Email of the user.</param>
		//SendEmailVerificationResult SendPasswordEmailVerificationCode(string email);

		/// <summary>
		/// Finds the user by the EmailVerification code provided.  Sets HasBeenVerfied to true.  Authenticates the user and sets the authentication cookie.
		/// </summary>
		/// <param name="code">Verification.</param>
		/// <param name="email"></param>
		/// <param name="shouldAuthenticate"></param>
		//VerifyEmailCodeResult VerifyEmailCode(string code, string email, bool shouldAuthenticate);
	}
}
