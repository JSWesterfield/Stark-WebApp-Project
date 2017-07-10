using stark.Data;
using stark.Web.Services.Interfaces;
using stark.Web.Domain;
using stark.Web.Models.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;

namespace stark.Web.Services
{
    public class UsersService : BaseService, IUsersService, IMiniUserResultMapper 
    {
        private IMessagingService _messagingService;
        private ICryptographyService _cryptographyService;
        private IConfigurationService _configurationService;
        private IPostService _postsService;

        public UsersService(IMessagingService messagingService, ICryptographyService cryptographyService, IConfigurationService configurationService, IPostService postsService)
        {
            _messagingService = messagingService;
            _cryptographyService = cryptographyService;
            _configurationService = configurationService;
            _postsService = postsService;

        }

        private void PasswordHasher(string password, out string salt, out string passwordHash)
        {
            salt = _cryptographyService.GenerateRandomString(15);
            passwordHash = _cryptographyService.Hash(password, salt, 1);
        }

        //Int is nullable in order to allow sql error catch to return null
        public int? Create(UserCreateRequest model)
        {
            int id = 0;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                string salt;
                string passwordHash;
                PasswordHasher(model.Password, out salt, out passwordHash);

                parameters.AddWithValue("@Username", model.Username);

                parameters.AddWithValue("@Email", model.Email);

                parameters.AddWithValue("@FirstName", model.FirstName);

                parameters.AddWithValue("@LastName", model.LastName);

                parameters.AddWithValue("@Blurb", model.Blurb ?? (object)DBNull.Value);

                parameters.AddWithValue("@ImageUrl", model.ImageUrl ?? (object)DBNull.Value);

                parameters.AddWithValue("@Password", passwordHash);

                parameters.AddWithValue("@Salt", salt);

                SqlParameter idParam = new SqlParameter("@Id", id);
                idParam.Direction = ParameterDirection.Output;
                parameters.Add(idParam);
            };

            Action<SqlParameterCollection> returnMapper = delegate (SqlParameterCollection parameters)
            {
                id = (int)parameters["@Id"].Value;
            };

            try
            {
                DataProvider.ExecuteNonQuery(GetConnection,
                "dbo.Users_Insert",
                inputMapper,
                returnMapper);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2601)
                    return null;
                else
                    throw;
            }

            var fullName = model.FirstName + " " + model.LastName;
            _messagingService.SendEmailConfirmationEmail(model.Email, fullName, id);
            
            return id;
        }

        public List<User> GetAll()
        {
            List<User> list = null;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                //none
            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                User newUser = GetAllProps(reader);

                if (list == null)
                {
                    list = new List<User>();
                }
                list.Add(newUser);
            };

            DataProvider.ExecuteCmd(
                GetConnection,
                "dbo.Users_SelectAll",
                inputMapper,
                resultMapper
            );

            return list;

        }

        public List<User> GetAllAdmin()
        {
            List<User> list = null;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                //none
            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                User newUser = GetAllProps(reader);

                if (list == null)
                {
                    list = new List<User>();
                }
                list.Add(newUser);
            };

            DataProvider.ExecuteCmd(
                GetConnection,
                "dbo.Users_SelectAllAdmin",
                inputMapper,
                resultMapper
            );

            return list;

        }

        private User GetAllProps(IDataReader reader)
        {
            User newUser = new User();

            int startingIndex = 0;
            newUser.Id = reader.GetInt32(startingIndex++);
            newUser.Username = reader.GetString(startingIndex++);
            newUser.Email = reader.GetString(startingIndex++);
            newUser.FirstName = reader.GetString(startingIndex++);
            newUser.LastName = reader.GetString(startingIndex++);
            newUser.Blurb = reader.GetSafeString(startingIndex++);
            newUser.ImageUrl = reader.GetSafeString(startingIndex++);
            newUser.DateCreated = reader.GetDateTime(startingIndex++);
            newUser.DateModified = reader.GetDateTime(startingIndex++);
            newUser.DateDeactivated = reader.GetSafeDateTimeNullable(startingIndex++);
            newUser.IsEmailConfirmed = reader.GetBoolean(startingIndex++);
            newUser.Salt = reader.GetString(startingIndex++);
            newUser.IsAdmin = reader.GetBoolean(startingIndex++);

            List<Post> posts = new List<Post>();
            int postCount = posts.Count();
            postCount = newUser.PostCount;
            posts = _postsService.GetRecentByUserId(newUser.Id, "dateCreated", int.MaxValue); //(int userId, string order, int limit);

            return newUser;

            
        }

        public void Update(UserUpdateRequest model)
        {
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {

                parameters.AddWithValue("@Username", model.Username);

                parameters.AddWithValue("@Email", model.Email);

                parameters.AddWithValue("@FirstName", model.FirstName);

                parameters.AddWithValue("@LastName", model.LastName);

                parameters.AddWithValue("@Blurb", model.Blurb ?? (object)DBNull.Value);

                parameters.AddWithValue("@ImageUrl", model.ImageUrl ?? (object)DBNull.Value);

                parameters.AddWithValue("@Id", model.Id);
            };

            Action<SqlParameterCollection> returnMapper = delegate (SqlParameterCollection parameters)
            {

            };

            DataProvider.ExecuteNonQuery(
                GetConnection,
                "dbo.Users_Update",
                inputMapper,
                returnMapper);
        }

        public User GetById(int Id)
        {
            User newUser = null;
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@Id", Id);
            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                newUser = GetAllProps(reader);
            };

            DataProvider.ExecuteCmd(
                GetConnection,
                "dbo.Users_SelectById",
                inputMapper,
                resultMapper
            );

            return newUser;
        }

        public void Delete(int Id)
        {
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@Id", Id);
            };
            Action<SqlParameterCollection> returnMapper = delegate (SqlParameterCollection parameters)
            {

            };

            DataProvider.ExecuteNonQuery(
                GetConnection,
                "dbo.Users_Deactivate",
                inputMapper,
                returnMapper);
        }

        public User GetByUsernameAndPassword(string username, string password)
        {
            User testUser = null;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@Username", username);
                parameters.AddWithValue("@Password", password);
            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                testUser = GetAllProps(reader);
            };

            DataProvider.ExecuteCmd(
                GetConnection,
                "dbo.Users_SelectByUsernameAndPassword",
                inputMapper,
                resultMapper
            );

            return testUser;
        }

        public void ConfirmEmail(int id)
        {

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@id", id);
            };

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Users_SetIsEmailConfirmed", inputMapper);

        }

        public void UndeleteAdmin(int id)
        {
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@id", id);
            };

            Action<SqlParameterCollection> returnMapper = delegate (SqlParameterCollection parameters)
            {
                //this function remains empty because Users_Activate doesn't return anything
            };

            DataProvider.ExecuteNonQuery(
                GetConnection,
                "dbo.Users_Activate",
                inputMapper,
                returnMapper
            );
        }

        public string GetSaltByUsername(string username)
        {
            string salt = null;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@Username", username);
            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                salt = reader.GetString(0);
            };

            DataProvider.ExecuteCmd(GetConnection, "dbo.Users_SelectSaltByUsername", inputMapper, resultMapper);
            return salt;
        }

        public void UpdateIsAdminAdmin(int id, bool isAdmin)
        {
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection paramaters)
            {
                paramaters.AddWithValue("@Id", id);
                paramaters.AddWithValue("@IsAdmin", isAdmin);
            };

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Users_UpdateIsAdmin", inputMapper);
        }

        public void SendPasswordResetLink(UserSendPasswordResetLinkRequest model)
        {
            User testUser = GetByEmail(model.Email);

			DateTime expiry = DateTime.Now.AddHours(3);
			string message = expiry + testUser.Salt + testUser.Id;
			string base64Hmac = _cryptographyService.HashMessage(message);

			if (testUser != null)
            {
                var fullName = testUser.FirstName + " " + testUser.LastName;
                _messagingService.SendPasswordResetRequestEmail(model.Email, fullName, testUser.Id, expiry, base64Hmac);
                
            };

        }

        public User GetByEmail(string email)
        {
            User testUser = null;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@Email", email);
            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                testUser = GetAllProps(reader);
            };

            DataProvider.ExecuteCmd(
                GetConnection,
                "dbo.Users_SelectByEmail",
                inputMapper,
                resultMapper
            );

            return testUser;
        }

        public void UpdatePassword(UserResetPasswordRequest model)
        {
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                string salt;
                string passwordHash;
                PasswordHasher(model.Password, out salt, out passwordHash);

                parameters.AddWithValue("@Password", passwordHash);

                parameters.AddWithValue("@Id", model.Id);

                parameters.AddWithValue("@Salt", salt);
            };

            Action<SqlParameterCollection> returnMapper = delegate (SqlParameterCollection parameters)
            {

            };

            DataProvider.ExecuteNonQuery(
                GetConnection,
                "dbo.Users_UpdatePasswordSalt",
                inputMapper,
                returnMapper);
        }
			
        public User GetByFacebookId(string facebookId)
        {
            User user = null;
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@FacebookId", facebookId);
            };
            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                user = GetAllProps(reader);
            };

            DataProvider.ExecuteCmd(GetConnection, "dbo.Users_SelectByFacebookId", inputMapper, resultMapper);
            return user;
        }

        public int CreateForFacebook(UserFacebookCreateRequest model)
        {
            int id = 0;
            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {
                parameters.AddWithValue("@Username", model.Username);
                parameters.AddWithValue("@Email", model.Email);
                parameters.AddWithValue("@FirstName", model.FirstName);
                parameters.AddWithValue("@LastName", model.LastName);
                parameters.AddWithValue("@Blurb", model.Blurb ?? (object)DBNull.Value);
                parameters.AddWithValue("@ImageUrl", model.ImageUrl ?? (object)DBNull.Value);
                parameters.AddWithValue("@FacebookId", model.FacebookId);

                SqlParameter idParam = new SqlParameter("@id", id);
                idParam.Direction = ParameterDirection.Output;
                parameters.Add(idParam);
            };
            Action<SqlParameterCollection> returnMapper = delegate (SqlParameterCollection parameters)
            {
                id = (int)parameters["@Id"].Value;
            };

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Users_InsertForFacebook", inputMapper, returnMapper);

            return id;
        }

        public List<MiniUser> GetAllMini()
        {
            List<MiniUser> list = null;

            Action<SqlParameterCollection> inputMapper = delegate (SqlParameterCollection parameters)
            {

            };

            Action<IDataReader, short> resultMapper = delegate (IDataReader reader, short set)
            {
                MiniUser newMiniUser = MapReaderToMiniUser(reader, 0);

                if(list == null)
                {
                    list = new List<MiniUser>();
                }

                list.Add(newMiniUser);
                
            };
            
             DataProvider.ExecuteCmd(
                GetConnection,
                "dbo.Users_SelectAllMini",
                inputMapper,
                resultMapper
            );

            return list;

        }


        public MiniUser MapReaderToMiniUser(IDataReader reader, int startingIndex)
        {
            MiniUser newMiniUser = new MiniUser();
            newMiniUser.Id = reader.GetInt32(startingIndex++);
            newMiniUser.Username = reader.GetString(startingIndex++);
            newMiniUser.ImageUrl = reader.GetSafeString(startingIndex++);
            return newMiniUser;
        }
        
    }
}