﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: GenericRepository.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

using Dapper.Contrib.Extensions;

using NLog;

using PlexRequests.Helpers;

namespace PlexRequests.Store.Repository
{
    public class GenericRepository<T> : IRepository<T> where T : Entity
    {
        private ICacheProvider Cache { get; }
        public GenericRepository(ISqliteConfiguration config, ICacheProvider cache)
        {
            Config = config;
            Cache = cache;
        }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        private ISqliteConfiguration Config { get; }
        public long Insert(T entity)
        {
            ResetCache();
            using (var cnn = Config.DbConnection())
            {
                cnn.Open();
                return cnn.Insert(entity);
            }
        }

        public IEnumerable<T> GetAll()
        {

            using (var db = Config.DbConnection())
            {
                db.Open();
                var result = db.GetAll<T>();
                return result;
            }

        }

        public T Get(string id)
        {
            throw new NotSupportedException("Get(string) is not supported. Use Get(int)");
        }

        public T Get(int id)
        {
            var key = "Get" + id;
            var item = Cache.GetOrSet(
                key,
                () =>
                {
                    using (var db = Config.DbConnection())
                    {
                        db.Open();
                        return db.Get<T>(id);
                    }
                });
            return item;
        }

        public void Delete(T entity)
        {
            ResetCache();
            using (var db = Config.DbConnection())
            {
                db.Open();
                db.Delete(entity);
            }
        }

        public bool Update(T entity)
        {
            ResetCache();
            Log.Trace("Updating entity");
            Log.Trace(entity.DumpJson());
            using (var db = Config.DbConnection())
            {
                db.Open();
                return db.Update(entity);
            }
        }

        public bool UpdateAll(IEnumerable<T> entity)
        {
            ResetCache();
            Log.Trace("Updating all entities");
            var result = new HashSet<bool>();

            using (var db = Config.DbConnection())
            {
                db.Open();
                foreach (var e in entity)
                {
                    result.Add(db.Update(e));
                }
            }
            return result.All(x => true);
        }

        private void ResetCache()
        {
            Cache.Remove("Get");
            Cache.Remove("GetAll");
        }
    }
}
