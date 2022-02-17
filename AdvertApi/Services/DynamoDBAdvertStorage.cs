using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using AutoMapper;

namespace AdvertApi.Services
{
    public class DynamoDBAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper mapper;

        public DynamoDBAdvertStorage(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = mapper.Map<AdvertDbModel>(model);
            dbModel.Id = new Guid().ToString();
            dbModel.CreationDateTime = DateTime.Now;
            dbModel.Status = AdvertStatus.Pending;

            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);
            await context.SaveAsync(dbModel);

            return dbModel.Id;
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var clientConfig = new AmazonDynamoDBConfig
                {
                    ServiceURL = "http://localhost:5025",
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };

                using var client = new AmazonDynamoDBClient(clientConfig);
                var tabledata = await client.DescribeTableAsync("Adverts");
                return string.Compare(tabledata.Table.TableStatus, "active", true) == 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// if s3 image was uploaded successfully then change the advert record status 
        /// if advert is not present in DynamoDb => no id present, then delete the record from the
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<bool> Confirm(ConfirmAdvertModel model)
        {
            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);
            var record = await context.LoadAsync<AdvertDbModel>(model.Id);

            if (record == null)
                throw new KeyNotFoundException($"A record with ID={model.Id} was not found");

            // model is coming from the MVC project
            if(model.Status == AdvertStatus.Active)
            {
                // change the actual record in database
                record.Status = AdvertStatus.Active;
                await context.SaveAsync(record);
            }
            else
            {
                await context.DeleteAsync(record);
            }

            return true;
        }

        public async Task<AdvertModel> GetById(string id)
        {
            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);
            var record = await context.LoadAsync<AdvertDbModel>(id);
            var advertModel = mapper.Map<AdvertModel>(record);

            return advertModel;
        }
    }
}
