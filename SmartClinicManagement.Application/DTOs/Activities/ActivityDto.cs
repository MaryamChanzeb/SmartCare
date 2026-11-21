using System;
using System.Collections.Generic;
using System.Text;

namespace SmartClinicManagement.Application.DTOs.Activities
{

    public class ActivityDto
    {
        public int ActivityID { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? MenuURL { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
