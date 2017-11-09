﻿using System;
using System.Threading.Tasks;
using Proto.IMDG.PList;
using Proto.Remote;

namespace Proto.IMDG
{
    public class ListProxy<T>
    {
        private readonly string _name;

        public ListProxy(string name)
        {
            _name = name;
        }

        public async Task AddAsync(T item)
        {
            var pid = await GetPid();
            var msg = new AddRequest
            {
                Value = PSerializer.Serialize(item)
            };
            pid.Tell(msg);
        }

        public async Task<int> CountAsync()
        {
            var pid = await GetPid();
            var res = await pid.RequestAsync<CountResponse>(new CountRequest());
            return res.Value;
        }

        public async Task<T> Get(int index)
        {
            var pid = await GetPid();
            var res = await pid.RequestAsync<GetResponse>(new GetRequest {Index = index});
            return PSerializer.Deserialize<T>(res.Value);
        }

        private async Task<PID> GetPid()
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    var (pid, status) = await Cluster.Cluster.GetAsync(_name, "PList");
                    if (status == ResponseStatusCode.OK || status == ResponseStatusCode.ProcessNameAlreadyExist)
                        return pid;
                }
                catch
                {
             
                }
                await Task.Delay(i * 50);
            }
            throw new Exception("Retry error");
        }
    }
}