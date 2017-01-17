using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region ReadMe

/*

    2017.01.16
    Oguzhan Kemal
    oguzhankml@gmail.com

    This project is developed to read datas from neo4j data set and find most efficent spreader by using k-shell decomposition

    Neo4jClient 2.0.0 is used to connect neo4j data set

    K-shell decompositon is used to find most efficent spreader from data set
    In order to get detailed information about k-shell decomposition, please look at below report
    Kitsak, M., Gallos, L. K., Havlin, S., Liljeros, F., Muchnik, L., Stanley, H. E., & Makse, H. A. (2010). 
        Identification of influential spreaders in complex networks. Nature physics, 6(11), 888-893

*/

#endregion



namespace SNAProject
{
    class Spreaders
    {
        static void Main(string[] args)
        {
            try
            {
                #region Connection

                //Neo4jClient 2.0.0 is used to connect and use the database
                //Neo4j client is a .NET client for neo4j. 
                //Supports basic CRUD operations, Cypher and Gremlin queries via fluent interfaces, and some indexing operations.


                Console.WriteLine("Connecting to database...");
                //Creating client to connect db
                var client = new GraphClient(new Uri("http://localhost:7474/db/data/"), "neo4j", "123456");
                //Connecting to the database
                client.Connect();

                Console.WriteLine("Successfully connected.");

                #endregion

                #region Get All Employees From DataSet

                Console.WriteLine("Loading dataset...");

                //Getting all employees from database
                //Put them into employees variable
                //We parse the return type into Employee Class which is created into Model class
                var employees = client.Cypher
                .Match("(employee:Employee)")
                .Return<Node<Employee>>("employee")
                .Results;

                //Creating ans initializing Like list
                List<Likes> likes = new List<Likes>();

                //Creating ans initializing Nodes list
                List<Nodes> nodes = new List<Nodes>();


                //For each employee, we find related nodes
                //Related nodes are also Employee
                //A Likes class has two variable, the first one is the employee
                //The second one is the employees who have 'TAKDIR_ETTI' relation with the employee
                //For each employee, we put the employee into employee class, and we put the others into friends list
                foreach (var employee in employees)
                {
                    //Find all employees which have 'TAKDIR_ETTI' relation with the employee
                    likes = client.Cypher
                    .OptionalMatch("(user:Employee)-[TAKDIR_ETTI]->(friend:Employee)")
                    .Where((Employee user) => user.uID == employee.Data.uID)
                    .Return((user, friend) => new Likes
                    {
                        employee = user.As<Employee>(),
                        friends = friend.CollectAs<Employee>()
                    })
                    .Results.ToList();

                    // We also have Nodes list
                    // This list has node for each employee
                    // Each node has specific employee id, k degree, k-shell degree, neighbour count of employee 
                    // and neighbour id list of employees who have 'TAKDIR_ETTI' relation with the employee
                    Nodes n = new Nodes();
                    n.id = employee.Data.uID;
                    //At the beginning we dont know the k degree value, so we simply write 0
                    n.k = 0;
                    //At the beginning we dont know the neighbours count, so we simply write 0
                    n.neighboursCount = 0;
                    n.neighboursId = new List<int>();

                    //In this step, we look at each friends of the employee
                    // and for each friend, we increase the k value and neighbours count value
                    // we also add friend's id into the neighboursId list
                    foreach (var item in likes.FirstOrDefault().friends)
                    {
                        n.k++;
                        n.neighboursId.Add(item.uID);
                        n.neighboursCount = n.k;
                    }

                    //At the end, we add this node into Nodes list
                    nodes.Add(n);
                }

                Console.WriteLine("Successfully loaded");

                // At the end of this step, we have Nodes list which have specific node for each employee
                // Each node has employee id, k degree, neighbour count of employee, 
                // neighbour id list of employees who have 'TAKDIR_ETTI' relation with the employee, and k-shell degree which is null,
                #endregion

                #region K-Shell Decomposition

                Console.WriteLine("K-Shell Decomposition is being applying...");
                //In this algorithm, we trying to find k-shell values by using k-shell decompositon
                // In order to do this, firstly we reorder the nodes ascending the neighbours count
                List<Nodes> ordered = nodes.OrderBy(kvalue => kvalue.neighboursCount).ToList();

                //After reordering, we call prude node function recursively
                //KShellDecompositon function accept node list and integer number for minmum k-shell value
                //This function find the k-shell value of the first element of the ordered employee list
                //and remove it from all employee's neighbour list and the ordered list 
                //For the first node, we send minimum k-shell value(minks) 0.
                KShellDecompositon(ordered, 0);
                
                #endregion

                #region Write Results

                Console.WriteLine("Employees ordered by their k-shell values :");

                //The nodes list is ordered  by descending k-shell value
                foreach (var item in ordered.OrderByDescending(o => o.k_shell))
                {
                    //In order to get employee's specific values, we find it from employee list by using Id of the employee
                    Node<Employee> em = employees.Where(e => e.Data.uID == item.id).FirstOrDefault();

                    //Then we write the k-shell value, k degree and name of the employee name
                    Console.WriteLine("[ id : " + item.id + " ] - [ k-shell : " + item.k_shell + " ] - [ k : " + item.k + " ] - [ Name : " + em.Data.fullName + " ]");
                }

                #endregion

                
                Console.ReadLine();
            }
            catch (Exception err)
            {

                Console.WriteLine("There is an error : " + err.ToString());
            }
            
        }

        #region KShellDecompositon Function
        //KShellDecompositon function accept node list and integer number for minmum k-shell value 
        //This function find the k-shell value of the first element of the ordered employee list
        //and remove it from all employee's neighbour list and the ordered list
        // At the end, it calls PruneNode function recursively with the ordered list and minimum k-shell degree
        private static void KShellDecompositon(List<Nodes> ordered, int minks)
        {
            //Get the first node which has minimum neighbour count
            Nodes node = ordered.FirstOrDefault();

            if (node.neighboursCount == 0)
            {
                //If neighbours count is zero, we simply set node's k-shell value to zero
                node.k_shell = minks;
            }
            else if (node.neighboursCount == 1)
            {
                //If neighbours count is one and minimum k-shell value is zero, 
                // we set node's k-shell value to one
                // and set also minumum k-shell value to 1 
                if (minks == 0)
                {
                    node.k_shell = 1;
                    minks = 1;
                }
                else
                    //If neighbours count is not zero, we simply set node's k-shell value to minimum k-shell value
                    node.k_shell = minks;
            }
            else
            {

                //If neighbours count is smaller than the minimum k-shell value
                if (node.neighboursCount < minks)
                {
                    //node's k-shell value is changed with minumum k-shell value
                    node.k_shell = minks;
                }
                else
                {
                    //If neighbours count is not smaller than the minimum k-shell value,
                    // we set node's k-shell value to neighbours count
                    // and set also minumum k-shell value to neighbours count
                    minks = node.neighboursCount;
                    node.k_shell = minks;
                }
            }

            //If nodes has neighbours we have to prune the employee from them
            if (node.neighboursCount > 0)
            {
                // we look at rest of the nodes 
                foreach (var item in ordered)
                {
                    // if any of them has this employee as a neighbour, it removed drom neighbourId list
                    bool removed = item.neighboursId.Remove(node.id);
                    if (removed)
                        // The neighbours Count is also decreased
                        item.neighboursCount--;
                }
            }
            // Lastly, this node removed from ordered list
            ordered.Remove(node);

            //If there is any node in nodes list, we contunie to calculation
            //This repeats until there is no node into ordered list
            if (ordered.Count > 0)
                KShellDecompositon(ordered.OrderBy(n => n.neighboursCount).ToList(), minks);

        }
        #endregion

    }
}
