using System.ComponentModel.DataAnnotations;

namespace WebApiTutorial.Models {

    public class PollOption {
        [Key]
        public int ID { get; set; }

        public string Text { get; set; }
        public List<User> VotedUsers { get; set; } = new();
    }

    public class Poll {
        public int ID { get; set; }

        public string Title { get; set; }
        public string Text { get; set; }

        public List<PollOption> Options { get; set; }
    }
}