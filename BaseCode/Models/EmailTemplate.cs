namespace BaseCode.Models
{
    public class EmailTemplate
    {
        public long TemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public string TemplateContent { get; set; }
        public string TemplateSubject { get; set; }
        public string TemplateFooter { get; set; }
        public string TemplateHeader { get; set; }
        public string TemplateStatus { get; set; }
    }
}