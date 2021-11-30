using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models
{
    public class Director
    {
        public string name { get; set; }
        public string surname { get; set; }
        public int age { get; set; }
        public int id { get; set; }

        public Director(string name, string surname, int age)
        {
            this.name = name;
            this.surname = surname;
            this.age = age;
        }

        public Director(string name, string surname, int age, int id)
        {
            this.name = name;
            this.surname = surname;
            this.age = age;
            this.id = id;
        }
    }
}
