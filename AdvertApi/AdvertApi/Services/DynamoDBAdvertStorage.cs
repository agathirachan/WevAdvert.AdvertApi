using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public class DynamoDBAdvertStorage : IAdvertStorageService
    {

        private readonly IMapper _mapper;
        public DynamoDBAdvertStorage(IMapper mapper)
        {
            this._mapper = mapper;
        }
        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = _mapper.Map<AdvertDbModel>(model);
            dbModel.Id = new Guid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;
            using (var client = new AmazonDynamoDBClient())
            {
                using(var context=new DynamoDBContext(client))
                {
                   await context.SaveAsync(dbModel);
                }
            }
            return dbModel.Id;
        }

        public Task<bool> Confirm(ConfimAdvertModelcs model)
        {
            throw new NotImplementedException();
        }
    }
}
