using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using UAB.DAL.Models;
using System.Data;

namespace UAB.DAL.LoginDTO
{
    /// <summary>
    /// The standard implementation of <see cref="IAuthenticationService"/>.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPasswordAlgorithmFactory mPasswordAlgorithmFactory;
        private readonly IClock mClock;


        public AuthenticationService(IPasswordAlgorithmFactory passwordAlgorithmFactory, IClock clock)
        {
            mPasswordAlgorithmFactory = passwordAlgorithmFactory;
            mClock = clock;
        }

        public SignInResult SignIn(string email, string password)
        {
            UAB.DAL.LoginDTO.Users user = null;
            using (UAB.DAL.LoginDTO.IdentityServerContext context = new IdentityServerContext())
            {
                if (!string.IsNullOrWhiteSpace(email))
                    user = FindByEmail(email, context);

                if (user == null)
                {
                    return new SignInResult { Result = AuthenticationResult.UserNotFound };
                }

                var signInResult = Authenticate(user, email, password, "Sign-In");

                return signInResult;

            }

        }
        private SignInResult Authenticate(UAB.DAL.LoginDTO.Users user, string email, string password, string action)
        {
            using (UAB.DAL.LoginDTO.IdentityServerContext context = new IdentityServerContext())
            {
                #region Attempt
                UAB.DAL.LoginDTO.Attempts mdl = new UAB.DAL.LoginDTO.Attempts();
                mdl.UserId = user.UsersId;
                mdl.Email = email;
                mdl.Timestamp = mClock.GetUtcNow();
                mdl.Action = action;
                #endregion
                if (string.IsNullOrEmpty(user.Code))
                {
                    mdl.Result = AuthenticationResult.Invalid.ToString();
                    AddAttempts(mdl);
                    return new SignInResult { Result = AuthenticationResult.Invalid, LockedOutUntil = null };
                }
                var result = Validate(mPasswordAlgorithmFactory, mClock, email, password, 1);
                if (result == AuthenticationResult.Valid)
                {
                    mdl.Result = AuthenticationResult.Valid.ToString();
                }
                if (result == AuthenticationResult.Invalid)
                {
                    mdl.Result = AuthenticationResult.Invalid.ToString();
                }
                if (result == AuthenticationResult.TooManyStrikes)
                {
                    mdl.Result = AuthenticationResult.TooManyStrikes.ToString();
                }
                AddAttempts(mdl);

                return new SignInResult { Result = result, LockedOutUntil = user.LockoutFinish };
            }
        }

        public AuthenticationResult Validate(IPasswordAlgorithmFactory algorithmFactory, IClock clock, string email, string password, int version)
        {
            using (UAB.DAL.LoginDTO.IdentityServerContext context = new IdentityServerContext())
            {
                var user = context.Users.Where(a => a.Email == email).FirstOrDefault();

                if (algorithmFactory == null)
                    throw new ArgumentNullException("algorithmFactory");
                if (password == null)
                    throw new ArgumentNullException("password");
                if (clock == null)
                    throw new ArgumentNullException("clock");

                // If there are too many strikes, don't even try to validate the given password
                if (IsLocked(user, clock))
                {
                    return AuthenticationResult.TooManyStrikes;
                }
                else
                {
                    // ResetStrikes
                    user.Strikes = 0;
                    user.StrikeValue1 = null;
                    user.StrikeValue2 = null;
                    user.StrikeValue3 = null;
                    user.StrikeValue4 = null;
                    user.StrikeValue5 = null;
                    user.LockoutStart = null;
                    user.LockoutFinish = null;
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();

                }

                //byte[] data = Convert.FromBase64String("CA+wBy96ErVYSITpXmSHYsaHoT5uH4hZFdp2Q1IR7R4=");
                byte[] data = Convert.FromBase64String(user.Password);
                var algorithm = algorithmFactory.GetFor(version);
                if (algorithm.IsValid(data, password))
                {
                    return user.ExpirationDate <= clock.GetUtcNow()
                        ? AuthenticationResult.Expired
                        : AuthenticationResult.Valid;
                }

                // If the given password is not valid, increment the strikes
                #region IncrementStrikes

                if (user.Strikes < 5)
                {
                    user.Strikes = user.Strikes + 1;
                    if (user.Strikes == 1)
                        user.StrikeValue1 = clock.GetUtcNow();
                    if (user.Strikes == 2)
                        user.StrikeValue2 = clock.GetUtcNow();
                    if (user.Strikes == 3)
                        user.StrikeValue3 = clock.GetUtcNow();
                    if (user.Strikes == 4)
                        user.StrikeValue4 = clock.GetUtcNow();
                    if (user.Strikes == 5)
                        user.StrikeValue5 = clock.GetUtcNow();
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();
                }

                if (user.Strikes == 5 && user.LockoutStart == null)
                {
                    user.LockoutStart = clock.GetUtcNow();
                    user.LockoutFinish = clock.GetUtcNow().AddHours(1);
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();
                    var IsLocked = user.LockoutStart.HasValue && user.LockoutFinish > clock.GetUtcNow();
                    if (IsLocked)
                    {
                        return AuthenticationResult.TooManyStrikes;
                    }
                }
                #endregion
                return user.LockoutStart.HasValue ? AuthenticationResult.TooManyStrikes : AuthenticationResult.Invalid;
            }
        }

        public UAB.DAL.LoginDTO.Users FindByEmail(string email, UAB.DAL.LoginDTO.IdentityServerContext context)
        {
            return context.Users.Where(a => a.Email == email).FirstOrDefault();
        }
        public class UserInfo
        {
            public int UserId { get; set; }
            public string Email { get; set; }
            public bool IsActiveUser { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public int ProjectId { get; set; }
            public bool IsActiveInProject { get; set; }
        }

        public UserInfo GetUserInfoByEmail(string Email)
        {
            UserInfo userInfo = new UserInfo();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetUserInfo]";
                    cmm.Connection = cnn;
                    cnn.Open();

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@Email";
                    param.Value = Email;
                    cmm.Parameters.Add(param);


                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        userInfo = new UserInfo();
                        userInfo.UserId = Convert.ToInt32(reader["UserId"]);
                        userInfo.Email = reader["Email"].ToString();
                        //userInfo.IsActiveUser = Convert.ToBoolean(reader["IsActive"]);
                        //userInfo.RoleId = Convert.ToInt32(reader["RoleId"]);
                        userInfo.RoleName = reader["RoleName"].ToString();
                        //userInfo.ProjectId = Convert.ToInt32(reader["ProjectId"]);
                        // userInfo.IsActiveInProject = Convert.ToBoolean(reader["IsActive"]);

                    }
                }
            }
            return userInfo;
        }

        public bool HasMetRequirements(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            const string passwordRequirementsRegEx = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            return Regex.IsMatch(password, passwordRequirementsRegEx);
        }

        private bool UsedRecently(string password, string Oldpassword)
        {
            if (Oldpassword == password)
                return true;

            return false;
        }

        public void AddAttempts(UAB.DAL.LoginDTO.Attempts mdl)
        {
            using (UAB.DAL.LoginDTO.IdentityServerContext context = new UAB.DAL.LoginDTO.IdentityServerContext())
            {
                context.Attempts.Add(mdl);
                context.SaveChanges();
            }
        }
        private bool IsLocked(UAB.DAL.LoginDTO.Users user, IClock clock)
        {
            return user.LockoutStart.HasValue && user.LockoutFinish > clock.GetUtcNow();
        }

    }
}
