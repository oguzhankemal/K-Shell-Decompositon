using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4j
{
    class Program
    {
        private static bool isDeleted = false;
        
        //static List<int> ids = new List<int> { 1241, 1185, 1204, 1167, 1103, 1229, 1164, 1286, 1258,1179,1276,1168 };

        static void Main(string[] args)
        {
            List<OldNeo4j> old = new List<OldNeo4j>();

            Console.WriteLine("Connecting...");
            var client = new GraphClient(new Uri("http://localhost:7474/db/data/"), "neo4j", "123456");
            client.Connect();


            var employees = client.Cypher
            .Match("(employee:Employee)")
            //.Where((Employee employee) => employee.name == "Ür40")
            .Return<Node<Employee>>("employee")
            .Results;


            // Kullanıcıların takdir ettikleri kişilere
            foreach (var employee in employees)
            {
                var getLikes = client.Cypher
                .OptionalMatch("(user:Employee)-[TAKDIR_ALDI]-(friend:Takdir)")
                .Where((Employee user) => user.uID == employee.Data.uID)
                .AndWhere((Takdir friend) => friend.fromUserID != employee.Data.uID)
                .Return((user, friend) => new
                {
                    User = user.As<Employee>(),
                    Friends = friend.CollectAs<Takdir>()
                })
                .Results;

                OldNeo4j oldEmp = new OldNeo4j();
                oldEmp.uID = employee.Data.uID;
                oldEmp.User = getLikes.FirstOrDefault().User;
                oldEmp.Friends = getLikes.FirstOrDefault().Friends;
                old.Add(oldEmp);
            }

            try
            {
                if (!isDeleted)
                {
                    Console.WriteLine("Deleting old nodes and edges...");
                    client.Cypher
                       .Start(new { n = All.Nodes })
                       .Match("(n)-[r]-(x)")
                       .With("n, r")
                       .Delete("n, r")
                       .ExecuteWithoutResults();

                    client.Cypher
                       .Start(new { n = All.Nodes })
                       .Delete("n")
                       .ExecuteWithoutResults();

                    Console.WriteLine("Creating new nodes...");
                    CreateUsers(client, employees);

                    isDeleted = true;
                }

            }
            catch (Exception)
            {
                isDeleted = false;
            }


            if (isDeleted)
            {

                Console.WriteLine("Creating new edges...");
                foreach (var employee in old)
                {

                    foreach (var item in employee.Friends)
                    {
                        client.Cypher
                        .Match("(user1:Employee)", "(user2:Employee)")
                        .Where((Employee user1) => user1.uID == employee.uID)
                        .AndWhere((Employee user2) => user2.uID == item.fromUserID)
                        .CreateUnique("(user2)-[:TAKDIR_ETTI]->(user1)")
                        .ExecuteWithoutResults();

                        client.Cypher
                        .Match("(user1:Employee)", "(user2:Employee)")
                        .Where((Employee user1) => user1.uID == employee.uID)
                        .AndWhere((Employee user2) => user2.uID == item.fromUserID)
                        .CreateUnique("(user1)-[:TAKDIR_ALDI]->(user2)")
                        .ExecuteWithoutResults();
                    }

                }
            }
            
            Console.WriteLine("Successfully Completed");

            Console.ReadLine();
        }

        private static void CreateUsers(GraphClient client, IEnumerable<Node<Employee>> employees)
        {
            // Yeni neo4j için tüm kullanıcıları create ediyoruz
            foreach (var employee in employees)
            {
                var newEmployee = new Employee {
                    uID = employee.Data.uID,
                    name = employee.Data.name,
                    lmsMemberID = employee.Data.lmsMemberID,
                    gender = employee.Data.gender,
                    isCompleteProfile = employee.Data.isCompleteProfile,
                    fullName = employee.Data.fullName,
                    isRegisterLMS = employee.Data.isRegisterLMS,
                    avatar = employee.Data.avatar,
                    isActive = employee.Data.isActive,
                    isFirstLoginOK = employee.Data.isFirstLoginOK,
                    lms_secretCode = employee.Data.lms_secretCode,
                    password = employee.Data.password,
                    aciklama = employee.Data.aciklama,
                    lmsRegisterDate = employee.Data.lmsRegisterDate,
                    surname = employee.Data.surname,
                    activationCode = employee.Data.activationCode,
                    birtDate = employee.Data.birtDate,
                    userType = employee.Data.userType,
                    activationShortCode = employee.Data.activationShortCode,
                    email = employee.Data.email,
                };

                client.Cypher
                    .Merge("(employee:Employee { Id: {id} })")
                    .OnCreate()
                    .Set("employee = {newEmployee}")
                    .WithParams(new
                    {
                        id = newEmployee.uID,
                        newEmployee
                    })
                    .ExecuteWithoutResults();

            }

            isDeleted = true;
        }

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
        
        public class Takdir
        {
            public string date { get; set; }
            public string postText { get; set; }
            public string nID { get; set; }
            public int totalBalance { get; set; }
            public int fromUserID { get; set; }
            public string postName { get; set; }
            public int languageID { get; set; }
            public string lmsReturnCode { get; set; }

        }
        public class OldNeo4j
        {
            public int uID { get; set; }
            public Employee User { get; set; }
            public IEnumerable<Takdir> Friends { get; set; }
        }
    }
}