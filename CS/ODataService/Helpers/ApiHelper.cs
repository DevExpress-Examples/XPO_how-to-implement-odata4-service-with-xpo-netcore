using System;
using System.Collections;
using System.Net;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;

namespace ODataService.Helpers {
    public static class ApiHelper {
        public static TEntity Patch<TEntity, TKey>(TKey key, Delta<TEntity> delta, UnitOfWork uow) where TEntity : class {
            TEntity existing = uow.GetObjectByKey<TEntity>(key);
            if(existing != null) {
                delta.CopyChangedValues(existing);
                uow.CommitChanges();
            }
            return existing;
        }

        public static int Delete<TEntity, TKey>(TKey key, UnitOfWork uow) {
            TEntity existing = uow.GetObjectByKey<TEntity>(key);
            if(existing == null) {
                return (int)HttpStatusCode.NotFound;
            }
            uow.Delete(existing);
            uow.CommitChanges();
            return (int)HttpStatusCode.NoContent;
        }

        public static int CreateRef<TEntity, TKey>(HttpRequest request, TKey key, string navigationProperty, Uri link, UnitOfWork uow) {
            TEntity entity = uow.GetObjectByKey<TEntity>(key);
            if(entity == null) {
                return (int)HttpStatusCode.NotFound;
            }
            var classInfo = uow.GetClassInfo<TEntity>();
            var memberInfo = classInfo.FindMember(navigationProperty);
            if(memberInfo == null) {
                return (int)HttpStatusCode.BadRequest;
            }
            object relatedKey = UriHelper.GetKeyFromUri<object>(request, link);
            if(memberInfo.IsAssociationList) {
                var reference = uow.GetObjectByKey(memberInfo.CollectionElementType, relatedKey);
                var collection = (IList)memberInfo.GetValue(entity);
                collection.Add(reference);
            } else if(memberInfo.ReferenceType != null) {
                var reference = uow.GetObjectByKey(memberInfo.ReferenceType, relatedKey);
                memberInfo.SetValue(entity, reference);
            } else {
                return (int)HttpStatusCode.BadRequest;
            }
            uow.CommitChanges();
            return (int)HttpStatusCode.NoContent;
        }

        public static int DeleteRef<TEntity, TKey>(HttpRequest request, TKey key, string navigationProperty, Uri link, UnitOfWork uow) {
            TEntity entity = uow.GetObjectByKey<TEntity>(key);
            if(entity == null) {
                return (int)HttpStatusCode.NotFound;
            }
            var classInfo = uow.GetClassInfo<TEntity>();
            var memberInfo = classInfo.FindMember(navigationProperty);
            if(memberInfo == null) {
                return (int)HttpStatusCode.BadRequest;
            }
            if(memberInfo.ReferenceType != null) {
                memberInfo.SetValue(entity, null);
            } else {
                return (int)HttpStatusCode.BadRequest;
            }
            uow.CommitChanges();
            return (int)HttpStatusCode.NoContent;
        }

        public static int DeleteRef<TEntity, TKey, TRelatedKey>(TKey key, TRelatedKey relatedKey, string navigationProperty, UnitOfWork uow) {
            TEntity entity = uow.GetObjectByKey<TEntity>(key);
            if(entity == null) {
                return (int)HttpStatusCode.NotFound;
            }
            var classInfo = uow.GetClassInfo<TEntity>();
            var memberInfo = classInfo.FindMember(navigationProperty);
            if(memberInfo == null || !memberInfo.IsAssociationList) {
                return (int)HttpStatusCode.BadRequest;
            }
            var reference = uow.GetObjectByKey(memberInfo.CollectionElementType, relatedKey);
            var collection = (IList)memberInfo.GetValue(entity);
            collection.Remove(reference);
            uow.CommitChanges();
            return (int)HttpStatusCode.NoContent;
        }
    }
}