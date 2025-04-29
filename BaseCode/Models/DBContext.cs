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
using BaseCode.Models.Requests.Roles;
using BaseCode.Models.Responses.Roles;
using BaseCode.Models.Requests.Car;
using BaseCode.Models.Responses.Car;

using System.Net.Http.Headers;
using System.Net.Mail;



namespace BaseCode.Models
{
    public class DBContext
    {
        public string ConnectionString { get; set; }
        private readonly TemplateService _templateService;
        public DBContext(string connStr, TemplateService templateService = null)
        {
            this.ConnectionString = connStr;
            this._templateService = templateService;
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public void LogAPICalls(string api_path, object param, object resp, string errorMsg = "", string ipAddress = "")
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                try
                {
                    string parameters = JsonConvert.SerializeObject(param, Formatting.Indented);
                    string response = JsonConvert.SerializeObject(resp, Formatting.Indented);

                    MySqlCommand sql = new MySqlCommand("INSERT INTO API_LOG(API_METHOD_NAME, API_PARAMETERS, API_RESPONSE, API_IP_ADDRESS, API_TRACE_ID) VALUES(@API_METHOD_NAME, @API_PARAMETERS, @API_RESPONSE, @API_IP_ADDRESS, @API_TRACE_ID)", conn);
                    sql.Parameters.Add(new MySqlParameter("@API_METHOD_NAME", api_path));
                    sql.Parameters.Add(new MySqlParameter("@API_PARAMETERS", parameters));
                    sql.Parameters.Add(new MySqlParameter("@API_RESPONSE", response));
                    sql.Parameters.Add(new MySqlParameter("@API_IP_ADDRESS", ipAddress));
                    sql.Parameters.Add(new MySqlParameter("@API_TRACE_ID", errorMsg));
                    sql.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                    conn.Close();
                }
            }
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
                LogAPICalls("/BaseCode/InsertUpdateData", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = r.errorMessage + ": " + ex.Message;
                LogAPICalls("/BaseCode/InsertUpdateData", r, resp, ex.Message);
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
                resp.isSuccess = true;
                resp.Message = "Successfully added user profile.";
                LogAPICalls("/BaseCode/CreateUser", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = "Please try again.";
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/CreateUser", r, resp, ex.Message);
            }
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
                resp.isSuccess = true;
                resp.Message = "Successfully updated user profile.";
                LogAPICalls("/BaseCode/UpdateUser", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/UpdateUser", r, resp, ex.Message);
            }
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
                resp.isSuccess = true;
                resp.Message = "Successfully deleted user.";
                LogAPICalls("/BaseCode/DeleteUser", userId, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/DeleteUser", userId, resp, ex.Message);
            }
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
                LogAPICalls("/BaseCode/GetUserList", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/GetUserList", r, resp, ex.Message);
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
                        LogAPICalls("/BaseCode/GetUserById", r, resp, resp.isSuccess ? "" : resp.Message);
                        return resp;
                    }
                    resp.isSuccess = true;
                    resp.Message = "No User Found";
                    conn.Close();
                    LogAPICalls("/BaseCode/GetUserById", r, resp, resp.isSuccess ? "" : resp.Message);
                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/GetUserById", r, resp, ex.Message);
                return resp;
            }
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
                LogAPICalls("/BaseCode/GetData", query, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = ex.Message;
                LogAPICalls("/BaseCode/GetData", query, resp, ex.Message);
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
                resp.isSuccess = true;
                resp.Message = "Successfully added user info profile.";
                LogAPICalls("/BaseCode/CreateUserInfo", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/CreateUserInfo", r, resp, ex.Message);
            }
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
                    LogAPICalls("/BaseCode/GetUserProfileList", r, resp, resp.isSuccess ? "" : resp.Message);
                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/GetUserProfileList", r, resp, ex.Message);
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
                        LogAPICalls("/BaseCode/RegisterUser", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/BaseCode/RegisterUser", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
                LogAPICalls("/BaseCode/RegisterUser", r, resp, ex.Message);
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
                LogAPICalls("/BaseCode/LogInUser", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/BaseCode/LogInUser", r, resp, ex.Message);
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
                LogAPICalls("/BaseCode/ResetPassword", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/BaseCode/ResetPassword", r, resp, ex.Message);
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
                        LogAPICalls("/BaseCode/UpdateUserInfo", r, resp, resp.Message);
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
                        LogAPICalls("/BaseCode/UpdateUserInfo", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/BaseCode/UpdateUserInfo", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
                LogAPICalls("/BaseCode/UpdateUserInfo", r, resp, ex.Message);
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
                string otpCode = OTPGenerator.GenerateOTP();

                string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

                TwilioClient.Init(accountSid, authToken);

                var messageOptions = new CreateMessageOptions(
                    new PhoneNumber(r.PhoneNumber))
                {
                    From = new PhoneNumber("+15632042922"),
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
                        LogAPICalls("/BaseCode/SendOTPResetCode", r, resp, ex.Message);
                    }
                }
                catch (Twilio.Exceptions.ApiException ex)
                {
                    resp.isSuccess = false;
                    resp.Message = "Failed to send OTP: " + ex.Message;
                    LogAPICalls("/BaseCode/SendOTPResetCode", r, resp, ex.Message);
                }
                LogAPICalls("/BaseCode/SendOTPResetCode", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/BaseCode/SendOTPResetCode", r, resp, ex.Message);
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
                            LogAPICalls("/BaseCode/ValidateOTP", r, resp, resp.Message);
                            return resp;
                        }

                        string status = reader.GetString("STATUS");
                        DateTime createDate = reader.GetDateTime("CREATE_DATE");
                        reader.Close();

                        if (status != "ACTIVE")
                        {
                            resp.isSuccess = false;
                            resp.Message = $"OTP is {status.ToLower()}.";
                            LogAPICalls("/BaseCode/ValidateOTP", r, resp, resp.Message);
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
                            LogAPICalls("/BaseCode/ValidateOTP", r, resp, resp.Message);
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
                LogAPICalls("/BaseCode/ValidateOTP", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred during OTP validation: " + ex.Message;
                LogAPICalls("/BaseCode/ValidateOTP", r, resp, ex.Message);
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
                        LogAPICalls("/BaseCode/RegisterCustomer", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/BaseCode/RegisterCustomer", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
                LogAPICalls("/BaseCode/RegisterCustomer", r, resp, ex.Message);
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
                        sessionReader.Close();
                        LogAPICalls("/BaseCode/ViewCustomerProfile", r, resp, resp.Message);
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
                LogAPICalls("/BaseCode/ViewCustomerProfile", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/BaseCode/ViewCustomerProfile", r, resp, ex.Message);
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
                        LogAPICalls("/BaseCode/UpdateCustomerInfo", r, resp, resp.Message);
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
                        LogAPICalls("/BaseCode/UpdateCustomerInfo", r, resp, resp.Message);
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
                        LogAPICalls("/BaseCode/UpdateCustomerInfo", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/BaseCode/UpdateCustomerInfo", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Database connection error: " + ex.Message;
                LogAPICalls("/BaseCode/UpdateCustomerInfo", r, resp, ex.Message);
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
                        LogAPICalls("/BaseCode/DeleteCustomerAccount", customerId, resp, resp.Message);
                        return resp;
                    }

                    MySqlCommand userCmd = new MySqlCommand(
                        "UPDATE CUSTOMER SET STATUS = 'I' WHERE CUSTOMER_ID = @CUSTOMER_ID", conn);
                    userCmd.Parameters.Add(new MySqlParameter("@CUSTOMER_ID", customerId));
                    userCmd.ExecuteNonQuery();

                    resp.isSuccess = true;
                    resp.Message = "Successfully deleted customer.";
                }
                LogAPICalls("/BaseCode/DeleteCustomerAccount", customerId, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/BaseCode/DeleteCustomerAccount", customerId, resp, ex.Message);
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
                            LogAPICalls("/BaseCode/LogInCustomer", r, resp, resp.Message);
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
                LogAPICalls("/BaseCode/LogInCustomer", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/BaseCode/LogInCustomer", r, resp, ex.Message);
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
                LogAPICalls("/BaseCode/ForgotPassword", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/BaseCode/ForgotPassword", r, resp, ex.Message);
            }

            return resp;
        }

        // TASK 5 (FEB 20) ROLES AND PERMISSIONS
        public RoleResponse CreateRole(CreateRoleRequest r)
        {
            RoleResponse resp = new RoleResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string sql = "INSERT INTO ROLES (ROLE_NAME, DESCRIPTION) VALUES (@ROLE_NAME, @DESCRIPTION)";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.Add(new MySqlParameter("@ROLE_NAME", r.RoleName));
                        cmd.Parameters.Add(new MySqlParameter("@DESCRIPTION", r.Description));

                        cmd.ExecuteNonQuery();
                        int roleId = (int)cmd.LastInsertedId;

                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "Role created successfully";
                        resp.RoleId = roleId;
                        resp.RoleName = r.RoleName;
                        resp.Description = r.Description;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error creating role: " + ex.Message;
                        LogAPICalls("/RolePermission/CreateRole", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/RolePermission/CreateRole", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/RolePermission/CreateRole", r, resp, ex.Message);
            }
            return resp;
        }
        public GetRoleListResponse GetRoles(GetRoleRequest r)
        {
            GetRoleListResponse resp = new GetRoleListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM ROLES";

                    if (r.RoleId.HasValue)
                    {
                        sql += " WHERE ROLE_ID = @ROLE_ID";
                    }

                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    if (r.RoleId.HasValue)
                    {
                        cmd.Parameters.Add(new MySqlParameter("@ROLE_ID", r.RoleId.Value));
                    }

                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable()
                            .Select(dataRow => columns.Select(column =>
                                new { Column = column.ColumnName, Value = dataRow[column] })
                                .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString()))
                            .ToList());
                    }
                    resp.isSuccess = true;
                    resp.Message = "Roles retrieved successfully";
                }
                LogAPICalls("/RolePermission/GetRoles", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/RolePermission/GetRoles", r, resp, ex.Message);
            }
            return resp;
        }

        public PermissionResponse GetPermissions()
        {
            PermissionResponse resp = new PermissionResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM PERMISSIONS";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable()
                            .Select(dataRow => columns.Select(column =>
                                new { Column = column.ColumnName, Value = dataRow[column] })
                                .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString()))
                            .ToList());
                    }
                    resp.isSuccess = true;
                    resp.Message = "Permissions retrieved successfully";
                }
                LogAPICalls("/RolePermission/GetPermissions", null, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/RolePermission/GetPermissions", null, resp, ex.Message);
            }
            return resp;
        }

        public GenericAPIResponse AssignPermissionToRole(AssignPermissionRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string checkSql = "SELECT COUNT(*) FROM ROLE_PERMISSIONS WHERE ROLE_ID = @ROLE_ID AND PERMISSION_ID = @PERMISSION_ID";
                        MySqlCommand checkCmd = new MySqlCommand(checkSql, conn);
                        checkCmd.Parameters.Add(new MySqlParameter("@ROLE_ID", r.RoleId));
                        checkCmd.Parameters.Add(new MySqlParameter("@PERMISSION_ID", r.PermissionId));
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (exists > 0)
                        {
                            resp.isSuccess = true;
                            resp.Message = "Permission already assigned to role";
                            LogAPICalls("/RolePermission/AssignPermissionToRole", r, resp, "");
                            return resp;
                        }

                        string sql = "INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID) VALUES (@ROLE_ID, @PERMISSION_ID)";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.Add(new MySqlParameter("@ROLE_ID", r.RoleId));
                        cmd.Parameters.Add(new MySqlParameter("@PERMISSION_ID", r.PermissionId));

                        cmd.ExecuteNonQuery();
                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "Permission assigned to role successfully";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error assigning permission: " + ex.Message;
                        LogAPICalls("/RolePermission/AssignPermissionToRole", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/RolePermission/AssignPermissionToRole", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/RolePermission/AssignPermissionToRole", r, resp, ex.Message);
            }
            return resp;
        }

        public GenericAPIResponse AssignUserRole(AssignUserRoleRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string sql = "UPDATE USER SET ROLE_ID = @ROLE_ID WHERE USER_ID = @USER_ID";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.Add(new MySqlParameter("@ROLE_ID", r.RoleId));
                        cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.TargetUserId));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            resp.isSuccess = false;
                            resp.Message = "User not found";
                            LogAPICalls("/RolePermission/AssignUserRole", r, resp, resp.Message);
                            return resp;
                        }

                        transaction.Commit();

                        resp.isSuccess = true;
                        resp.Message = "Role assigned to user successfully";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        resp.isSuccess = false;
                        resp.Message = "Error assigning role: " + ex.Message;
                        LogAPICalls("/RolePermission/AssignUserRole", r, resp, ex.Message);
                        return resp;
                    }
                }
                LogAPICalls("/RolePermission/AssignUserRole", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/RolePermission/AssignUserRole", r, resp, ex.Message);
            }
            return resp;
        }

        public bool CheckPermission(int userId, string permissionName)
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT COUNT(*) FROM USER u
                JOIN ROLE_PERMISSIONS rp ON u.ROLE_ID = rp.ROLE_ID
                JOIN PERMISSIONS p ON rp.PERMISSION_ID = p.PERMISSION_ID
                WHERE u.USER_ID = @USER_ID 
                AND p.PERMISSION_NAME = @PERMISSION_NAME
                AND u.STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", userId));
                    cmd.Parameters.Add(new MySqlParameter("@PERMISSION_NAME", permissionName));

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    bool result = count > 0;

                    return result;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public PermissionResponse GetUserPermissions(int userId)
        {
            PermissionResponse resp = new PermissionResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT p.PERMISSION_ID, p.PERMISSION_NAME, p.DESCRIPTION
                FROM USER u
                JOIN ROLE_PERMISSIONS rp ON u.ROLE_ID = rp.ROLE_ID
                JOIN PERMISSIONS p ON rp.PERMISSION_ID = p.PERMISSION_ID
                WHERE u.USER_ID = @USER_ID 
                AND u.STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", userId));

                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable()
                            .Select(dataRow => columns.Select(column =>
                                new { Column = column.ColumnName, Value = dataRow[column] })
                                .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString()))
                            .ToList());
                        resp.isSuccess = true;
                        resp.Message = "User permissions retrieved successfully";
                    }
                    else
                    {
                        resp.isSuccess = true;
                        resp.Message = "User has no permissions";
                    }
                }
                LogAPICalls("/RolePermission/GetUserPermissions", userId, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/RolePermission/GetUserPermissions", userId, resp, ex.Message);
            }
            return resp;
        }

        public GetUserListResponse GetCustomerUsers()
        {
            GetUserListResponse resp = new GetUserListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM USER WHERE STATUS = 'A' AND ROLE_ID = 2";

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
                    resp.Message = "List of customers:";
                    conn.Close();
                }
                LogAPICalls("/RolePermission/GetCustomers", null, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                LogAPICalls("/RolePermission/GetCustomers", null, resp, ex.Message);
            }
            return resp;
        }

        // TASK 6 (MARCH 4) CAR MANAGEMENT
        public GetCarResponse GetCarById(GetCarByIdRequest r)
        {
            GetCarResponse resp = new GetCarResponse();
            resp.Data = new List<Dictionary<string, string>>();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM CAR WHERE CAR_ID = @CAR_ID";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@CAR_ID", r.CarId));
                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable()
                            .Select(dataRow => columns.Select(column =>
                                new { Column = column.ColumnName, Value = dataRow[column] })
                                .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString()))
                            .ToList());

                        resp.isSuccess = true;
                        resp.Message = "Car found";

                        if (resp.Data.Count > 0)
                        {
                            var firstCarData = resp.Data[0];
                            resp.CarId = int.Parse(firstCarData["CAR_ID"]);
                        }
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "No car found with the specified ID";
                    }
                    conn.Close();
                }
                LogAPICalls("/Car/GetCarById", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/Car/GetCarById", r, resp, ex.Message);
            }
            return resp;
        }

        public GetCarsResponse GetAllCars(GetAllCarsRequest r)
        {
            GetCarsResponse resp = new GetCarsResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string countSql = "SELECT COUNT(*) FROM CAR WHERE CAR_STATUS = @STATUS";
                    MySqlCommand countCmd = new MySqlCommand(countSql, conn);
                    countCmd.Parameters.Add(new MySqlParameter("@STATUS", r.Status));
                    int totalCount = Convert.ToInt32(countCmd.ExecuteScalar());

                    int offset = (r.Page - 1) * r.PageSize;

                    string sql = "SELECT * FROM CAR WHERE CAR_STATUS = @STATUS LIMIT @LIMIT OFFSET @OFFSET";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@STATUS", r.Status));
                    cmd.Parameters.Add(new MySqlParameter("@LIMIT", r.PageSize));
                    cmd.Parameters.Add(new MySqlParameter("@OFFSET", offset));

                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable()
                            .Select(dataRow => columns.Select(column =>
                                new { Column = column.ColumnName, Value = dataRow[column] })
                                .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString()))
                            .ToList());

                        int totalPages = (int)Math.Ceiling(totalCount / (double)r.PageSize);
                        resp.Pagination = new PaginationInfo
                        {
                            CurrentPage = r.Page,
                            PageSize = r.PageSize,
                            TotalItems = totalCount,
                            TotalPages = totalPages
                        };

                        resp.isSuccess = true;
                        resp.Message = "List of cars retrieved successfully";
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "No cars found";
                        resp.Pagination = new PaginationInfo
                        {
                            CurrentPage = r.Page,
                            PageSize = r.PageSize,
                            TotalItems = 0,
                            TotalPages = 0
                        };
                    }
                    conn.Close();
                }
                LogAPICalls("/Car/GetAllCars", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/Car/GetAllCars", r, resp, ex.Message);
            }
            return resp;
        }

        public GetCarsResponse GetCarByName(GetCarByNameRequest r)
        {
            GetCarsResponse resp = new GetCarsResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string countSql = "SELECT COUNT(*) FROM CAR WHERE (CAR_MODEL LIKE @CAR_NAME OR CAR_BRAND LIKE @CAR_NAME) AND CAR_STATUS = 'A'";
                    MySqlCommand countCmd = new MySqlCommand(countSql, conn);
                    countCmd.Parameters.Add(new MySqlParameter("@CAR_NAME", "%" + r.CarName + "%"));
                    int totalCount = Convert.ToInt32(countCmd.ExecuteScalar());

                    int offset = (r.Page - 1) * r.PageSize;

                    string sql = "SELECT * FROM CAR WHERE (CAR_MODEL LIKE @CAR_NAME OR CAR_BRAND LIKE @CAR_NAME) AND CAR_STATUS = 'A' LIMIT @LIMIT OFFSET @OFFSET";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@CAR_NAME", "%" + r.CarName + "%"));
                    cmd.Parameters.Add(new MySqlParameter("@LIMIT", r.PageSize));
                    cmd.Parameters.Add(new MySqlParameter("@OFFSET", offset));

                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable()
                            .Select(dataRow => columns.Select(column =>
                                new { Column = column.ColumnName, Value = dataRow[column] })
                                .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString()))
                            .ToList());

                        int totalPages = (int)Math.Ceiling(totalCount / (double)r.PageSize);
                        resp.Pagination = new PaginationInfo
                        {
                            CurrentPage = r.Page,
                            PageSize = r.PageSize,
                            TotalItems = totalCount,
                            TotalPages = totalPages
                        };

                        resp.isSuccess = true;
                        resp.Message = "Cars matching the search criteria found";
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "No cars found matching the search criteria";
                        resp.Pagination = new PaginationInfo
                        {
                            CurrentPage = r.Page,
                            PageSize = r.PageSize,
                            TotalItems = 0,
                            TotalPages = 0
                        };
                    }
                    conn.Close();
                }
                LogAPICalls("/Car/GetCarByName", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "An error occurred: " + ex.Message;
                LogAPICalls("/Car/GetCarByName", r, resp, ex.Message);
            }
            return resp;
        }

        // Email Verification
        public GenericAPIResponse SendVerificationEmail(EmailVerificationRequest r, TemplateService templateService = null)
        {
            GenericAPIResponse resp = new GenericAPIResponse();

            try
            {
                TemplateService service = templateService ?? _templateService;
                string verificationCode = OTPGenerator.GenerateOTP();
                string userName = "There!";

                if (!string.IsNullOrEmpty(r.Email))
                {
                    try
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            conn.Open();
                            string userSql = "SELECT FIRST_NAME FROM USER WHERE EMAIL = @EMAIL AND STATUS = 'A'";
                            MySqlCommand userCmd = new MySqlCommand(userSql, conn);
                            userCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                            var userResult = userCmd.ExecuteScalar();
                            if (userResult != null && !Convert.IsDBNull(userResult))
                            {
                                userName = userResult.ToString();
                            }
                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error getting user name: " + ex.Message);
                    }
                }

                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = "INSERT INTO EMAIL_VERIFICATION (EMAIL, TOKEN, EXPIRY_TIME, STATUS, USER_ID) " +
                                 "VALUES (@EMAIL, @TOKEN, @EXPIRY_TIME, @STATUS, @USER_ID);";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                    cmd.Parameters.Add(new MySqlParameter("@TOKEN", verificationCode));
                    cmd.Parameters.Add(new MySqlParameter("@EXPIRY_TIME", DateTime.Now.AddMinutes(15)));
                    cmd.Parameters.Add(new MySqlParameter("@STATUS", "ACTIVE"));
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                string emailBody;
                string emailSubject = "Your Email Verification Code";

                if (service != null)
                {
                    var template = service.GetTemplateByCode("WELCOMEEMAIL");

                    if (template != null)
                    {
                        emailBody = template.TemplateContent
                            .Replace("~NAME~", userName)
                            .Replace("~VCODE~", verificationCode);

                        if (!string.IsNullOrEmpty(template.TemplateSubject))
                        {
                            emailSubject = template.TemplateSubject;
                        }
                    }
                    else
                    {
                        emailBody = $"<html><body>" +
                                 $"<h2>Email Verification</h2>" +
                                 $"<p>Hello {userName},</p>" +
                                 $"<p>Your verification code is: <strong>{verificationCode}</strong></p>" +
                                 $"<p>This code will expire in 15 minutes.</p>" +
                                 $"</body></html>";
                    }
                }
                else
                {
                    emailBody = $"<html><body>" +
                             $"<h2>Email Verification</h2>" +
                             $"<p>Hello {userName},</p>" +
                             $"<p>Your verification code is: <strong>{verificationCode}</strong></p>" +
                             $"<p>This code will expire in 15 minutes.</p>" +
                             $"</body></html>";
                }

                using (var mailMessage = new System.Net.Mail.MailMessage())
                {
                    mailMessage.From = new System.Net.Mail.MailAddress("crisivan.javen.9@gmail.com", "Email Verification");
                    mailMessage.To.Add(new System.Net.Mail.MailAddress(r.Email));
                    mailMessage.Subject = emailSubject;
                    mailMessage.Body = emailBody;
                    mailMessage.IsBodyHtml = true;

                    using (var smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new System.Net.NetworkCredential(
                            "crisivan.javen.9@gmail.com",
                            "mcas tuum jsjc yrbf");

                        smtpClient.Timeout = 30000;
                        smtpClient.Send(mailMessage);
                    }
                }

                resp.isSuccess = true;
                resp.Message = "Verification code sent to your email.";
                LogAPICalls("/BaseCode/SendVerificationEmail", r, resp, "");
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Failed to send verification email: " + ex.Message;
                LogAPICalls("/BaseCode/SendVerificationEmail", r, resp, ex.Message);
            }

            return resp;
        }
        public GenericAPIResponse VerifyEmail(VerifyEmailRequest r)
        {
            GenericAPIResponse resp = new GenericAPIResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT USER_ID, STATUS, EXPIRY_TIME 
                           FROM EMAIL_VERIFICATION 
                           WHERE EMAIL = @EMAIL 
                           AND TOKEN = @TOKEN
                           ORDER BY CREATE_DATE DESC 
                           LIMIT 1";

                    MySqlCommand selectCmd = new MySqlCommand(sql, conn);
                    selectCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                    selectCmd.Parameters.Add(new MySqlParameter("@TOKEN", r.Token));

                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            resp.isSuccess = false;
                            resp.Message = "Invalid verification code.";
                            LogAPICalls("/BaseCode/VerifyEmail", r, resp, resp.Message);
                            return resp;
                        }

                        string status = reader.GetString("STATUS");
                        DateTime expiryTime = reader.GetDateTime("EXPIRY_TIME");
                        reader.Close();

                        if (status != "ACTIVE")
                        {
                            resp.isSuccess = false;
                            resp.Message = $"Verification code is {status.ToLower()}.";
                            LogAPICalls("/BaseCode/VerifyEmail", r, resp, resp.Message);
                            return resp;
                        }

                        if (DateTime.Now > expiryTime)
                        {
                            string updateExpiredSql = "UPDATE EMAIL_VERIFICATION SET STATUS = 'EXPIRED' WHERE EMAIL = @EMAIL AND TOKEN = @TOKEN";
                            MySqlCommand updateExpiredCmd = new MySqlCommand(updateExpiredSql, conn);
                            updateExpiredCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                            updateExpiredCmd.Parameters.Add(new MySqlParameter("@TOKEN", r.Token));
                            updateExpiredCmd.ExecuteNonQuery();

                            resp.isSuccess = false;
                            resp.Message = "Verification code has expired.";
                            LogAPICalls("/BaseCode/VerifyEmail", r, resp, resp.Message);
                            return resp;
                        }

                        string updateUsedSql = "UPDATE EMAIL_VERIFICATION SET STATUS = 'USED' WHERE EMAIL = @EMAIL AND TOKEN = @TOKEN";
                        MySqlCommand updateUsedCmd = new MySqlCommand(updateUsedSql, conn);
                        updateUsedCmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                        updateUsedCmd.Parameters.Add(new MySqlParameter("@TOKEN", r.Token));
                        updateUsedCmd.ExecuteNonQuery();

                        resp.isSuccess = true;
                        resp.Message = "Email verification successful.";
                    }

                    conn.Close();
                }
                LogAPICalls("/BaseCode/VerifyEmail", r, resp, resp.isSuccess ? "" : resp.Message);
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Error verifying email: " + ex.Message;
                LogAPICalls("/BaseCode/VerifyEmail", r, resp, ex.Message);
            }
            return resp;
        }
    }
}