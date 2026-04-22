using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DistSysAcwServer.Models
{
    /// <summary>
    /// User data class
    /// </summary>
    public class User
    {
        #region Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute

        public User() {}

        [Key]
        public string? ApiKey{ get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        #endregion
        
        #region Task13
        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
        #endregion

    }
}