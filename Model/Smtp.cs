namespace EmailWorkerService.Model
{
    public class Smtp
    {
        public string ConnectionUsername { get; set; }
        public string ConnectionXWD { get; set; }
        public string ConnectionHost { get; set; }

        public int ConnectionPort { get; set; }
    }
}
