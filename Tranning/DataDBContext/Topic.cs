using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Tranning.DataDBContext
{
    public class Topic
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("course_id")]
        public int course_id { get; set; }
        [Column("name", TypeName = "Varchar(50)"), Required]
        public string name { get; set; }

        [Column("description", TypeName = "Varchar(150)"), AllowNull]
        public string description { get; set; }
        [Column("videos", TypeName = "Varchar(150)"), AllowNull]
        public string videos { get; set; }
        [Column("documents", TypeName = "Varchar(150)"), AllowNull]
        public string? documents { get; set; }
        [Column("attach_file", TypeName = "Varchar(150)"), AllowNull]
        public string attach_file { get; set; }

        [Column("status", TypeName = "Varchar(200)"), Required]
        public string status { set; get; }

        [AllowNull]
        public DateTime? created_at { get; set; }
        [AllowNull]
        public DateTime? updated_at { get; set; }
        [AllowNull]
        public DateTime? deleted_at { get; set; }
    }
}