using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNAProject
{
    //Employee model holds all features of the employee's
    public class Employee
    {
        public string name { get; set; }
        public string surname { get; set; }
        public int uID { get; set; }
        public string email { get; set; }
        public string lmsMemberID { get; set; }
        public string gender { get; set; }
        public bool isCompleteProfile { get; set; }
        public bool isRegisterLMS { get; set; }
        public string avatar { get; set; }
        public bool isFirstLoginOK { get; set; }
        public string isActive { get; set; }
        public string fullName { get; set; }
        public string lms_secretCode { get; set; }
        public string password { get; set; }
        public string aciklama { get; set; }
        public string lmsRegisterDate { get; set; }
        public string activationCode { get; set; }
        public string birtDate { get; set; }
        public string userType { get; set; }
        public string activationShortCode { get; set; }
    }

    //Likes model holds the employee and his/her friends who has 'TAKDIR_ETTI' relation with the employee
    public class Likes
    {
        public Employee employee { get; set; }
        public IEnumerable<Employee> friends { get; set; }

    }

    //Nodes model holds if of the employee, k degree, k-shell degree, neighbour count and neighboursId list
    public class Nodes
    {
        public int id { get; set; }
        public int k { get; set; }
        public int k_shell { get; set; }
        public int neighboursCount { get; set; }
        public List<int> neighboursId { get; set; }
    }
}
