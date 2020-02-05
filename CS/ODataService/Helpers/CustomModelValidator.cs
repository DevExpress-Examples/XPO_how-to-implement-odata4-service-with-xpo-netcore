using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Concurrent;

namespace ODataService.Helpers {
    class CustomModelValidator : IObjectModelValidator {
        readonly static ConcurrentDictionary<Type, bool> persistentTypes = new ConcurrentDictionary<Type, bool>();
        public void Validate(ActionContext actionContext, ValidationStateDictionary validationState, string prefix, object model) {
            bool skipValidate = model != null && persistentTypes.GetOrAdd(model.GetType(), t => !typeof(IXPSimpleObject).IsAssignableFrom(t));
            if(skipValidate) {
                actionContext.ModelState.Clear();
            }
        }
    }
}