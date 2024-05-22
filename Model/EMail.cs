namespace EmailWorkerService.Model
{
    public class EMail
    {

        public string ToEmailAddress { get; set; }
        public string Subject { get; set; }
        public string BodyHTML { get; set; }

        public Smtp SmtpConfig { get; set; }
    }
}
