using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework2._2
{
    class Program
    {
        static void Main(string[] args)
        {
            DoHomework(args).GetAwaiter().GetResult();
            Console.WriteLine();
            Console.WriteLine("Press Enter...");
            Console.ReadLine();
        }

        static async Task DoHomework(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("students");

            var coll = db.GetCollection<BsonDocument>("grades");

            var list = await coll.Find(new BsonDocument())
                .Sort(Builders<BsonDocument>.Sort
                .Ascending("student_id")
                .Ascending("type")
                .Ascending("score")
                ).ToListAsync();

            int count = 1;

            foreach (var doc in list)
            {
                if (count == 2)
                {
                    var id = doc.GetValue("_id");
                    Console.WriteLine(id);
                    var deleted = await coll.DeleteOneAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", id));
                    Console.WriteLine("deleted: " + deleted);
                }
                   

                Console.WriteLine(doc);

                if (count == 4)
                    count = 0;

                count++;
            }
                
        
        }
    }
}
