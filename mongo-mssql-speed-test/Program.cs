using MongoDB.Bson;
using MongoDB.Driver;
using System.Data.SqlClient;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var mongoDatabaseName = "OnlineShopRecensionsDb";
        var mongoConnectionString = "mongodb://localhost:27017";
        var mssqlConnectionString = "Data Source=DESKTOP-39HT63M\\SQLEXPRESS;Initial Catalog=OnlineShopDB;Trusted_Connection=True;";

        var mongoClient = new MongoClient(mongoConnectionString);
        var dbMongo = mongoClient.GetDatabase(mongoDatabaseName);
        var collection = dbMongo.GetCollection<BsonDocument>("Recensions");


        var random = new Random();

        var mongodbStopwatch = new Stopwatch();
        mongodbStopwatch.Start();
        for (int i = 0; i < 1000; i++)
        {
            var document = new BsonDocument()
    {
        {"userId", random.Next(1,2)},
        {"markId", random.Next(1,5)},
        {"review", $"review reviewreviewreviewreviewreviewreview review{i}" },
        {"date", DateTime.Now.ToShortDateString() }
    };
            await collection.InsertOneAsync(document);
            var filter = Builders<BsonDocument>.Filter.Eq("review", $"review reviewreviewreviewreviewreviewreview review{i}");
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            var update = Builders<BsonDocument>.Update.Set("review", $"review{i}");
            await collection.UpdateOneAsync(filter, update);
            filter = Builders<BsonDocument>.Filter.Eq("review", $"review{i}");
            await collection.DeleteOneAsync(filter);
        }
        mongodbStopwatch.Stop();
        Console.WriteLine($"It takes {mongodbStopwatch.ElapsedMilliseconds} miliseconds to execute CRUD operations in mongodb");
        var mssqlStopwatch = new Stopwatch();

        using (SqlConnection connection = new SqlConnection(mssqlConnectionString))
        {
            try
            {
                await connection.OpenAsync();
                mssqlStopwatch.Start();
                for (int i = 0; i < 1000; i++)
                {
                    var sqlCreateQuery = $"INSERT INTO [dbo].[Recension] ([UserId],[MarkId],[Review],[CreateDate]) VALUES({random.Next(1, 2)},{random.Next(1, 5)},'review reviewreviewreviewreviewreviewreview review{i}','11-11-1111')";
                    var sqlReadQuery = $"SELECT * FROM [dbo].[Recension] WHERE [Review] = 'review reviewreviewreviewreviewreviewreview review{i}'";
                    var sqlUpdateQuery = $"UPDATE [dbo].[Recension] SET [UserId] = {random.Next(1, 2)}, [MarkId] = {random.Next(1, 5)}, [Review] = 'review{i}', [CreateDate] = '10-10-1010' WHERE [Review] = 'review reviewreviewreviewreviewreviewreview review{i}'";
                    var sqlDeleteQuery = $"DELETE FROM [dbo].[Recension] WHERE [Review] = 'review{i}'";
                    using (SqlCommand command = new SqlCommand(sqlCreateQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    using (SqlCommand command = new SqlCommand(sqlReadQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    using (SqlCommand command = new SqlCommand(sqlUpdateQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    using (SqlCommand command = new SqlCommand(sqlDeleteQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                mssqlStopwatch.Stop();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            Console.WriteLine($"It takes {mssqlStopwatch.ElapsedMilliseconds} miliseconds to execute CRUD operations in mssql");
        }
    }
}