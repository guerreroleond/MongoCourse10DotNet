using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework3._1
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
            var db = client.GetDatabase("school");

            var coll = db.GetCollection<Student>("students");

            var list = await coll.Find(x => x.name != "").ToListAsync();
           

            foreach (var doc in list)
            {
                //var updated = await coll.UpdateOneAsync(
                //    Builders<BsonDocument>.Filter.Eq("_id", id),
                //    Builders<BsonDocument>.Update.Push("scores", )
                //    );
                var countHw = 0;
                double hw1 = 0;
                double hw2 = 0;

                foreach (var sco in doc.scores)
                {
                    if (sco.type == "homework")
                    {
                        if (hw1 == 0)
                        {
                             hw1 = sco.score;
                             countHw++;
                        }
                        else
                        {
                            hw2 = sco.score;
                            countHw++;
                        }
                    }
                } 
                
                var builder = Builders<Student>.Filter;
                var filter = builder.Eq(stu => stu.Id, doc.Id);

                if (hw1 < hw2)
                {
                    Console.WriteLine("Student {0} - lower homework: {1}", doc.name, hw1);
                    var update = Builders<Student>.Update.PullFilter("scores",
                        Builders<Score>.Filter.Eq("score", hw1) );
                    var result = coll.FindOneAndUpdateAsync(filter, update).Result;
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("Student {0} - lower homework: {1}", doc.name, hw2);
                    var update = Builders<Student>.Update.PullFilter("scores",
                        Builders<Score>.Filter.Eq("score", hw2));

                    var result = coll.FindOneAndUpdateAsync(filter, update).Result;
                    Console.WriteLine(result);
                }

            }
        }

        class Student
        {
            //public ObjectId Id { get; set; }
            public int Id { get; set; }

            public string name { get; set; }

            public List<Score> scores { get; set; }

        }

        class Score
        {
            public string type { get; set; }

            public double score { get; set; }
        }
    }
}
