using System;
using MySql.Data.MySqlClient;

namespace BaseCode.Models
{
    public class TemplateService
    {
        private string ConnectionString { get; set; }

        public TemplateService(string connStr)
        {
            this.ConnectionString = connStr;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public EmailTemplate GetTemplateByCode(string templateCode)
        {
            EmailTemplate template = null;

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM TEMPLATE WHERE TEMPLATE_CODE = @TEMPLATE_CODE AND TEMPLATE_STATUS = 'A' LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.Add(new MySqlParameter("@TEMPLATE_CODE", templateCode));

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        template = new EmailTemplate
                        {
                            TemplateId = reader.GetInt64("TEMPLATE_ID"),
                            TemplateCode = reader.GetString("TEMPLATE_CODE"),
                            TemplateName = reader.GetString("TEMPLATE_NAME"),
                            TemplateDescription = reader.GetString("TEMPLATE_DESCRIPTION"),
                            TemplateContent = reader.GetString("TEMPLATE_CONTENT"),
                            TemplateSubject = reader.GetString("TEMPLATE_SUBJECT"),
                            TemplateFooter = reader.IsDBNull(reader.GetOrdinal("TEMPLATE_FOOTER")) ? "" : reader.GetString("TEMPLATE_FOOTER"),
                            TemplateHeader = reader.IsDBNull(reader.GetOrdinal("TEMPLATE_HEADER")) ? "" : reader.GetString("TEMPLATE_HEADER"),
                            TemplateStatus = reader.GetString("TEMPLATE_STATUS")
                        };
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching template: " + ex.Message);
            }

            return template;
        }
    }
}