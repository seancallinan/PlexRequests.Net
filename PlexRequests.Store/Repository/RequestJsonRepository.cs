﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SettingsJsonRepository.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System.Collections.Generic;
using System.Linq;

using Dapper.Contrib.Extensions;

using PlexRequests.Helpers;
using PlexRequests.Store.Models;

namespace PlexRequests.Store.Repository
{
    public class RequestJsonRepository : IRequestRepository
    {
        private ICacheProvider Cache { get; }
        
        public RequestJsonRepository(ISqliteConfiguration config, ICacheProvider cacheProvider)
        {
            Db = config;
            Cache = cacheProvider;
        }

        private ISqliteConfiguration Db { get; }

        public long Insert(RequestBlobs entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                var id = con.Insert(entity);
                return id;
            }
        }

        public IEnumerable<RequestBlobs> GetAll()
        {
            var key = "GetAll";
            var item = Cache.GetOrSet(key, () =>
            {
                using (var con = Db.DbConnection())
                {
                    var page = con.GetAll<RequestBlobs>();
                    return page;
                }
            }, 5);
            return item;
        }

        public RequestBlobs Get(int id)
        {
            var key = "Get" + id;
            var item = Cache.GetOrSet(key, () =>
            {
                using (var con = Db.DbConnection())
                {
                    var page = con.Get<RequestBlobs>(id);
                    return page;
                }
            }, 5);
            return item;
        }

        public bool Delete(RequestBlobs entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return con.Delete(entity);
            }
        }

        public bool Update(RequestBlobs entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return con.Update(entity);
            }
        }

        private void ResetCache()
        {
            Cache.Remove("Get");
            Cache.Remove("GetAll");
        }

        public bool UpdateAll(IEnumerable<RequestBlobs> entity)
        {
            ResetCache();
            var result = new HashSet<bool>();
            using (var db = Db.DbConnection())
            {
                db.Open();
                foreach (var e in entity)
                {
                    result.Add(db.Update(e));
                }
            }
            return result.All(x => true);
        }
    }
}
