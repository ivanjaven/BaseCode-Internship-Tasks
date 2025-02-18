using System;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Net;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using BaseCode.Models.Responses;
using BaseCode.Models.Requests;
using BaseCode.Models.Tables;

using System.Reflection;
using Microsoft.AspNetCore.Identity.Data;
using BaseCode.Models.Tools;
using System.Net.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using BaseCode.Models.Responses.Customer;
using BaseCode.Models.Requests.Customer;

namespace BaseCode.Models
{
    public class DBContext
    {
        public string ConnectionString { get; set; }
        public DBContext(string connStr)
        {
            this.ConnectionString = connStr;
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public GenericInsertUpdateResponse InsertUpdateData(GenericInsertUpdateRequest r)
        {
            GenericInsertUpdateResponse resp = new GenericInsertUpdateResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlTransaction myTrans;
                    myTrans = conn.BeginTransaction();
                    MySqlCommand cmd = new MySqlCommand(r.query, conn);
                    cmd.ExecuteNonQuery();

                    resp.Id = r.isInsert ? int.Parse(cmd.LastInsertedId.ToString()) : -1;
                    myTrans.Commit();
                    conn.Close();
                    resp.isSuccess = true;
                    resp.Message = r.responseMessage;
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = r.errorMessage + ": " + ex.Message;
            }
            return resp;
        }
        public CreateUserResponse CreateUserUsingSqlScript(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();
            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");
            try
            {
                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("INSERT INTO USER (FIRST_NAME,LAST_NAME,USER_NAME,PASSWORD) " +
                    "VALUES ('" + r.FirstName + "','" + r.LastName + "','" + r.UserName + "','" + r.Password + "');", conn);

                    //  cmd.Parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                    cmd.Parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                    cmd.Parameters.Add(new MySqlParameter("@USER_NAME", r.UserName));
                    cmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }

            catch (Exception ex)
            {
                resp.Message = "Please try again.";
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully added user profile.";
            return resp;
        }

        public CreateUserResponse UpdateUser(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");

            try
            {

                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();


                    MySqlCommand cmd = new MySqlCommand("UPDATE USER SET FIRST_NAME = @FIRST_NAME, LAST_NAME = @LAST_NAME, USER_NAME = @USER_NAME, PASSWORD = @PASSWORD " +
                    "WHERE USER_ID = @USER_ID;", conn);
                    cmd.Parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                    cmd.Parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                    cmd.Parameters.Add(new MySqlParameter("@USER_NAME", r.UserName));
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));
                    cmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully updated user profile.";
            return resp;
        }
        public CreateUserResponse DeleteUser(string userId)
        {
            CreateUserResponse resp = new CreateUserResponse();

            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");
            try
            {
                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();


                    MySqlCommand cmd = new MySqlCommand("UPDATE USER SET STATUS = 'I' " +
                    "WHERE USER_ID = @USER_ID;", conn);
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", userId));

                    cmd.ExecuteNonQuery();

                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully deleted user.";
            return resp;
        }
        public GetUserListResponse GetUserList(GetUserListRequest r)
        {
            GetUserListResponse resp = new GetUserListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM USER WHERE STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                  new { Column = column.ColumnName, Value = dataRow[column] })
                  .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString())).ToList());
                    }
                    resp.isSuccess = true;
                    resp.Message = "List of users:";
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }

            return resp;
        }

        public GetUserListResponse GetUserById(string r)
        {
            GetUserListResponse resp = new GetUserListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM USER WHERE STATUS = 'A' AND USER_ID = @USER_ID";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", r));
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                  new { Column = column.ColumnName, Value = dataRow[column] })
                  .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString())).ToList());
                        resp.isSuccess = true;
                        resp.Message = "User Found";
                        conn.Close();

                        return resp;
                    }
                    resp.isSuccess = true;
                    resp.Message = "No User Found";
                    conn.Close();

                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }

            return resp;
        }

        public GenericGetDataResponse GetData(string query)
        {
            GenericGetDataResponse resp = new GenericGetDataResponse();
            DataTable dt;
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    dt = new DataTable();
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        dt.Load(reader);
                        reader.Close();
                    }
                    conn.Close();
                }
                resp.isSuccess = true;
                resp.Message = "Successfully get data";
                resp.Data = dt;

            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = ex.Message;
            }
            return resp;
        }

        public CreateUserInfoResponse CreateUserInfoUsingSqlScript(CreateUserInfoRequest r)
        {
            CreateUserInfoResponse resp = new CreateUserInfoResponse();
            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");
            try
            {
                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("INSERT INTO USER_INFO (USER_ID,MOBILE,EMAIL,BIRTHDAY,COUNTRY) " +
                    "VALUES (@USER_ID,@MOBILE,@EMAIL,@BIRTHDAY,@COUNTRY);", conn);

                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));
                    cmd.Parameters.Add(new MySqlParameter("@MOBILE", r.Mobile));
                    cmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                    cmd.Parameters.Add(new MySqlParameter("@BIRTHDAY", r.Birthday));
                    cmd.Parameters.Add(new MySqlParameter("@COUNTRY", r.Country));

                    cmd.ExecuteNonQuery();

                    conn.Clone();
                }
            }

            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully added user info profile.";
            return resp;
        }

        public GetUserProfileListResponse GetUserProfileList(GetUserProfileListRequest r)
        {
            GetUserProfileListResponse resp = new GetUserProfileListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT U.USER_ID,CONCAT(U.`LAST_NAME`,' ',U.`FIRST_NAME`) AS FULL_NAME,UI.`BIRTHDAY`,UI.`MOBILE`,UI.`EMAIL`,UI.`COUNTRY` FROM USER U " +
                        "LEFT JOIN USER_INFO UI ON UI.USER_ID = U.`USER_ID` WHERE U.STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                  new { Column = column.ColumnName, Value = dataRow[column] })
                  .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString())).ToList());
                    }

                    resp.isSuccess = true;
                    resp.Message = "List of users profile:";
                    conn.Close();

                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }

        }

        // TASK 2 (FEBRUARY 1)
        public CreateUserResponse RegisterUsingSqlScript(RegisterUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        MySqlCommand cmdUser = new MySqlCommand(
                            "INSERT INTO user (first_name, middle_name, last_name, age, phone_number, email, password, birthday) " +
                            "VALUES (@FIRST_NAME, @MIDDLE_NAME, @LAST_NAME, @AGE, @PHONE_NUMBER, @EMAIL, @PASSWORD, @BIRTHDAY);", conn);

                        cmdUser.Parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                        cmdUser.Parameters.Add(new MySqlParameter("@MIDDLE_NAME", r.MiddleName));
                        cmdUser.Parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                        cmdUser.Parameters.Add(new MySqlParameter("@AGE", r.Age));
                        cmdUser.Parameters.Add(new MySqlParameter("@PHONE_NUMBER", r.PhoneNumber));
                        cmdUser.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        cmdUser.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));
                        cmdUser.Parameters.Add(new MySqlParameter("@BIRTHDAY", r.Birthday));

                        cmdUser.ExecuteNonQuery();

                        // To be used for reference to address
                        int userId = (int)cmdUser.LastInsertedId;

                        MySqlCommand cmdAddress = new MySqlCommand(
                            "INSERT INTO address (user_id, house_no, barangay, city, province, zip) " +
                            "VALUES (@USER_ID, @HOUSE_NO, @BARANGAY, @CITY, @PROVINCE, @ZIP);", conn);

                        cmdAddress.Parameters.Add(new MySqlParameter("@USER_ID", userId));
                        cmdAddress.Parameters.Add(new MySqlParameter("@HOUSE_NO", r.Address.House_No));
                        cmdAddress.Parameters.Add(new MySqlParameter("@BARANGAY", r.Address.Barangay));
                        cmdAddress.Parameters.Add(new MySqlParameter("@CITY", r.Address.City));
                        cmdAddress.Parameters.Add(new MySqlParameter("@PROVINCE", r.Address.Province));
                        cmdAddress.Parameters.Add(new MySqlParameter("@ZIP", r.Address.ZIP));

                        cmdAddress.ExecuteNonQuery();

                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "User successfully registered.";
                        resp.UserId = userId;
                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
            }

            return resp;
        }

        public LogInUserResponse LogInUser(LogInUserRequest r)
        {
            LogInUserResponse resp = new LogInUserResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = "SELECT USER_ID FROM USER WHERE EMAIL = @EMAIL AND PASSWORD = @PASSWORD AND STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                    cmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        resp.isSuccess = true;
                        resp.Message = "Login successful.";
                        resp.UserId = reader.GetInt32("USER_ID");
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "Invalid username or password.";
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
            }

            return resp;
        }

        public ResetPasswordResponse ResetPassword(Requests.ResetPasswordRequest r)
        {
            ResetPasswordResponse resp = new ResetPasswordResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = "SELECT USER_ID, PASSWORD FROM USER WHERE EMAIL = @EMAIL";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        string currentStoredPassword = reader.GetString("PASSWORD");

                        reader.Close();

                        if (currentStoredPassword == r.CurrentPassword)
                        {

                            string updatePasswordSql = "UPDATE USER SET PASSWORD = @PASSWORD WHERE EMAIL = @EMAIL";
                            MySqlCommand updatePasswordCmd = new MySqlCommand(updatePasswordSql, conn);
                            updatePasswordCmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.NewPassword));
                            updatePasswordCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));

                            updatePasswordCmd.ExecuteNonQuery();

                            resp.isSuccess = true;
                            resp.Message = "Password has been successfully updated.";
                        }
                        else
                        {
                            resp.isSuccess = false;
                            resp.Message = "Current password is incorrect.";
                        }
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "No user found with the provided email.";
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
            }

            return resp;
        }

        public GenericAPIResponse UpdateUserInfo(RegisterUserRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string checkUserExistenceSql = "SELECT COUNT(*) FROM USER WHERE USER_ID = @USER_ID";
                    MySqlCommand checkCmd = new MySqlCommand(checkUserExistenceSql, conn);
                    checkCmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));
                    int userExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (userExists == 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "User ID not found.";
                        return resp;
                    }

                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string updateSql = "UPDATE USER SET ";
                        List<MySqlParameter> parameters = new List<MySqlParameter>();

                        if (!string.IsNullOrEmpty(r.FirstName))
                        {
                            updateSql += "FIRST_NAME = @FIRST_NAME, ";
                            parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                        }
                        if (!string.IsNullOrEmpty(r.MiddleName))
                        {
                            updateSql += "MIDDLE_NAME = @MIDDLE_NAME, ";
                            parameters.Add(new MySqlParameter("@MIDDLE_NAME", r.MiddleName));
                        }
                        if (!string.IsNullOrEmpty(r.LastName))
                        {
                            updateSql += "LAST_NAME = @LAST_NAME, ";
                            parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                        }
                        if (r.Age > 0)
                        {
                            updateSql += "AGE = @AGE, ";
                            parameters.Add(new MySqlParameter("@AGE", r.Age));
                        }
                        if (!string.IsNullOrEmpty(r.PhoneNumber))
                        {
                            updateSql += "PHONE_NUMBER = @PHONE_NUMBER, ";
                            parameters.Add(new MySqlParameter("@PHONE_NUMBER", r.PhoneNumber));
                        }
                        if (!string.IsNullOrEmpty(r.Email))
                        {
                            updateSql += "EMAIL = @EMAIL, ";
                            parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        }
                        if (!string.IsNullOrEmpty(r.Birthday))
                        {
                            updateSql += "BIRTHDAY = @BIRTHDAY, ";
                            parameters.Add(new MySqlParameter("@BIRTHDAY", r.Birthday));
                        }

                        if (updateSql.EndsWith(", "))
                            updateSql = updateSql.Substring(0, updateSql.Length - 2);

                        updateSql += " WHERE USER_ID = @USER_ID";
                        parameters.Add(new MySqlParameter("@USER_ID", r.UserId));

                        MySqlCommand cmd = new MySqlCommand(updateSql, conn);
                        cmd.Parameters.AddRange(parameters.ToArray());
                        cmd.ExecuteNonQuery();

                        if (r.Address != null)
                        {
                            string addressSql = "UPDATE ADDRESS SET ";
                            List<MySqlParameter> addressParameters = new List<MySqlParameter>();

                            if (!string.IsNullOrEmpty(r.Address.House_No))
                            {
                                addressSql += "HOUSE_NO = @HOUSE_NO, ";
                                addressParameters.Add(new MySqlParameter("@HOUSE_NO", r.Address.House_No));
                            }
                            if (!string.IsNullOrEmpty(r.Address.Barangay))
                            {
                                addressSql += "BARANGAY = @BARANGAY, ";
                                addressParameters.Add(new MySqlParameter("@BARANGAY", r.Address.Barangay));
                            }
                            if (!string.IsNullOrEmpty(r.Address.City))
                            {
                                addressSql += "CITY = @CITY, ";
                                addressParameters.Add(new MySqlParameter("@CITY", r.Address.City));
                            }
                            if (!string.IsNullOrEmpty(r.Address.Province))
                            {
                                addressSql += "PROVINCE = @PROVINCE, ";
                                addressParameters.Add(new MySqlParameter("@PROVINCE", r.Address.Province));
                            }
                            if (!string.IsNullOrEmpty(r.Address.ZIP))
                            {
                                addressSql += "ZIP = @ZIP, ";
                                addressParameters.Add(new MySqlParameter("@ZIP", r.Address.ZIP));
                            }

                            if (addressSql.EndsWith(", "))
                                addressSql = addressSql.Substring(0, addressSql.Length - 2);

                            addressSql += " WHERE USER_ID = @USER_ID";
                            addressParameters.Add(new MySqlParameter("@USER_ID", r.UserId));

                            MySqlCommand cmdAddress = new MySqlCommand(addressSql, conn);
                            cmdAddress.Parameters.AddRange(addressParameters.ToArray());
                            cmdAddress.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "User details successfully updated.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
            }

            return resp;
        }

        // TASK 3 (FEBRUARY 3)
        public GenericAPIResponse SendOTPResetCode(OTPRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            try
            {
                // Generate the OTP code using the OTPGenerator class
                string otpCode = OTPGenerator.GenerateOTP(r.UserId);

                string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

                TwilioClient.Init(accountSid, authToken);

                var messageOptions = new CreateMessageOptions(
                    new PhoneNumber(r.PhoneNumber))
                {
                    From = new PhoneNumber("+16184056690"),
                    Body = $"Your OTP code is {otpCode}"
                };

                try
                {
                    var message = MessageResource.Create(messageOptions);

                    // The message was sent successfully, save it to database
                    try
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            conn.Open();

                            string sql = "INSERT INTO OTP (USER_ID, VALUE, STATUS) " +
                                        "VALUES (@USER_ID, @VALUE, @STATUS);";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);

                            cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));
                            cmd.Parameters.Add(new MySqlParameter("@VALUE", otpCode));
                            cmd.Parameters.Add(new MySqlParameter("@STATUS", "ACTIVE"));

                            cmd.ExecuteNonQuery();

                            conn.Close();
                        }

                        resp.isSuccess = true;
                        resp.Message = "OTP sent successfully.";
                    }
                    catch (Exception ex)
                    {
                        resp.isSuccess = true;
                        resp.Message = "OTP sent successfully but failed to save to database.";
                    }
                }
                catch (Twilio.Exceptions.ApiException ex)
                {
                    resp.isSuccess = false;
                    resp.Message = "Failed to send OTP: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
            }

            return resp;
        }

        public GenericAPIResponse ValidateOTP(OTPValidationRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT USER_ID, STATUS, CREATE_DATE 
                               FROM OTP 
                               WHERE VALUE = @OTPCode 
                               AND USER_ID = @UserId
                               ORDER BY CREATE_DATE DESC 
                               LIMIT 1";

                    MySqlCommand selectCmd = new MySqlCommand(sql, conn);
                    selectCmd.Parameters.Add(new MySqlParameter("@OTPCode", r.OTPCode));
                    selectCmd.Parameters.Add(new MySqlParameter("@UserId", r.UserId));

                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            resp.isSuccess = false;
                            resp.Message = "Invalid OTP code.";
                            return resp;
                        }

                        string status = reader.GetString("STATUS");
                        DateTime createDate = reader.GetDateTime("CREATE_DATE");
                        reader.Close();

                        if (status != "ACTIVE")
                        {
                            resp.isSuccess = false;
                            resp.Message = $"OTP is {status.ToLower()}.";
                            return resp;
                        }

                        TimeSpan timeDifference = DateTime.Now - createDate;
                        if (timeDifference.TotalMinutes > 1)
                        {
                            string updateExpiredSql = "UPDATE OTP SET STATUS = 'EXPIRED' WHERE VALUE = @OTPCode AND USER_ID = @UserId";
                            MySqlCommand updateExpiredCmd = new MySqlCommand(updateExpiredSql, conn);
                            updateExpiredCmd.Parameters.Add(new MySqlParameter("@OTPCode", r.OTPCode));
                            updateExpiredCmd.Parameters.Add(new MySqlParameter("@UserId", r.UserId));
                            updateExpiredCmd.ExecuteNonQuery();

                            resp.isSuccess = false;
                            resp.Message = "OTP has expired.";
                            return resp;
                        }

                        string updateUsedSql = "UPDATE OTP SET STATUS = 'USED' WHERE VALUE = @OTPCode AND USER_ID = @UserId";
                        MySqlCommand updateUsedCmd = new MySqlCommand(updateUsedSql, conn);
                        updateUsedCmd.Parameters.Add(new MySqlParameter("@OTPCode", r.OTPCode));
                        updateUsedCmd.Parameters.Add(new MySqlParameter("@UserId", r.UserId));
                        updateUsedCmd.ExecuteNonQuery();

                        resp.isSuccess = true;
                        resp.Message = "OTP validation successful.";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred during OTP validation: " + ex.Message;
            }

            return resp;
        }

        // Task 4 (Feb 17)
        // CREATE 
        public CreateCustomerResponse RegisterCustomer(RegisterCustomerRequest r) 
        {
            CreateCustomerResponse resp = new CreateCustomerResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        MySqlCommand cmdUser = new MySqlCommand(
                            "INSERT INTO CUSTOMER (FIRST_NAME, MIDDLE_NAME, LAST_NAME, AGE, PHONE_NUMBER, EMAIL, PASSWORD, BIRTHDAY) " +
                            "VALUES (@FIRST_NAME, @MIDDLE_NAME, @LAST_NAME, @AGE, @PHONE_NUMBER, @EMAIL, @PASSWORD, @BIRTHDAY);", conn);

                        cmdUser.Parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                        cmdUser.Parameters.Add(new MySqlParameter("@MIDDLE_NAME", r.MiddleName));
                        cmdUser.Parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                        cmdUser.Parameters.Add(new MySqlParameter("@AGE", r.Age));
                        cmdUser.Parameters.Add(new MySqlParameter("@PHONE_NUMBER", r.PhoneNumber));
                        cmdUser.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        cmdUser.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));
                        cmdUser.Parameters.Add(new MySqlParameter("@BIRTHDAY", r.Birthday));

                        cmdUser.ExecuteNonQuery();

                        // To be used for reference to address
                        int customerId = (int)cmdUser.LastInsertedId;

                        MySqlCommand cmdAddress = new MySqlCommand(
                            "INSERT INTO ADDRESS_CUSTOMER (CUSTOMER_ID, HOUSE_NO, BARANGAY, CITY, PROVINCE, ZIP) " +
                            "VALUES (@CUSTOMER_ID, @HOUSE_NO, @BARANGAY, @CITY, @PROVINCE, @ZIP);", conn);

                        cmdAddress.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", customerId));
                        cmdAddress.Parameters.Add(new MySqlParameter("@HOUSE_NO", r.Address.House_No));
                        cmdAddress.Parameters.Add(new MySqlParameter("@BARANGAY", r.Address.Barangay));
                        cmdAddress.Parameters.Add(new MySqlParameter("@CITY", r.Address.City));
                        cmdAddress.Parameters.Add(new MySqlParameter("@PROVINCE", r.Address.Province));
                        cmdAddress.Parameters.Add(new MySqlParameter("@ZIP", r.Address.ZIP));

                        cmdAddress.ExecuteNonQuery();

                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "User successfully registered.";
                        resp.CustomerId = customerId;
                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
            }

            return resp;
        }

        //READ
        public GetCustomerProfileResponse ViewCustomerProfile(RegisterCustomerRequest r)
        {
            GetCustomerProfileResponse resp = new GetCustomerProfileResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    // Check active session first
                    string sessionSql = @"SELECT * FROM SESSION_CUSTOMER 
                               WHERE CUSTOMER_ID = @CustomerId 
                               AND STATUS = 'A'";
                    MySqlCommand sessionCmd = new MySqlCommand(sessionSql, conn);
                    sessionCmd.Parameters.AddWithValue("@CustomerId", r.CustomerId);
                    var sessionReader = sessionCmd.ExecuteReader();

                    if (!sessionReader.HasRows)
                    {
                        resp.isSuccess = false;
                        resp.Message = "No active session found. Please login again.";
                        return resp;
                    }
                    sessionReader.Close();

                    string sql = @"SELECT C.CUSTOMER_ID, C.FIRST_NAME, C.MIDDLE_NAME, C.LAST_NAME, 
                         C.AGE, C.PHONE_NUMBER, C.BIRTHDAY,
                         A.HOUSE_NO, A.BARANGAY, A.CITY, A.PROVINCE, A.ZIP 
                         FROM CUSTOMER C
                         LEFT JOIN ADDRESS_CUSTOMER A ON A.CUSTOMER_ID = C.CUSTOMER_ID 
                         WHERE A.CUSTOMER_ID = @CustomerId 
                         AND C.STATUS = 'A'";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@CustomerId", r.CustomerId);
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        resp.CustomerId = Convert.ToInt32(reader["CUSTOMER_ID"]);
                        resp.FirstName = reader["FIRST_NAME"].ToString();
                        resp.MiddleName = reader["MIDDLE_NAME"].ToString();
                        resp.LastName = reader["LAST_NAME"].ToString();
                        resp.Age = Convert.ToInt32(reader["AGE"]);
                        resp.PhoneNumber = reader["PHONE_NUMBER"].ToString();
                        resp.Birthday = reader["BIRTHDAY"].ToString();

                        resp.Address = new Responses.Customer.AddressResponse
                        {
                            House_No = reader["HOUSE_NO"].ToString(),
                            Barangay = reader["BARANGAY"].ToString(),
                            City = reader["CITY"].ToString(),
                            Province = reader["PROVINCE"].ToString(),
                            ZIP = reader["ZIP"].ToString()
                        };

                        resp.isSuccess = true;
                        resp.Message = "Customer profile retrieved successfully";
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "Customer not found";
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
            }
            return resp;
        }

        // UPDATE
        public GenericAPIResponse UpdateCustomerInfo(RegisterCustomerRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sessionSql = "SELECT COUNT(*) FROM SESSION_CUSTOMER WHERE CUSTOMER_ID = @CUSTOMER_ID AND STATUS = 'A'";
                    MySqlCommand sessionCmd = new MySqlCommand(sessionSql, conn);
                    sessionCmd.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", r.CustomerId));
                    int activeSessionCount = Convert.ToInt32(sessionCmd.ExecuteScalar());

                    if (activeSessionCount == 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "No active session found. Please log in first.";
                        return resp;
                    }

                    string checkUserExistenceSql = "SELECT COUNT(*) FROM CUSTOMER WHERE CUSTOMER_ID = @CUSTOMER_ID";
                    MySqlCommand checkCmd = new MySqlCommand(checkUserExistenceSql, conn);
                    checkCmd.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", r.CustomerId));
                    int userExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (userExists == 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "Customer ID not found.";
                        return resp;
                    }

                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string updateSql = "UPDATE CUSTOMER SET ";
                        List<MySqlParameter> parameters = new List<MySqlParameter>();

                        if (!string.IsNullOrEmpty(r.FirstName))
                        {
                            updateSql += "FIRST_NAME = @FIRST_NAME, ";
                            parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                        }
                        if (!string.IsNullOrEmpty(r.MiddleName))
                        {
                            updateSql += "MIDDLE_NAME = @MIDDLE_NAME, ";
                            parameters.Add(new MySqlParameter("@MIDDLE_NAME", r.MiddleName));
                        }
                        if (!string.IsNullOrEmpty(r.LastName))
                        {
                            updateSql += "LAST_NAME = @LAST_NAME, ";
                            parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                        }
                        if (r.Age > 0)
                        {
                            updateSql += "AGE = @AGE, ";
                            parameters.Add(new MySqlParameter("@AGE", r.Age));
                        }
                        if (!string.IsNullOrEmpty(r.PhoneNumber))
                        {
                            updateSql += "PHONE_NUMBER = @PHONE_NUMBER, ";
                            parameters.Add(new MySqlParameter("@PHONE_NUMBER", r.PhoneNumber));
                        }
                        if (!string.IsNullOrEmpty(r.Email))
                        {
                            updateSql += "EMAIL = @EMAIL, ";
                            parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        }
                        if (!string.IsNullOrEmpty(r.Birthday))
                        {
                            updateSql += "BIRTHDAY = @BIRTHDAY, ";
                            parameters.Add(new MySqlParameter("@BIRTHDAY", r.Birthday));
                        }

                        if (updateSql.EndsWith(", "))
                            updateSql = updateSql.Substring(0, updateSql.Length - 2);

                        updateSql += " WHERE CUSTOMER_ID = @CUSTOMER_ID";
                        parameters.Add(new MySqlParameter("@CUSTOMER_ID", r.CustomerId));

                        MySqlCommand cmd = new MySqlCommand(updateSql, conn);
                        cmd.Parameters.AddRange(parameters.ToArray());
                        cmd.ExecuteNonQuery();

                        if (r.Address != null)
                        {
                            string addressSql = "UPDATE ADDRESS SET ";
                            List<MySqlParameter> addressParameters = new List<MySqlParameter>();

                            if (!string.IsNullOrEmpty(r.Address.House_No))
                            {
                                addressSql += "HOUSE_NO = @HOUSE_NO, ";
                                addressParameters.Add(new MySqlParameter("@HOUSE_NO", r.Address.House_No));
                            }
                            if (!string.IsNullOrEmpty(r.Address.Barangay))
                            {
                                addressSql += "BARANGAY = @BARANGAY, ";
                                addressParameters.Add(new MySqlParameter("@BARANGAY", r.Address.Barangay));
                            }
                            if (!string.IsNullOrEmpty(r.Address.City))
                            {
                                addressSql += "CITY = @CITY, ";
                                addressParameters.Add(new MySqlParameter("@CITY", r.Address.City));
                            }
                            if (!string.IsNullOrEmpty(r.Address.Province))
                            {
                                addressSql += "PROVINCE = @PROVINCE, ";
                                addressParameters.Add(new MySqlParameter("@PROVINCE", r.Address.Province));
                            }
                            if (!string.IsNullOrEmpty(r.Address.ZIP))
                            {
                                addressSql += "ZIP = @ZIP, ";
                                addressParameters.Add(new MySqlParameter("@ZIP", r.Address.ZIP));
                            }

                            if (addressSql.EndsWith(", "))
                                addressSql = addressSql.Substring(0, addressSql.Length - 2);

                            addressSql += " WHERE CUSTOMER_ID = @CUSTOMER_ID";
                            addressParameters.Add(new MySqlParameter("@CUSTOMER_ID", r.CustomerId));

                            MySqlCommand cmdAddress = new MySqlCommand(addressSql, conn);
                            cmdAddress.Parameters.AddRange(addressParameters.ToArray());
                            cmdAddress.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "Customer details successfully updated.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
            }

            return resp;
        }

        // DELETE
        public CreateCustomerResponse DeleteCustomerAccount(string customerId)
        {
            CreateCustomerResponse resp = new CreateCustomerResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sessionSql = "SELECT COUNT(*) FROM SESSION_CUSTOMER WHERE CUSTOMER_ID = @CUSTOMER_ID AND STATUS = 'A'";
                    MySqlCommand sessionCmd = new MySqlCommand(sessionSql, conn);
                    sessionCmd.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", customerId));
                    int activeSessionCount = Convert.ToInt32(sessionCmd.ExecuteScalar());

                    if (activeSessionCount == 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "No active session found. Please log in first.";
                        return resp;
                    }

                    MySqlCommand userCmd = new MySqlCommand(
                        "UPDATE CUSTOMER SET STATUS = 'I' WHERE CUSTOMER_ID = @CUSTOMER_ID", conn);
                    userCmd.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", customerId));
                    userCmd.ExecuteNonQuery();

                    resp.isSuccess = true;
                    resp.Message = "Successfully deleted customer.";
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
            }
            return resp;
        }

        // LOG IN
        public LogInCustomerResponse LogInCustomer(LogInUserRequest r) 
        {
            LogInCustomerResponse resp = new LogInCustomerResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        // Check login attempts in last 2 minutes
                        string attemptSql = @"SELECT COUNT(*) FROM LOGIN_ATTEMPT_CUSTOMER
                                    WHERE CUSTOMER_ID = (SELECT CUSTOMER_ID FROM CUSTOMER WHERE EMAIL = @EMAIL)
                                    AND SUCCESS = false 
                                    AND ATTEMPT_DATE >= DATE_SUB(NOW(), INTERVAL 2 MINUTE)";

                        MySqlCommand attemptCmd = new MySqlCommand(attemptSql, conn);
                        attemptCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        int recentAttempts = Convert.ToInt32(attemptCmd.ExecuteScalar());

                        if (recentAttempts >= 5)
                        {
                            resp.isSuccess = false;
                            resp.Message = "Account is temporarily locked. Please try again later.";
                            return resp;
                        }

                        string loginSql = "SELECT CUSTOMER_ID FROM CUSTOMER WHERE EMAIL = @EMAIL AND PASSWORD = @PASSWORD AND STATUS = 'A'";
                        MySqlCommand loginCmd = new MySqlCommand(loginSql, conn);
                        loginCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        loginCmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));

                        var reader = loginCmd.ExecuteReader();
                        bool loginSuccess = reader.HasRows;
                        int customerId = 0;

                        if (loginSuccess)
                        {
                            reader.Read();
                            customerId = reader.GetInt32("CUSTOMER_ID");
                        }
                        reader.Close();

                        string insertAttemptSql = @"INSERT INTO LOGIN_ATTEMPT_CUSTOMER (CUSTOMER_ID, SUCCESS) 
                                          VALUES ((SELECT CUSTOMER_ID FROM CUSTOMER WHERE EMAIL = @EMAIL), @SUCCESS)";
                        MySqlCommand insertAttemptCmd = new MySqlCommand(insertAttemptSql, conn);
                        insertAttemptCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        insertAttemptCmd.Parameters.Add(new MySqlParameter("@SUCCESS", loginSuccess));
                        insertAttemptCmd.ExecuteNonQuery();

                        if (loginSuccess)
                        {
                            string sessionSql = "INSERT INTO SESSION_CUSTOMER (CUSTOMER_ID) VALUES (@CUSTOMER_ID)";
                            MySqlCommand sessionCmd = new MySqlCommand(sessionSql, conn);
                            sessionCmd.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", customerId));
                            sessionCmd.ExecuteNonQuery();

                            resp.isSuccess = true;
                            resp.Message = "Login successful.";
                            resp.CustomerId = customerId;
                        }
                        else
                        {
                            resp.isSuccess = false;
                            resp.Message = "Invalid username or password.";
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
            }
            return resp;
        }

        // FORGOT PASSWORD
        public ResetPasswordResponse ForgotPassword(Requests.ResetPasswordRequest r)
        {
            ResetPasswordResponse resp = new ResetPasswordResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = "SELECT CUSTOMER_ID, PASSWORD FROM CUSTOMER WHERE EMAIL = @EMAIL";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        string currentStoredPassword = reader.GetString("PASSWORD");

                        reader.Close();

                        if (currentStoredPassword == r.CurrentPassword)
                        {

                            string updatePasswordSql = "UPDATE CUSTOMER SET PASSWORD = @PASSWORD WHERE EMAIL = @EMAIL";
                            MySqlCommand updatePasswordCmd = new MySqlCommand(updatePasswordSql, conn);
                            updatePasswordCmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.NewPassword));
                            updatePasswordCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));

                            updatePasswordCmd.ExecuteNonQuery();

                            resp.isSuccess = true;
                            resp.Message = "Password has been successfully updated.";
                        }
                        else
                        {
                            resp.isSuccess = false;
                            resp.Message = "Current password is incorrect.";
                        }
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "No user found with the provided email.";
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
            }

            return resp;
        }

    }
}
