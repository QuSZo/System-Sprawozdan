namespace SystemSprawozdan.Backend.Data.Models.DbModels
{
    public class SubjectSubgroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SubjectGroupId { get; set; }
        public bool IsIndividual { get; set; } = true;

        public SubjectGroup SubjectGroup { get; set; }
        public ICollection<Student> Students { get; set; }
        public ICollection<StudentReport> StudentReports { get; set; }
    }
}
