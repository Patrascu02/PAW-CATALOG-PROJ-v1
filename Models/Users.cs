using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace PAW_CATALOG_PROJ.Models
{
    public class User : IdentityUser
    {
        

        public string Name { get; set; } = null!;

        // lists of enrollments, messages for each user
        public ICollection<Enrollment> EnrollmentsAsStudent { get; set; } = new List<Enrollment>();
        public ICollection<Enrollment> EnrollmentsAsTeacher { get; set; } = new List<Enrollment>();

        // messages sent and received by the user
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}
