using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DwComment.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public int ThreadId { get; set; }
        [Required]
        public string Nickname { get; set; }
        public string Mail { get; set; }
        public string Link { get; set; }
        [Required]
        public string Content { get; set; }
        public long Time { get; set; }
        public long ForwardTo { get; set; }
    }
}
