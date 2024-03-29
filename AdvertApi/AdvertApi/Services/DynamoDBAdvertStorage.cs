﻿using AdvertApi.DbModels;
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
            //var dbModel = _mapper.Map<AdvertDbModel>(model);
            AdvertDbModel dbModel = new AdvertDbModel()
            {
                Description = model.Description,
                Price = model.Price,
                Title = model.Title
            };
            dbModel.Id = Guid.NewGuid().ToString();
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

        public async Task<bool> CheckHealthAsync()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var tableData = await client.DescribeTableAsync("Adverts");
                    return string.Compare(tableData.Table.TableStatus, "active", true) == 0;
                }
            }
        }

        public async Task Confirm(ConfimAdvertModel model)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var record = await context.LoadAsync<AdvertDbModel>(model.Id);
                    if(record == null)
                    {
                        throw new KeyNotFoundException($"A record with ID= {model.Id} was not found.");
                    }
                    if(model.Status == AdvertStatus.Active)
                    {
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    }
                    else
                    {
                        await context.DeleteAsync(record);
                    }                  
                }
            }
            return;
        }

        public async Task<List<AdvertModel>> GetAllAsync()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var scanResult =
                        await context.ScanAsync<AdvertDbModel>(new List<ScanCondition>()).GetNextSetAsync();
                   // return scanResult.Select(item => _mapper.Map<AdvertModel>(item)).ToList();
                    return scanResult.Select(item => new AdvertModel() {Description = item.Description,
                                                                                        Price = item.Price,
                                                                                        Title = item.Title}).ToList();
                }
            }
        }

        public async Task<AdvertModel> GetByIdAsync(string id)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var dbModel = await context.LoadAsync<AdvertDbModel>(id);
                  //  if (dbModel != null) return _mapper.Map<AdvertModel>(dbModel);
                    if (dbModel != null) return new AdvertModel() {Description = dbModel.Description, Price = dbModel.Price, Title = dbModel.Title };
                }
            }

            throw new KeyNotFoundException();
        }

    }
}
