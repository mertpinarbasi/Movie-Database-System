namespace Movie_Database_System.Models
{
    public class Actor
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public int age { get; set; }
        
        public Actor(string name, string surname, int age)
        {
            this.name = name;
            this.surname = surname;
            this.age = age;
        }

        public Actor(string name, string surname, int age, int id)
        {
            this.name = name;
            this.surname = surname;
            this.age = age;
            this.id = id;
        }

        public Actor()
        {
        }
    }
}
