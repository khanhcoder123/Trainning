using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Tranning.Validations;

namespace Tranning.Models
{
    public class TopicModel
    {
        public List<TopicDetail> TopicDetailLists { get; set; }
    }

    public class TopicDetail
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Please enter the course ID.")]
        public int course_id { get; set; }

        public string name { get; set; }
        public string? description { get; set; }
        public string? videos { get; set; }
        public string documents { get; set; }
        public string attach_file { get; set; }

        [Required(ErrorMessage = "Please choose a file.")]
        [AllowedExtensionFile(new string[] { ".doc", ".jpg", ".jpeg", ".gif" })]
        [AllowedSizeFile(8 * 1024 * 1024)]
        public IFormFile file { get; set; }

        [Required(ErrorMessage = "Please choose a status.")]
        public string status { get; set; }

        [Required(ErrorMessage = "Please choose a file.")]
        [AllowedExtensionFile(new string[] { ".png", ".jpg", ".jpeg", ".gif" })]
        [AllowedSizeFile(4 * 1024 * 1024)]
        public IFormFile photo { get; set; }

        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
