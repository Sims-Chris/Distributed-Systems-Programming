using System.ComponentModel.DataAnnotations;

namespace DistSysAcwServer.Models
{
    public class LogArchive
    {
        public LogArchive() { }

        [Key]
        public int LogArchiveId { get; set; }

        [Required]
        public string LogString { get; set; }

        [Required]
        public DateTime LogDateTime { get; set; }

        // Store the ApiKey as a simple string so it persists after user deletion
        public string OriginalApiKey { get; set; }
    }
}