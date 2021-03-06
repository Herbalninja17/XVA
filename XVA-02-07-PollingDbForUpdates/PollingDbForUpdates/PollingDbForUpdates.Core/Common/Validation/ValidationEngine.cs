using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PollingDbForUpdates.Core.Interfaces.Validation;
	using PollingDbForUpdates.Core.Model;
	
namespace PollingDbForUpdates.Core.Common.Validation
{
    public static class ValidationEngine
    {
        /// <summary>
        /// Will validate an entity that implements IValidatableObject and DataAnnotations
        /// </summary>
        /// <typeparam name="T">The type that inherits the abstract basetype PersistentEntity</typeparam>
        /// <typeparam name="TA"></typeparam>
        /// <param name="entity">The Entity to validate</param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static IValidationContainer<T,TA> GetValidationContainer<T,TA>(this T entity, TA vm) where T : PersistentEntity
        {
            var brokenrules = new Dictionary<string, IList<string>>();

            //IValidatableObject
            var customErrors = entity.Validate(new ValidationContext(entity, null, null));
            if (customErrors != null)
                foreach (var customError in customErrors)
                {
                    if (customError == null) continue;
                    foreach (var memberName in customError.MemberNames)
                    {
                        if (!brokenrules.ContainsKey(memberName))
                            brokenrules.Add(memberName, new List<string>());
                        brokenrules[memberName].Add(customError.ErrorMessage);
                    }
                }

            //DataAnnotations
            foreach (var pi in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                foreach (var attribute in (ValidationAttribute[])pi.GetCustomAttributes(typeof(ValidationAttribute), false))
                {
                    if (attribute.IsValid(pi.GetValue(entity, null))) continue;
                    if (!brokenrules.ContainsKey(pi.Name))
                        brokenrules.Add(pi.Name, new List<string>());
                    brokenrules[pi.Name].Add(attribute.FormatErrorMessage(pi.Name));
                }

            return new ValidationContainer<T,TA>(brokenrules, entity);
        }
    }
}
