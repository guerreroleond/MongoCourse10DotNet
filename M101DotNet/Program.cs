using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M101DotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            //MainAsync(args).GetAwaiter().GetResult();
            FindAndModifyAsync(args).GetAwaiter().GetResult();
            Console.WriteLine();
            Console.WriteLine("PressEnter");
            Console.ReadLine();
        }
        
        static async Task FindAndModifyAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("widgets");
            await db.DropCollectionAsync("widgets");
            var docs = Enumerable.Range(0, 10).Select(i => new BsonDocument("_id", i).Add("x", i));
            await col.InsertManyAsync(docs);


            var result = await col.FindOneAndDeleteAsync<BsonDocument>(
                Builders<BsonDocument>.Filter.Gt("x", 5),
                new FindOneAndDeleteOptions<BsonDocument, BsonDocument>
                {
                    Sort = Builders<BsonDocument>.Sort.Descending("x")
                });

            //var result = await col.FindOneAndUpdateAsync<BsonDocument>(
            //    Builders<BsonDocument>.Filter.Gt("x", 5),
            //    Builders<BsonDocument>.Update.Inc("x", 1),
            //    new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
            //    {
            //        ReturnDocument = ReturnDocument.After,
            //        Sort = Builders<BsonDocument>.Sort.Descending("x")
            //    });

            Console.WriteLine(result);
            Console.WriteLine();

            //var result = await col.FindOneAndUpdateAsync(
            //    Builders<BsonDocument>.Filter.Gt("x", 5),
            //    Builders<BsonDocument>.Update.Inc("x", 5));

            await col.Find(new BsonDocument())
                .ForEachAsync(x => Console.WriteLine(x));
        }

        static async Task DeleteAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("widgets");
            await db.DropCollectionAsync("widgets");
            var docs = Enumerable.Range(0, 10).Select(i => new BsonDocument("_id", i).Add("x", i));
            await col.InsertManyAsync(docs);
            // deletes all the documents where x is greater than 5
            var result = await col.DeleteManyAsync(
               Builders<BsonDocument>.Filter.Gt("x", 5));                
                                   
            // deletes the first document when x is greagter than 5
            //var result = await col.DeleteOneAsync(
            //    Builders<BsonDocument>.Filter.Gt("x", 5));                

            await col.Find(new BsonDocument())
                .ForEachAsync(x => Console.WriteLine(x));
        }

        static async Task UpdateAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("widgets");
            await db.DropCollectionAsync("widgets");
            var docs = Enumerable.Range(0, 10).Select(i => new BsonDocument("_id", i).Add("x", i));
            await col.InsertManyAsync(docs);
            // increment many by 10 when x: greater than 5
            var result = await col.UpdateManyAsync(
                Builders<BsonDocument>.Filter.Gt("x", 5),
                Builders<BsonDocument>.Update.Inc("x", 10));

            // increment { x: 5 } by 10
            //var result = await col.UpdateOneAsync(
            //    Builders<BsonDocument>.Filter.Eq("x", 5),
            //    new BsonDocument("$inc", new BsonDocument("x", 10)));

            await col.Find(new BsonDocument())
                .ForEachAsync(x => Console.WriteLine(x));
        }

        static async Task UpdateReplaceAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("widgets");
            await db.DropCollectionAsync("widgets");
            var docs = Enumerable.Range(0, 10).Select(i => new BsonDocument("_id", i).Add("x", i));
            await col.InsertManyAsync(docs);
            
            // replace document that does not exist, wont do anything
            // result. MatchedCount = 0, .ModifiedCount = 0, .UpsertedId = null
            var result = await col.ReplaceOneAsync(
                new BsonDocument("x", 10),
                new BsonDocument("x", 30));

            // replace document {_id:5,x:5} with {_id:5, x:30} _id MOST BE THE SAME!
            //var result = await col.ReplaceOneAsync(
            //    new BsonDocument("_id", 5), 
            //    new BsonDocument("_id", 5).Add("x", 30));

            await col.Find(new BsonDocument())
                .ForEachAsync(x => Console.WriteLine(x));
        }

        static async Task FindProjectionsAnonymous(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<Person>("people");
            // projection with anonymous type
            var list = await col.Find(new BsonDocument())
                .Project(x => new { x.Name, CalcAge = x.Age + 20 })
                .ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindProjectionsStrong(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<Person>("people");
            // projection with builder & expression tree
            var list = await col.Find(new BsonDocument())
                .Project<Person>(Builders<Person>.Projection.Include(x => x.Name).Exclude(X => X.Id))
                .ToListAsync();
            // projection with builder
            //var list = await col.Find(new BsonDocument())
            //    .Project<Person>(Builders<Person>.Projection.Include("Name").Exclude("_id"))
            //    .ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }
        
        static async Task FindProjections(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");
            // projection with builder
            var list = await col.Find(new BsonDocument())
                .Project(Builders<BsonDocument>.Projection.Include("Name").Exclude("_id"))
                .ToListAsync();
            // projection with bson doc
            //var list = await col.Find(new BsonDocument())
            //    .Project(new BsonDocument("Name", 1).Add("_id", 0))
            //    .ToListAsync();
            // projection
            //var list = await col.Find(new BsonDocument())
            //    .Project("{Name: 1, _id:0}")
            //    .ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindLimitAndSkipStrongAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<Person>("people");
            // sort by methods
            var list = await col.Find(new BsonDocument())
                .SortBy(x => x.Age)
                .ThenByDescending(x => x.Name)
                .ToListAsync();
            // sort with builder
            //var list = await col.Find(new BsonDocument())
            //    .Sort(Builders<Person>.Sort.Ascending(x => x.Name))
            //    .ToListAsync();
            
            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindLimitAndSkipAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");
            // sort with builder
            var list = await col.Find(new BsonDocument())
                .Sort(Builders<BsonDocument>.Sort.Ascending("Age").Descending("Name"))
                .ToListAsync();

            // with sort
            //var list = await col.Find(new BsonDocument())
            //    .Sort(new BsonDocument("Age", 1))
            //    .ToListAsync();

            //var list = await col.Find(new BsonDocument())
            //    .Skip(5)
            //    .Limit(3)
            //    .ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindFilterBuilderStrongAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<Person>("people");

            var builder = Builders<Person>.Filter;
            //var filter = builder.Lt("Age", 33);
            //var filter = builder.And(builder.Lt("Age", 30), builder.Eq("Name", "Jones"));
            //var filter = builder.Lt("Age", 30) & builder.Eq("Name", "Jones");
            //var filter = builder.Lt("Age", 30) & !builder.Eq("Name", "Smith");

            // with expression trees
            //var filter = !builder.Eq(x => x.Name, "Smith") & builder.Lt(x => x.Age, 30);

            // force the documents to live in memory
            //var list = await col.Find(filter).ToListAsync();

            // overload Find(expression tree)
            var list = await col.Find(x => x.Age < 30 && x.Name != "Smith")
                .ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindFilterBuilderAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");

            var builder = Builders<BsonDocument>.Filter;
            //var filter = builder.Lt("Age", 33);
            //var filter = builder.And(builder.Lt("Age", 30), builder.Eq("Name", "Jones"));
            //var filter = builder.Lt("Age", 30) & builder.Eq("Name", "Jones");
            var filter = builder.Lt("Age", 30) & !builder.Eq("Name", "Smith");

            // force the documents to live in memory
            var list = await col.Find(filter).ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindFilterAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");
            //var filter = new BsonDocument("Name", "Smith");
            //var filter = new BsonDocument("Age", new BsonDocument("$lt", 30));
            var filter = new BsonDocument("$and", new BsonArray
                {
                    new BsonDocument("Age", new BsonDocument("$lt", 50)),
                    new BsonDocument("Name", "Perez")
                });
            // force the documents to live in memory
            var list = await col.Find(filter).ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindListForEachAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");
            // force the documents to live in memory
            await col.Find(new BsonDocument())
                .ForEachAsync(doc => Console.WriteLine(doc));
        }

        static async Task FindListAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");
            // force the documents to live in memory
            var list = await col.Find(new BsonDocument()).ToListAsync();

            foreach (var doc in list)
                Console.WriteLine(doc);
        }

        static async Task FindAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");

            using (var cursor = await col.Find(new BsonDocument()).ToCursorAsync())
            {
                while(await cursor.MoveNextAsync())
                {
                    foreach (var doc in cursor.Current)
                        Console.WriteLine(doc);
                }
            }
        }

        static async Task InsertOneStrongTyped(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<Person>("people");

            var doc = new Person
            {
                Name = "Jones",
                Age = 24,
                Profession = "Hacker"
            };

            await col.InsertOneAsync(doc);
        }

        static async Task InsertManyAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");

            var doc = new BsonDocument
            {
                { "Name", "Perez"},
                { "Age", 40},
                { "Profession", "Dev"}
            };

            var doc2 = new BsonDocument
            {
                {"SomethingElse", true}
            };

            await col.InsertManyAsync(new [] { doc, doc2});
        }

        static async Task InsertOneAsync(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("people");

            var doc = new BsonDocument
            {
                { "Name", "Smith"},
                { "Age", 30},
                { "Profession", "Hacker"}
            };

            await col.InsertOneAsync(doc);
        }

        static async Task MainAsync(string[] args)
        {
            //BsonClassMap.RegisterClassMap<Person>(cm =>
            //{
            //    cm.AutoMap();
            //    cm.MapMember(x => x.Name).SetElementName("name");
            //});

            var conventionPack = new ConventionPack();
            conventionPack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register("camelCase", conventionPack, t => true);

            var person = new Person
            {
                Name = "Jones",
                Age = 30,
                //Colors = new List<string> { "red", "blue" },
                //Pets = new List<Pet> { new Pet { Name = "Fluffy", Type = "Pig" } },
                //ExtraElements = new BsonDocument("anotherName", "anotherValue")
            };

            using(var writer = new JsonWriter(Console.Out))
            {
                BsonSerializer.Serialize(writer, person);
            }

            //var connectionString = "mongodb://localhost:27017";
            //var client = new MongoClient(connectionString);

            //var db = client.GetDatabase("test");
            //var col = db.GetCollection<BsonDocument>("people");
        }

        class Person
        {
            public ObjectId Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }

            public string Profession { get; set; }

            public List<string> Colors { get; set; }

            public List<Pet> Pets { get; set; }

            public BsonDocument ExtraElements { get; set; }

            public override string ToString()
            {
                return string.Format("Id: {0}, Name: \"{1}\", Age: {2}, Profession: \"{3}\"",
                    Id, Name, Age, Profession);
            }

        }

        class Pet
        {
            public string Name { get; set; }

            public string Type { get; set; }
        }
    }
}
